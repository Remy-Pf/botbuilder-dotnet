﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AdaptiveExpressions.BuiltinFunctions
{
    public class Json : ExpressionEvaluator
    {
        public Json()
            : base(ExpressionType.Json, Evaluator(), ReturnType.Object, Validator)
        {
        }

        private static EvaluateExpressionDelegate Evaluator()
        {
            return FunctionUtils.Apply(
                        args =>
                        {
                            using (var textReader = new StringReader(args[0].ToString()))
                            {
                                using (var jsonReader = new JsonTextReader(textReader) { DateParseHandling = DateParseHandling.None })
                                {
                                    return JToken.ReadFrom(jsonReader);
                                }
                            }
                        });
        }

        private static void Validator(Expression expression)
        {
            FunctionUtils.ValidateOrder(expression, null, ReturnType.String);
        }
    }
}
