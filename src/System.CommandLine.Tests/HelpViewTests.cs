// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Builder;
using FluentAssertions;
using System.Linq;
using Xunit;
using Xunit.Abstractions;
using static System.Environment;

namespace System.CommandLine.Tests
{
    public class HelpViewTests
    {
        private readonly ITestOutputHelper _output;
        private const int ColumnGutterWidth = 4;
        private const int IndentationWidth = 2;
        private readonly string _columnPadding;
        private readonly string _indentation;

        public HelpViewTests(ITestOutputHelper output)
        {
            _output = output;
            _columnPadding = new string(' ', ColumnGutterWidth);
            _indentation = new string(' ', IndentationWidth);
        }

        #region " Setup "

        [Fact]
        public void An_argument_is_not_hidden_from_help_if_no_help_is_provided()
        {
            var command = new ParserBuilder()
                .AddCommand("outer", "HelpDefinition text for the outer command",
                    arguments: args => args.ExactlyOne())
                .BuildCommandDefinition();

            var help = command.Subcommand("outer").GenerateHelp();

            help.Should().Contain($"Arguments:{NewLine}  <>");
        }

        [Fact]
        public void An_argument_shows_help_if_help_is_provided()
        {
            var command = new ParserBuilder()
                .AddCommand("outer", "HelpDefinition text for the outer command",
                    arguments: args => args.WithHelp("test name", "test desc").ExactlyOne())
                .BuildCommandDefinition();

            var help = command.Subcommand("outer").GenerateHelp();

            help.Should().Contain($"Arguments:{NewLine}{_indentation}<test name>{_columnPadding}test desc");
        }

        [Fact]
        public void An_argument_shows_no_help_if_help_is_hidden()
        {
            var command = new ParserBuilder()
                .AddCommand("outer", "HelpDefinition text for the outer command",
                    arguments: args => args.WithHelp("test name", "test desc", true).ExactlyOne())
                .BuildCommandDefinition();

            var help = command.GenerateHelp();

            help.Should().NotContain("test desc");
        }

        [Fact]
        public void An_option_is_not_hidden_from_help_output_if_its_description_is_empty()
        {
            var command = new ParserBuilder()
                .AddCommand("the-command", "Does things.",
                    cmd => cmd.AddOption("-x", "")
                        .AddOption("-n", "Not hidden"))
                .BuildCommandDefinition();

            var help = command.Subcommand("the-command").GenerateHelp();

            help.Should().Contain("-x");
            help.Should().Contain("-n");
        }

        [Fact]
        public void An_option_is_hidden_from_help_output_if_it_is_flagged_as_hidden()
        {
            var command = new ParserBuilder()
                .AddCommand("the-command", "Does things.",
                    cmd => cmd.AddOption("-x", "Is Hidden", opt => opt.WithHelp(isHidden: true))
                        .AddOption("-n", "Not hidden"))
                .BuildCommandDefinition();

            var help = command.Subcommand("the-command").GenerateHelp();

            help.Should().Contain("-n");
            help.Should().NotContain("-x");
        }

        #endregion " Setup "

        #region " Format "

        [Fact]
        public void Help_view_wraps_with_aligned_column_when_help_text_contains_newline()
        {
            var command = new ParserBuilder()
                          .AddCommand("the-command",
                                      "command help",
                                      cmd => cmd.AddOption(
                                          new[] { "-v", "--verbosity" },
                                          $"Sets the verbosity. Accepted values are: [quiet] [loud] [very-loud]",
                                          arguments: args => args.ExactlyOne()))
                          .BuildCommandDefinition();

            var help = command.Subcommand("the-command").GenerateHelp();

            const string indent = "                        ";

            help.Should().Contain($"Sets the verbosity. Accepted values are: [quiet] [loud]{NewLine}{indent}[very-loud]");
        }

        [Fact]
        public void Column_for_argument_descriptions_are_vertically_aligned()
        {
            var command = new ParserBuilder()
                          .AddCommand(
                              "outer", "HelpDefinition text for the outer command",
                              arguments: args => args.WithHelp(name: "outer-command-arg",
                                                               description: "The argument for the inner command")
                                                     .ExactlyOne(),
                              symbols: outer => outer.AddCommand(
                                  "inner", "HelpDefinition text for the inner command",
                                  arguments: innerArgs => innerArgs.WithHelp(name: "the-inner-command-arg",
                                                                             description: "The argument for the inner command")
                                                                   .ExactlyOne()))
                          .BuildCommandDefinition();

            var help = command.Subcommand("outer").Subcommand("inner").GenerateHelp();

            _output.WriteLine(help);

            var lines = help.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            var optionA = lines.Last(line => line.Contains("outer-command-arg"));
            var optionB = lines.Last(line => line.Contains("the-inner-command-arg"));

            optionA.IndexOf("The argument").Should().Be(optionB.IndexOf("The argument"));
        }

        [Fact]
        public void Column_for_options_descriptions_are_vertically_aligned()
        {
            var command = new ParserBuilder()
                          .AddCommand("the-command", "HelpDefinition text for the command",
                                      symbols =>
                                          symbols.AddOption(
                                                     new[] { "-a", "--aaa" },
                                                     "An option with 8 characters")
                                                 .AddOption(
                                                     new[] { "-b", "--bbbbbbbbbb" },
                                                     "An option with 15 characters"))
                          .BuildCommandDefinition();

            var help = command.Subcommand("the-command").GenerateHelp();

            var lines = help.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            var optionA = lines.Last(line => line.Contains("-a"));
            var optionB = lines.Last(line => line.Contains("-b"));

            optionA.IndexOf("An option").Should().Be(optionB.IndexOf("An option"));
        }

        #endregion " Format "

        #region " Relationships "

        [Fact]
        public void Command_help_view_includes_names_of_parent_commands()
        {
            var command = new ParserBuilder()
                          .AddCommand(
                              "outer", "the outer command",
                              outer => outer.AddCommand(
                                  "inner", "the inner command",
                                  inner => inner.AddCommand(
                                      "inner-er", "the inner-er command",
                                      innerEr => innerEr.AddOption(
                                          "--some-option",
                                          "some option"))))
                          .BuildCommandDefinition();

            var help = command.Subcommand("outer").Subcommand("inner").Subcommand("inner-er").GenerateHelp();

            help.Should().StartWith($"Usage: {ParserBuilder.ExeName} outer inner inner-er [options]");
        }

        [Fact]
        public void Command_help_view_does_not_include_names_of_sibling_commands()
        {
            var command = new ParserBuilder()
                          .AddCommand(
                              "outer", "outer description",
                              outer => {
                                  outer.AddCommand(
                                      "sibling", "sibling description");
                                  outer.AddCommand(
                                      "inner", "inner description",
                                      inner => inner.AddCommand(
                                          "inner-er", "inner-er description",
                                          innerEr => innerEr.AddOption(
                                              "some-option",
                                              "some-option description")));
                              })
                          .BuildCommandDefinition();

            var help = command.Subcommand("outer").Subcommand("inner").GenerateHelp();

            help.Should().NotContain("sibling");
        }

        [Fact]
        public void Command_help_view_does_not_include_names_of_child_commands_under_options_section()
        {
            var command = new ParserBuilder()
                          .AddCommand("outer", "description for outer",
                                      outer => outer.AddCommand("inner", "description for inner"))
                          .BuildCommandDefinition();

            var help = command.Subcommand("outer").GenerateHelp();

            _output.WriteLine(help);

            help.Should().NotContain("Options:");
        }

        #endregion " Relationships "

        #region " Synopsis "

        [Fact]
        public void When_a_command_accepts_arguments_then_the_synopsis_shows_them()
        {
            var command = new ParserBuilder()
                          .AddCommand("the-command", "command help",
                                      arguments: args => args
                                                         .WithHelp(name: "the-args")
                                                         .ZeroOrMore(),
                                      symbols: cmd => cmd.AddOption(
                                          new[] { "-v", "--verbosity" },
                                          "Sets the verbosity"))
                          .BuildCommandDefinition();

            var help = command.Subcommand("the-command").GenerateHelp();

            help.Should().StartWith($"Usage: { ParserBuilder.ExeName } the-command [options] <the-args>");
        }

        [Fact]
        public void When_a_command_and_subcommand_both_accept_arguments_then_the_synopsis_for_the_inner_command_shows_them()
        {
            var command = new ParserBuilder()
                          .AddCommand(
                              "outer-command", "command help",
                              arguments: outerArgs => outerArgs
                                  .WithHelp(name: "outer-args")
                                  .ZeroOrMore(),
                              symbols: outer => outer.AddCommand(
                                  "inner-command", "command help",
                                  arguments: args => args
                                      .WithHelp(name: "inner-args")
                                      .ZeroOrOne(),
                                  symbols: inner => inner.AddOption(
                                      "-v|--verbosity",
                                      "Sets the verbosity")))
                          .BuildCommandDefinition();

            var help = command.Subcommand("outer-command").Subcommand("inner-command").GenerateHelp();

            help.Should().StartWith($"Usage: { ParserBuilder.ExeName } outer-command <outer-args> inner-command [options] <inner-args>");
        }

        [Fact]
        public void When_a_command_does_not_accept_arguments_then_the_synopsis_does_not_show_them()
        {
            var command = new ParserBuilder()
                          .AddCommand("the-command", "command help",
                                      cmd => cmd.AddOption(
                                          new[] { "-v", "--verbosity" },
                                          "Sets the verbosity"))
                          .BuildCommandDefinition();

            var help = command.GenerateHelp();

            help.Should().NotContain("arguments");
        }

        [Fact]
        public void When_unmatched_tokens_are_allowed_then_help_view_indicates_it()
        {
            var command =
                new ParserBuilder()
                    .TreatUnmatchedTokensAsErrors(false)
                    .AddCommand("some-command", "Does something",
                        c => c.AddOption(
                            "-x",
                            "Indicates whether x"))
                    .BuildCommandDefinition();

            var help = command.Subcommand("some-command").GenerateHelp();

            help.Should().StartWith($"Usage: { ParserBuilder.ExeName } some-command [options] [[--] <additional arguments>...]]");
        }

        #endregion " Synopsis "

        #region " Arguments "

        [Fact]
        public void Argument_section_is_not_included_if_no_argumants()
        {
            var command = new ParserBuilder()
                .AddCommand("the-command", "command help")
                .BuildCommandDefinition();

            var help = command.GenerateHelp();

            help.Should().NotContain("Arguments");
        }

        [Fact]
        public void Argument_names_are_included_in_help_view()
        {
            var command = new ParserBuilder()
                          .AddCommand("the-command",
                                      "command help",
                                      cmd => cmd.AddOption(
                                          new[] { "-v", "--verbosity" },
                                          "Sets the verbosity.",
                                          arguments: args => args.WithHelp(name: "LEVEL")
                                                                 .ExactlyOne()))
                          .BuildCommandDefinition();

            var help = command.Subcommand("the-command").GenerateHelp();

            help.Should().Contain($"{_indentation}-v, --verbosity <LEVEL>{_columnPadding}Sets the verbosity.");
        }

        [Fact]
        public void If_arguments_have_descriptions_then_there_is_an_arguments_section()
        {
            var command = new ParserBuilder()
                          .AddCommand("the-command", "The help text for the command",
                                      arguments: args => args.WithHelp(name: "the-arg",
                                                                       description: "This is the argument for the command.")
                                                             .ZeroOrOne(),
                                      symbols: cmd => cmd.AddOption(
                                          new[] { "-o", "--one" },
                                          "The first option"))
                          .BuildCommandDefinition();

            var help = command.Subcommand("the-command").GenerateHelp();

            _output.WriteLine(help);

            help.Should().Contain($"Arguments:{NewLine}{_indentation}<the-arg>{_columnPadding}This is the argument for the command.");
        }

        #endregion " Arguments "

        #region " Options "

        [Fact]
        public void Retain_single_dash_on_multi_char_option()
        {
            var command = new ParserBuilder()
                          .AddCommand("command", "HelpDefinition Test",
                                      c => c.AddOption(
                                          new[] { "-multi", "--alt-option" },
                                          "HelpDefinition for option"))
                          .BuildCommandDefinition();

            var help = command.Subcommand("command").GenerateHelp();
            help.Should().Contain("-multi");
            help.Should().NotContain("--multi");
        }

        [Fact]
        public void Retain_multiple_dashes_on_single_char_option()
        {
            var command = new ParserBuilder()
                          .AddCommand("command", "HelpDefinition Test",
                                      c => c.AddOption(
                                          new[] { "--m", "--alt-option" },
                                          "HelpDefinition for option"))
                          .BuildCommandDefinition();

            var help = command.Subcommand("command").GenerateHelp();

            help.Should().Contain("--m");
        }

        #endregion " Options "
    }
}
