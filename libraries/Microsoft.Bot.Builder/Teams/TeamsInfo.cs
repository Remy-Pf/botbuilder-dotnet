﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Connector.Teams;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Teams
{
    public static class TeamsInfo
    {
        public static async Task<TeamDetails> GetTeamDetailsAsync(ITurnContext turnContext, string teamId = null, CancellationToken cancellationToken = default)
        {
            var t = teamId ?? turnContext.Activity.TeamsGetTeamInfo()?.Id ?? throw new InvalidOperationException("This method is only valid within the scope of MS Teams Team.");
#pragma warning disable CA2000 // Dispose objects before losing scope (we need to review this, disposing the connectorClient may have unintended consequences)
            return await GetTeamsConnectorClient(turnContext).Teams.FetchTeamDetailsAsync(t, cancellationToken).ConfigureAwait(false);
#pragma warning restore CA2000 // Dispose objects before losing scope
        }

        public static async Task<IList<ChannelInfo>> GetTeamChannelsAsync(ITurnContext turnContext, string teamId = null, CancellationToken cancellationToken = default)
        {
            var t = teamId ?? turnContext.Activity.TeamsGetTeamInfo()?.Id ?? throw new InvalidOperationException("This method is only valid within the scope of MS Teams Team.");
#pragma warning disable CA2000 // Dispose objects before losing scope (we need to review this, disposing the connectorClient may have unintended consequences)
            var channelList = await GetTeamsConnectorClient(turnContext).Teams.FetchChannelListAsync(t, cancellationToken).ConfigureAwait(false);
#pragma warning restore CA2000 // Dispose objects before losing scope
            return channelList.Conversations;
        }

        public static Task<IEnumerable<TeamsChannelAccount>> GetTeamMembersAsync(ITurnContext turnContext, string teamId = null, CancellationToken cancellationToken = default)
        {
            var t = teamId ?? turnContext.Activity.TeamsGetTeamInfo()?.Id ?? throw new InvalidOperationException("This method is only valid within the scope of MS Teams Team.");
            return GetMembersAsync(GetConnectorClient(turnContext), t, cancellationToken);
        }

        public static Task<IEnumerable<TeamsChannelAccount>> GetMembersAsync(ITurnContext turnContext, CancellationToken cancellationToken = default)
        {
            var teamInfo = turnContext.Activity.TeamsGetTeamInfo();

            if (teamInfo?.Id != null)
            {
                return GetTeamMembersAsync(turnContext, teamInfo.Id, cancellationToken);
            }
            else
            {
                var conversationId = turnContext.Activity?.Conversation?.Id;
                return GetMembersAsync(GetConnectorClient(turnContext), conversationId, cancellationToken);
            }
        }

        public static Task<TeamsPagedMembersResult> GetPagedTeamMembersAsync(ITurnContext turnContext, string teamId = null, string continuationToken = default(string), int? pageSize = default(int?), CancellationToken cancellationToken = default)
        {
            var t = teamId ?? turnContext.Activity.TeamsGetTeamInfo()?.Id ?? throw new InvalidOperationException("This method is only valid within the scope of MS Teams Team.");
            return GetPagedMembersAsync(GetConnectorClient(turnContext), t, continuationToken, cancellationToken, pageSize);
        }

        public static Task<TeamsPagedMembersResult> GetPagedMembersAsync(ITurnContext turnContext, int? pageSize = default(int?), string continuationToken = default(string), CancellationToken cancellationToken = default)
        {
            var teamInfo = turnContext.Activity.TeamsGetTeamInfo();

            if (teamInfo?.Id != null)
            {
                return GetPagedTeamMembersAsync(turnContext, teamInfo.Id, continuationToken, pageSize, cancellationToken);
            }
            else
            {
                var conversationId = turnContext.Activity?.Conversation?.Id;
                return GetPagedMembersAsync(GetConnectorClient(turnContext), conversationId, continuationToken, cancellationToken, pageSize);
            }
        }

        public static Task<TeamsChannelAccount> GetTeamMemberAsync(ITurnContext turnContext, string userId, string teamId = null, CancellationToken cancellationToken = default)
        {
            var t = teamId ?? turnContext.Activity.TeamsGetTeamInfo()?.Id ?? throw new InvalidOperationException("This method is only valid within the scope of MS Teams Team.");
            return GetMemberAsync(GetConnectorClient(turnContext), userId, t, cancellationToken);
        }

        public static Task<TeamsChannelAccount> GetMemberAsync(ITurnContext turnContext, string userId, CancellationToken cancellationToken = default)
        {
            var teamInfo = turnContext.Activity.TeamsGetTeamInfo();

            if (teamInfo?.Id != null)
            {
                return GetTeamMemberAsync(turnContext, userId, teamInfo.Id, cancellationToken);
            }
            else
            {
                var conversationId = turnContext.Activity?.Conversation?.Id;
                return GetMemberAsync(GetConnectorClient(turnContext), userId, conversationId, cancellationToken);
            }
        }

        public static async Task<Tuple<ConversationReference, string>> SendMessageToTeamsChannelAsync(ITurnContext turnContext, IActivity activity, string teamsChannelId, MicrosoftAppCredentials credentials, CancellationToken cancellationToken = default)
        {
            if (turnContext == null)
            {
                throw new ArgumentNullException(nameof(turnContext));
            }

            if (turnContext.Activity == null)
            {
                throw new InvalidOperationException(nameof(turnContext.Activity));
            }

            if (string.IsNullOrEmpty(teamsChannelId))
            {
                throw new ArgumentNullException(nameof(teamsChannelId));
            }

            if (credentials == null)
            {
                throw new ArgumentNullException(nameof(credentials));
            }

            ConversationReference conversationReference = null;
            var newActivityId = string.Empty;
            var serviceUrl = turnContext.Activity.ServiceUrl;
            var conversationParameters = new ConversationParameters
            {
                IsGroup = true,
                ChannelData = new { channel = new { id = teamsChannelId } },
                Activity = (Activity)activity,
            };

            await ((BotFrameworkAdapter)turnContext.Adapter).CreateConversationAsync(
                teamsChannelId,
                serviceUrl,
                credentials,
                conversationParameters,
                (t, ct) =>
                {
                    conversationReference = t.Activity.GetConversationReference();
                    newActivityId = t.Activity.Id;
                    return Task.CompletedTask;
                },
                cancellationToken).ConfigureAwait(false);

            return new Tuple<ConversationReference, string>(conversationReference, newActivityId);
        }

        private static async Task<IEnumerable<TeamsChannelAccount>> GetMembersAsync(IConnectorClient connectorClient, string conversationId, CancellationToken cancellationToken)
        {
            if (conversationId == null)
            {
                throw new InvalidOperationException("The GetMembers operation needs a valid conversation Id.");
            }

            var teamMembers = await connectorClient.Conversations.GetConversationMembersAsync(conversationId, cancellationToken).ConfigureAwait(false);
            var teamsChannelAccounts = teamMembers.Select(channelAccount => JObject.FromObject(channelAccount).ToObject<TeamsChannelAccount>());
            return teamsChannelAccounts;
        }

        private static IConnectorClient GetConnectorClient(ITurnContext turnContext)
        {
            return turnContext.TurnState.Get<IConnectorClient>() ?? throw new InvalidOperationException("This method requires a connector client.");
        }

        private static async Task<TeamsChannelAccount> GetMemberAsync(IConnectorClient connectorClient, string userId, string conversationId, CancellationToken cancellationToken)
        {
            if (conversationId == null)
            {
                throw new InvalidOperationException("The GetMembers operation needs a valid conversation Id.");
            }

            if (userId == null)
            {
                throw new InvalidOperationException("The GetMembers operation needs a valid user Id.");
            }

            var teamMember = await ((Conversations)connectorClient.Conversations).GetConversationMemberAsync(userId, conversationId, cancellationToken).ConfigureAwait(false);
            var teamsChannelAccount = JObject.FromObject(teamMember).ToObject<TeamsChannelAccount>();
            return teamsChannelAccount;
        }

        private static async Task<TeamsPagedMembersResult> GetPagedMembersAsync(IConnectorClient connectorClient, string conversationId, string continuationToken, CancellationToken cancellationToken, int? pageSize = default(int?))
        {
            if (conversationId == null)
            {
                throw new InvalidOperationException("The GetMembers operation needs a valid conversation Id.");
            }

            var pagedMemberResults = await connectorClient.Conversations.GetConversationPagedMembersAsync(conversationId, pageSize, continuationToken, cancellationToken).ConfigureAwait(false);
            var teamsPagedMemberResults = new TeamsPagedMembersResult(pagedMemberResults.ContinuationToken, pagedMemberResults.Members);
            return teamsPagedMemberResults;
        }

        private static ITeamsConnectorClient GetTeamsConnectorClient(ITurnContext turnContext)
        {
            var connectorClient = GetConnectorClient(turnContext);
            if (connectorClient is ConnectorClient connectorClientImpl)
            {
                return new TeamsConnectorClient(connectorClientImpl.BaseUri, connectorClientImpl.Credentials, connectorClientImpl.HttpClient);
            }
            else
            {
                return new TeamsConnectorClient(connectorClient.BaseUri, connectorClient.Credentials);
            }
        }
    }
}
