﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;

namespace AdaptiveExpressions.BuiltinFunctions
{
    public class Bool : ComparisonEvaluator
    {
        public Bool()
            : base(
                  ExpressionType.Bool,
                  Function,
                  FunctionUtils.ValidateUnary)
        {
        }

        private static bool Function(IReadOnlyList<object> args)
        {
            return FunctionUtils.IsLogicTrue(args[0]);
        }
    }
}
