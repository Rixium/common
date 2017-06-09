// Copyright Matthias Koch 2017.
// Distributed under the MIT License.
// https://github.com/matkoch/Nuke/blob/master/LICENSE

using System;
using System.Linq;
using Nuke.Core;

namespace Nuke.Common.Tools.Xunit
{
    partial class XunitSettings
    {
        private string GetExecutable ()
        {
            return EnvironmentInfo.Is64Bit ? "xunit.console.exe" : "xunit.console.x86.exe";
        }
    }
}
