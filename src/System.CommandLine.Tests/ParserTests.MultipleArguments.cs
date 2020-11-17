﻿// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Parsing;
using System.CommandLine.Tests.Utility;
using FluentAssertions;
using FluentAssertions.Execution;
using Xunit;

namespace System.CommandLine.Tests
{
    public partial class ParserTests
    {
        public class MultipleArguments
        {
            [Fact]
            public void Multiple_arguments_can_differ_by_arity()
            {
                var command = new Command("the-command")
                {
                    new Argument<string>
                    {
                        Arity = new ArgumentArity(3, 3),
                        Name = "several"
                    },
                    new Argument<string>
                    {
                        Arity = ArgumentArity.ZeroOrMore,
                        Name = "one"
                    }
                };

                var result = command.Parse("1 2 3 4");

                var several = result.CommandResult
                                    .GetArgumentValueOrDefault<IEnumerable<string>>("several");

                var one = result.CommandResult
                                .GetArgumentValueOrDefault<IEnumerable<string>>("one");

                several.Should()
                       .BeEquivalentSequenceTo("1", "2", "3");
                one.Should()
                       .BeEquivalentSequenceTo("4");
            }

            [Fact]
            public void Multiple_arguments_can_differ_by_type()
            {
                var command = new Command("the-command")
                {
                    new Argument<string>
                    {
                        Name = "the-string"
                    },
                    new Argument<int>
                    {
                        Name = "the-int"
                    }
                };

                var result = command.Parse("1 2");

                var theString = result.CommandResult
                                    .GetArgumentValueOrDefault<string>("the-string");

                var theInt = result.CommandResult
                                .GetArgumentValueOrDefault<int>("the-int");

                theString.Should().Be("1");
                theInt.Should().Be(2);
            }

            [Theory]
            [InlineData("--verbose one two three four five")]
            [InlineData("one --verbose two three four five")]
            [InlineData("one two --verbose three four five")]
            [InlineData("one two three --verbose four five")]
            [InlineData("one two three four --verbose five")]
            [InlineData("one two three four five --verbose")]
            [InlineData("--verbose true one two three four five")]
            [InlineData("one --verbose true two three four five")]
            [InlineData("one two --verbose true three four five")]
            [InlineData("one two three --verbose true four five")]
            [InlineData("one two three four --verbose true five")]
            [InlineData("one two three four five --verbose true")]
            public void When_multiple_arguments_are_present_then_their_order_relative_to_sibling_options_is_not_significant(string commandLine)
            {
                var command = new Command("the-command")
                {
                    new Argument<string> { Name = "first" },
                    new Argument<string> { Name = "second" },
                    new Argument<string[]> { Name = "third" },
                    new Option("--verbose")
                    {
                        Argument = new Argument<bool>()
                    }
                };

                var parseResult = command.Parse(commandLine);

                parseResult
                    .ValueForArgument("first")
                    .Should()
                    .Be("one");

                parseResult
                    .ValueForArgument<string>("second")
                    .Should()
                    .Be("two");

                parseResult
                    .ValueForArgument<string[]>("third")
                    .Should()
                    .BeEquivalentSequenceTo("three", "four", "five");

                parseResult
                    .ValueForOption<bool>("--verbose")
                    .Should()
                    .BeTrue();
            }

            [Fact]
            public void Multiple_arguments_of_unspecified_type_are_parsed_correctly()
            {
                var root = new RootCommand
                {
                    new Argument("source")
                    {
                        Arity = ArgumentArity.ExactlyOne
                    },
                    new Argument("destination")
                    {
                        Arity = ArgumentArity.ExactlyOne
                    }
                };

                var result = root.Parse("src.txt dest.txt");

                result.RootCommandResult
                      .GetArgumentValueOrDefault("source")
                      .Should()
                      .Be("src.txt");
                
                result.RootCommandResult
                      .GetArgumentValueOrDefault("destination")
                      .Should()
                      .Be("dest.txt");
            }

            [Fact]
            public void arity_ambiguities_can_be_differentiated_by_type_convertibility()
            {
                var ints = new Argument<int[]>();
                var strings = new Argument<string[]>();

                var root = new RootCommand
                {
                    ints,
                    strings
                };

                var result = root.Parse("1 2 3 one", "two");

                var _ = new AssertionScope();

                result.ValueForArgument(ints)
                      .Should()
                      .BeEquivalentTo(new[] { 1, 2, 3 },
                                      options => options.WithStrictOrdering());

                result.ValueForArgument(strings)
                      .Should()
                      .BeEquivalentTo(new[] { "one", "two" },
                                      options => options.WithStrictOrdering());
            }

            [Theory]
            [InlineData(true)]
            [InlineData(false)]
            public void Multiple_arguments_can_be_specified_after_option(bool allowMultipleArguments)
            {
                var option = new Option<string[]>("--option") { AllowMultipleArgumentsPerOptionFlag = allowMultipleArguments };
                var command = new Command("the-command") { option };

                var result = command.Parse("--option 1 2");

                var optionResult = result.ValueForOption(option);

                optionResult.Should().BeEquivalentTo(allowMultipleArguments ? new[] { "1" , "2"} : new[] { "1" });
            }

            [Theory]
            [InlineData(true)]
            [InlineData(false)]
            public void Multiple_arguments_can_be_specified_after_multiple_options(bool allowMultipleArguments)
            {
                var option = new Option<string[]>("--option") { AllowMultipleArgumentsPerOptionFlag = allowMultipleArguments };
                var command = new Command("the-command") { option };

                var result = command.Parse("--option 1 --option 2");

                var optionResult = result.ValueForOption(option);

                optionResult.Should().BeEquivalentTo(new[] { "1", "2" });
            }
        }
    }
}
