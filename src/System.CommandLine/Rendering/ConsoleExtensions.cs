﻿// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace System.CommandLine.Rendering
{
    public static class ConsoleExtensions
    {
        public static OutputMode DetectOutputMode(this IConsole console)
        {
            if (console == null)
            {
                throw new ArgumentNullException(nameof(console));
            }

            if (console is ITerminal terminal && 
                !terminal.IsOutputRedirected)
            {
                return terminal.IsVirtualTerminal
                           ? OutputMode.Ansi
                           : OutputMode.NonAnsi;
            }
            else
            {
                return OutputMode.File;
            }
        }

        public static void Clear(this ITerminal console)
        {
            if (console is ITerminal terminal && terminal.IsVirtualTerminal)
            {
                console.Out.WriteLine(Ansi.Clear.EntireScreen.EscapeSequence);
            }
            else
            {
                Console.Clear();
            }
        }
    }
}
