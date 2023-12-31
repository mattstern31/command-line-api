﻿// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Xunit;

namespace System.CommandLine.Tests.Utility;

public class ReleaseBuildOnlyTheoryAttribute : TheoryAttribute
{
    public ReleaseBuildOnlyTheoryAttribute()
    {
#if DEBUG
            Skip = "This test runs only on Release builds.";
#endif
    }
}