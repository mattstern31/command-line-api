// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine
{
    public class ArgumentArityValidator : ISymbolValidator
    {
        public ArgumentArityValidator(int minimumNumberOfArguments, int maximumNumberOfArguments)
        {
            if (minimumNumberOfArguments < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(minimumNumberOfArguments));
            }

            if (maximumNumberOfArguments < minimumNumberOfArguments)
            {
                throw new ArgumentException($"{nameof(maximumNumberOfArguments)} must be greater than or equal to {nameof(minimumNumberOfArguments)}");
            }

            MinimumNumberOfArguments = minimumNumberOfArguments;
            MaximumNumberOfArguments = maximumNumberOfArguments;
        }

        public int MinimumNumberOfArguments { get; }

        public int MaximumNumberOfArguments { get; }

        public string Validate(Symbol symbol)
        {
            if (symbol.Arguments.Count < MinimumNumberOfArguments)
            {
                switch (symbol)
                {
                    case Option option:
                        return symbol.ValidationMessages.RequiredArgumentMissingForOption(option.Definition);
                    case Command command:
                        return symbol.ValidationMessages.RequiredArgumentMissingForCommand(command.Definition);
                }
            }

            if (symbol.Arguments.Count > MaximumNumberOfArguments)
            {
                if (MaximumNumberOfArguments == 1)
                {
                    switch (symbol)
                    {
                        case Command command:
                            return command.ValidationMessages.CommandExpectsOneArgument(command.Definition, command.Arguments.Count);
                        case Option option:
                            return symbol.ValidationMessages.OptionExpectsOneArgument(option.Definition, option.Arguments.Count);
                    }
                }
                else
                {
                    // FIX: (Validate)
                    return $"Too many arguments for {symbol}";
                }
            }

            return null;
        }
    }
}
