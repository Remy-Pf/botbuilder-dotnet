﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Text.RegularExpressions;

namespace AdaptiveExpressions.BuiltinFunctions
{
    public class ReplaceIgnoreCase : ExpressionEvaluator
    {
        public ReplaceIgnoreCase()
            : base(ExpressionType.ReplaceIgnoreCase, Evaluator(), ReturnType.String, Validator)
        {
        }

        private static EvaluateExpressionDelegate Evaluator()
        {
            return FunctionUtils.ApplyWithError(
                        args =>
                        {
                            string error = null;
                            string result = null;
                            string inputStr = FunctionUtils.ParseStringOrNull(args[0]);
                            string oldStr = FunctionUtils.ParseStringOrNull(args[1]);
                            if (oldStr.Length == 0)
                            {
                                error = $"{args[1]} the oldValue in replace function should be a string with at least length 1.";
                            }

                            string newStr = FunctionUtils.ParseStringOrNull(args[2]);
                            if (error == null)
                            {
                                result = Regex.Replace(inputStr, oldStr, newStr, RegexOptions.IgnoreCase);
                            }

                            return (result, error);
                        }, FunctionUtils.VerifyStringOrNull);
        }

        private static void Validator(Expression expression)
        {
            FunctionUtils.ValidateArityAndAnyType(expression, 3, 3, ReturnType.String);
        }
    }
}
