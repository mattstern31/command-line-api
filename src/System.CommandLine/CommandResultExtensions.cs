﻿// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Binding;
using System.Linq;

namespace System.CommandLine
{
    public static class CommandResultExtensions
    {
        [Obsolete("Use GetArgumentValueOrDefault instead. This method will be removed in a future version.")]
        public static object GetValueOrDefault(this CommandResult commandResult)
        {
            return commandResult.GetValueOrDefault<object>();
        }

        [Obsolete("Use GetArgumentValueOrDefault instead. This method will be removed in a future version.")]
        public static T GetValueOrDefault<T>(this CommandResult commandResult)
        {
            var argumentResult = commandResult.ArgumentResults
                                              .SingleOrDefault() ??
                                 ArgumentResult.None();

            return argumentResult
                   .GetValueAs(typeof(T))
                   .GetValueOrDefault<T>();
        }

        public static object GetArgumentValueOrDefault(
            this CommandResult commandResult,
            string argumentName)
        {
            return commandResult.GetArgumentValueOrDefault<object>(argumentName);
        }

        public static T GetArgumentValueOrDefault<T>(
            this CommandResult commandResult,
            string argumentName)
        {
            var argumentResult =
                commandResult.ArgumentResults
                             .SingleOrDefault(r => r.Argument.Name == argumentName);

            return argumentResult.GetValueOrDefault<T>();
        }

        internal static bool TryGetValueForOption(
            this CommandResult commandResult, 
            IValueDescriptor valueDescriptor, 
            out object value)
        {
            var children = commandResult.Children
                                        .Where(o => valueDescriptor.ValueName.IsMatch(o.Symbol))
                                        .ToArray();

            SymbolResult symbolResult = null;

            if (children.Length > 1)
            {
                throw new ArgumentException($"Ambiguous match while trying to bind parameter {valueDescriptor.ValueName} among: {String.Join(",", children.Select(o => o.Name))}");
            }

            if (children.Length == 1)
            {
                symbolResult = children[0];
            }

            if (symbolResult is OptionResult optionResult)
            {
                if (optionResult.GetValueAs(valueDescriptor.Type) is SuccessfulArgumentResult successful)
                {
                    value = successful.Value;
                    return true;
                }
            }

            value = null;
            return false;
        }
    }
}
