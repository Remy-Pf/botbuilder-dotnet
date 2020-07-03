// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;

namespace AdaptiveExpressions.BuiltinFunctions
{
    public class Base64ToBinary : ExpressionEvaluator
    {
        public Base64ToBinary()
            : base(ExpressionType.Base64ToBinary, Evaluator(), ReturnType.Object, FunctionUtils.ValidateUnary)
        {
        }

        private static EvaluateExpressionDelegate Evaluator()
        {
            return FunctionUtils.Apply(args => Convert.FromBase64String(args[0].ToString()), FunctionUtils.VerifyString);
        }
    }
}
