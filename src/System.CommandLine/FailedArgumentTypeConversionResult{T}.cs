// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine
{
    public class FailedArgumentTypeConversionResult<T> : FailedArgumentParseResult
    {
        // TODO: (FailedArgumentTypeConversionResult) localize
        public FailedArgumentTypeConversionResult(string value) : 
            base($"Cannot parse argument '{value}' as {typeof(T)}")
        {
        }
    }
}
