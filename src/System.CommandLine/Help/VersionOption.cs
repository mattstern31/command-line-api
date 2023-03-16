﻿// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Invocation;
using System.CommandLine.IO;
using System.CommandLine.Parsing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace System.CommandLine.Help
{
    internal class VersionOption : Option<bool>
    {
        internal VersionOption()
            : base("--version", new Argument<bool>("--version") { Arity = ArgumentArity.Zero })
        {
            Description = LocalizationResources.VersionOptionDescription();
            Action = new VersionOptionAction();
            AddValidators();
        }

        internal VersionOption(string name, string[] aliases)
            : base(name, aliases)
        {
            Description = LocalizationResources.VersionOptionDescription();
            Action = new VersionOptionAction();
            AddValidators();
        }

        private void AddValidators()
        {
            Validators.Add(static result =>
            {
                if (result.Parent is CommandResult parent &&
                    parent.Children.Where(r => !(r is OptionResult optionResult && optionResult.Option is VersionOption))
                          .Any(IsNotImplicit))
                {
                    result.AddError(LocalizationResources.VersionOptionCannotBeCombinedWithOtherArguments(result.Token?.Value ?? result.Option.Name));
                }
            });
        }

        private static bool IsNotImplicit(SymbolResult symbolResult)
        {
            return symbolResult switch
            {
                ArgumentResult argumentResult => !argumentResult.IsImplicit,
                OptionResult optionResult => !optionResult.IsImplicit,
                _ => true
            };
        }

        internal override bool IsGreedy => false;

        public override bool Equals(object? obj) => obj is VersionOption;

        public override int GetHashCode() => typeof(VersionOption).GetHashCode();

        private sealed class VersionOptionAction : CliAction
        {
            public override int Invoke(InvocationContext context)
            {
                context.Console.Out.WriteLine(RootCommand.ExecutableVersion);
                return 0;
            }

            public override Task<int> InvokeAsync(InvocationContext context, CancellationToken cancellationToken = default)
                => cancellationToken.IsCancellationRequested
                    ? Task.FromCanceled<int>(cancellationToken)
                    : Task.FromResult(Invoke(context));
        }
    }
}