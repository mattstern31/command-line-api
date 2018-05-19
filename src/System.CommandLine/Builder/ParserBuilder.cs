// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace System.CommandLine.Builder
{
    public class ParserBuilder : CommandDefinitionBuilder
    {
        private static readonly Lazy<string> executableName =
            new Lazy<string>(() => Path.GetFileNameWithoutExtension(Assembly.GetEntryAssembly().Location));

        private List<Invocation> _invocationList;

        public ParserBuilder() : base(executableName.Value)
        {
        }

        public static string ExeName { get; } = executableName.Value;

        public bool EnablePosixBundling { get; set; } = true;

        public ResponseFileHandling ResponseFileHandling { get; set; }

        public Parser Build()
        {
            return new Parser(
                new ParserConfiguration(
                    BuildChildSymbolDefinitions(),
                    allowUnbundling: EnablePosixBundling,
                    validationMessages: ValidationMessages.Instance,
                    responseFileHandling: ResponseFileHandling,
                    invocationList: _invocationList));
        }

        public override CommandDefinition BuildCommandDefinition()
        {
            if (CommandDefinitionBuilders?.Count == 1)
            {
                return CommandDefinitionBuilders.Single().BuildCommandDefinition();
            }

            return CommandDefinition.CreateImplicitRootCommand(BuildChildSymbolDefinitions().ToArray());
        }

        internal void AddInvocation(Invocation action)
        {
            if (_invocationList == null)
            {
                _invocationList = new List<Invocation>();
            }

            _invocationList.Add(action);
        }
    }
}
