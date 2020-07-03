// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Linq;

namespace AdaptiveExpressions.BuiltinFunctions
{
    public class Intersection : ExpressionEvaluator
    {
        public Intersection()
            : base(ExpressionType.Intersection, Evaluator(), ReturnType.Array, Validator)
        {
        }

        private static EvaluateExpressionDelegate Evaluator()
        {
            return FunctionUtils.Apply(
                        args =>
                        {
                            var result = (IEnumerable<object>)args[0];
                            for (var i = 1; i < args.Count; i++)
                            {
                                var nextItem = (IEnumerable<object>)args[i];
                                result = result.Intersect(nextItem);
                            }

                            return result.ToList();
                        }, FunctionUtils.VerifyList);
        }

        private static void Validator(Expression expression)
        {
            FunctionUtils.ValidateArityAndAnyType(expression, 1, int.MaxValue, ReturnType.Array);
        }
    }
}
