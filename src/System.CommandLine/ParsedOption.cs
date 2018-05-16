// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine
{
    public class ParsedOption : ParsedSymbol
    {
        public ParsedOption(OptionDefinition optionDefinition, string token = null, ParsedCommand parent = null) :
            base(optionDefinition, token ?? optionDefinition?.ToString(), parent)
        {
        }

        public override ParsedSymbol TryTakeToken(Token token) =>
            TryTakeArgument(token);

        protected internal override ParseError Validate()
        {
            if (Arguments.Count > 1 &&
                SymbolDefinition.ArgumentDefinition.Parser.ArgumentArity != ArgumentArity.Many)
            {
                // TODO: (Validate) localize
                return new ParseError(
                    $"Option '{SymbolDefinition}' cannot be specified more than once.",
                    this,
                    false);
            }

            return base.Validate();
        }
    }
}
