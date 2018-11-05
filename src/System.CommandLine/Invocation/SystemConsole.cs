﻿// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Rendering;
using System.IO;

namespace System.CommandLine.Invocation
{
    internal class SystemConsole : IConsole
    {
        private VirtualTerminalMode _virtualTerminalMode;
        private readonly ConsoleColor _initialForegroundColor;
        private readonly ConsoleColor _initialBackgroundColor;
        private readonly ConsoleCancelEventHandler _cancelEventHandler;

        internal SystemConsole()
        {
            _initialForegroundColor = Console.ForegroundColor;
            _initialBackgroundColor = Console.BackgroundColor;
        }

        public void SetOut(TextWriter writer) => Console.SetOut(writer);

        public TextWriter Error => Console.Error;

        public TextWriter Out => Console.Out;

        public ConsoleColor BackgroundColor
        {
            get => Console.BackgroundColor;
            set => Console.BackgroundColor = value;
        }

        public ConsoleColor ForegroundColor
        {
            get => Console.ForegroundColor;
            set => Console.ForegroundColor = value;
        }

        public void ResetColor() => Console.ResetColor();

        public Region GetRegion() =>
            IsOutputRedirected
                ? new Region(0, 0, int.MaxValue, int.MaxValue, false)
                : EntireConsoleRegion.Instance;

        public int CursorLeft
        {
            get => Console.CursorLeft;
            set => Console.CursorLeft = value;
        }

        public int CursorTop
        {
            get => Console.CursorTop;
            set => Console.CursorTop = value;
        }

        public void SetCursorPosition(int left, int top) => Console.SetCursorPosition(left, top);

        public bool IsOutputRedirected => Console.IsOutputRedirected;

        public bool IsErrorRedirected => Console.IsErrorRedirected;

        public bool IsInputRedirected => Console.IsInputRedirected;

        public bool IsVirtualTerminal
        {
            get
            {
                if (_virtualTerminalMode != null)
                {
                    return _virtualTerminalMode.IsEnabled;
                }

                var terminalName = Environment.GetEnvironmentVariable("TERM");

                return !string.IsNullOrEmpty(terminalName)
                       && terminalName.StartsWith("xterm", StringComparison.OrdinalIgnoreCase);
            }
        }

        public void TryEnableVirtualTerminal()
        {
            if (IsOutputRedirected)
            {
                return;
            }

            _virtualTerminalMode = VirtualTerminalMode.TryEnable();
        }

        private void ResetConsole()
        {
            _virtualTerminalMode?.Dispose();

            Console.ForegroundColor = _initialForegroundColor;
            Console.BackgroundColor = _initialBackgroundColor;
        }

        public void Dispose()
        {
            CancelKeyPress = null;
            ResetConsole();
            GC.SuppressFinalize(this);
        }

        public Action CancelKeyPress
        {
            set
            {
                if (value != null)
                {
                    if (_cancelEventHandler != null)
                    {
                        throw new ArgumentException("Action is already assigned.", nameof(value));
                    }
                    _cancelEventHandler = (o, e) => { e.Cancel = true; value(); };
                    ConsoleExtensions.CancelKeyPress += _cancelEventHandler;
                }
                else if (_cancelEventHandler != null)
                {
                    ConsoleExtensions.CancelKeyPress -= _cancelEventHandler;
                    _cancelEventHandler = null;
                }
            }
        }

        ~SystemConsole()
        {
            ResetConsole();
        }

        public static IConsole Create() => new SystemConsole();
    }
}
