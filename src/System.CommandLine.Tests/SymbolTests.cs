// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Builder;
using FluentAssertions;
using Xunit;

namespace System.CommandLine.Tests
{
    public class SymbolTests
    {
        [Fact]
        public void An_option_with_a_default_argument_value_is_valid_without_having_the_argument_supplied()
        {
            var definition = new OptionDefinition(
                "-x",
                "",
                argumentDefinition: new ArgumentDefinitionBuilder()
                    .FromAmong("one", "two", "default")
                    .WithDefaultValue(() => "default")
                    .ExactlyOne());

            var option = new Option(definition, "-x");

            option.Arguments.Should().BeEquivalentTo("default");
        }

        [Fact]
        public void Default_values_are_reevaluated_and_not_cached_between_parses()
        {
            var i = 0;
            var definition =
                new OptionDefinition(
                    "-x",
                    "",
                    argumentDefinition: new ArgumentDefinitionBuilder()
                        .WithDefaultValue(() => (++i).ToString())
                        .ExactlyOne());

            var result1 = definition.Parse("-x");
            var result2 = definition.Parse("-x");

            result1["x"].GetValueOrDefault().Should().Be("1");
            result2["x"].GetValueOrDefault().Should().Be("2");
        }

        [Fact]
        public void HasOption_can_be_used_to_check_the_presence_of_an_option()
        {
            var definition = new CommandDefinition("the-command", "", new[] {
                new OptionDefinition(
                    new[] {"-h", "--help"}, "")
            });

            var result = definition.Parse("the-command -h");

            result.HasOption("help").Should().BeTrue();
        }


        [Fact]
        public void Result_returns_single_string_default_value_when_no_argument_is_provided()
        {
            var definition = new OptionDefinition(
                "-x",
                "",
                argumentDefinition: new ArgumentDefinitionBuilder()
                    .WithDefaultValue(() => "default")
                    .ExactlyOne());

            var option = new Option(definition);

            option.Result.Should().BeOfType<SuccessfulArgumentParseResult<string>>()
                .Which.Value.Should().Be("default");
        }

        [Fact]
        public void Result_returns_IEnumerable_containing_string_default_value_when_no_argument_is_provided()
        {
            var definition = new OptionDefinition(
                "-x",
                "",
                argumentDefinition: new ArgumentDefinitionBuilder()
                    .WithDefaultValue(() => "default")
                    .OneOrMore());

            var option = new Option(definition);

            option.Result.Should().BeOfType<SuccessfulArgumentParseResult<IReadOnlyCollection<string>>>()
                .Which.Value.Should().BeEquivalentTo("default");
        }

        [Fact]
        public void Command_will_not_accept_a_command_if_a_sibling_command_has_already_been_accepted()
        {
            var definition = new CommandDefinition("outer", "", new[] {
                new CommandDefinition("inner-one", "", ArgumentDefinition.None),
                new CommandDefinition("inner-two", "", ArgumentDefinition.None)
            });

            var result = new Parser(definition).Parse("outer inner-one inner-two");

            result.Command.Name.Should().Be("inner-one");
            result.Errors.Count.Should().Be(1);

            var result2 = new Parser(definition).Parse("outer inner-two inner-one");

            result2.Command.Name.Should().Be("inner-two");
            result2.Errors.Count.Should().Be(1);
        }

        [Fact]
        public void ValueForOption_throws_with_empty_alias()
        {
            var definition = new CommandDefinition("one", "");

            var command = new Command(definition);

            Action action = () => { command.ValueForOption<string>(string.Empty); };

            action.Should().Throw<ArgumentException>("Value cannot be null or whitespace.");
        }

        [Fact]
        public void HasAlias_returns_true_if_option_has_an_alias()
        {
            var option = new Option(new OptionDefinition(new []{"o", "one"}, ""));

            option.HasAlias("one").Should().BeTrue();
        }
    }
}

