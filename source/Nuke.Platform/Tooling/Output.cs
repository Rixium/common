﻿// Copyright 2018 Maintainers of NUKE.
// Distributed under the MIT License.
// https://github.com/nuke-build/nuke/blob/master/LICENSE

using System;
using System.Linq;
using JetBrains.Annotations;

namespace Nuke.Platform.Tooling
{
    [PublicAPI]
    public struct Output
    {
        public OutputType Type;
        public string Text;
    }
}