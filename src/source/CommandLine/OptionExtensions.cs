// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.DotNet.Cli.CommandLine
{
    public static class OptionExtensions
    {
        public static bool IsHidden(this Option option) =>
            string.IsNullOrWhiteSpace(option.Description);

        internal static IEnumerable<Parsed> AllOptions(
            this Parsed option)
        {
            if (option == null)
            {
                throw new ArgumentNullException(nameof(option));
            }

            yield return option;

            foreach (var item in option.ParsedOptions.FlattenBreadthFirst(o => o.ParsedOptions))
            {
                yield return item;
            }
        }

        public static OptionParseResult Parse(
            this Option option,
            string commandLine,
            char[] delimiters = null) =>
            new OptionParser(new ParserConfiguration(argumentDelimiters: delimiters, definedOptions: new[] { option })).Parse(commandLine);

        public static CommandParseResult Parse(
            this Command command,
            params string[] args) =>
            new CommandParser(command).Parse(args);

        public static CommandParseResult Parse(
            this Command command,
            string commandLine,
            char[] delimiters = null) =>
            new CommandParser(command).Parse(commandLine);
    }
}
