// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace AdaptiveExpressions.BuiltinFunctions
{
    public class Length : ExpressionEvaluator
    {
        public Length()
            : base(ExpressionType.Length, Evaluator(), ReturnType.Number, FunctionUtils.ValidateUnaryString)
        {
        }

        private static EvaluateExpressionDelegate Evaluator()
        {
            return FunctionUtils.Apply(
                        args =>
                        {
                            var result = 0;
                            if (args[0] is string str)
                            {
                                result = str.Length;
                            }
                            else
                            {
                                result = 0;
                            }

                            return result;
                        }, FunctionUtils.VerifyStringOrNull);
        }
    }
}
