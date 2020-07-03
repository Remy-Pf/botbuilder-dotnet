﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;

namespace AdaptiveExpressions.BuiltinFunctions
{
    public class Floor : NumberTransformEvaluator
    {
        public Floor()
            : base(ExpressionType.Floor, Function)
        {
        }

        private static object Function(IReadOnlyList<object> args)
        {
            return Math.Floor(Convert.ToDouble(args[0]));
        }
    }
}
