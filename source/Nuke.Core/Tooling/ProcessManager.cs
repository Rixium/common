// Copyright Matthias Koch 2017.
// Distributed under the MIT License.
// https://github.com/matkoch/Nuke/blob/master/LICENSE

using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using Nuke.Core.Output;
using Nuke.Core.Utilities;

namespace Nuke.Core.Tooling
{
    public class ProcessManager : IProcessManager
    {
        public static IProcessManager Instance { get; private set; } = new ProcessManager();

        internal ProcessManager ()
        {
        }

        public FakeProcessStartInfo CaptureProcessStartInfo (Action action)
        {
            var fakeProcessManager = new FakeProcessManager();
            using (DelegateDisposable.CreateBracket(
                () => Instance = fakeProcessManager,
                () => Instance = this))
            {
                action();
                return fakeProcessManager.FakeProcessStartInfo;
            }
        }

        [CanBeNull]
        public IProcess StartProcess (ToolSettings toolSettings, ProcessSettings processSettings = null)
        {
            ControlFlow.Assert(toolSettings.ToolPath != null,
                $"ToolPath could not be resolved automatically. Please set it manually using '{toolSettings.GetType().Name}.SetToolPath()'.");
            ControlFlow.Assert(File.Exists(toolSettings.ToolPath), $"ToolPath '{toolSettings.ToolPath}' does not exist.");
            OutputSink.Info($"> {toolSettings.ToolPath} {toolSettings.GetArguments().RenderForOutput()}");

            processSettings = processSettings ?? new ProcessSettings();
            var arguments = toolSettings.GetArguments();
            return StartProcessInternal(
                toolSettings.ToolPath,
                arguments.RenderForExecution(),
                toolSettings.WorkingDirectory,
                processSettings.ExecutionTimeout,
                processSettings.RedirectOutput,
                processSettings.RedirectOutput ? new Func<string, string>(arguments.Filter) : null);
        }

        [CanBeNull]
        public virtual IProcess StartProcess (
            string toolPath,
            string arguments = null,
            string workingDirectory = null,
            int? timeout = null,
            bool redirectOutput = false,
            Func<string, string> outputFilter = null)
        {
            ControlFlow.Assert(File.Exists(toolPath), $"ToolPath '{toolPath}' does not exist.");
            OutputSink.Info($"> {toolPath} {arguments}");

            return StartProcessInternal(toolPath, arguments, workingDirectory, timeout, redirectOutput, outputFilter ?? (x => x));
        }

        [CanBeNull]
        private static IProcess StartProcessInternal (
            string toolPath,
            [CanBeNull] string arguments,
            [CanBeNull] string workingDirectory,
            int? timeout,
            bool redirectOutput,
            [CanBeNull] Func<string, string> outputFilter)
        {
            ControlFlow.Assert(workingDirectory == null || Directory.Exists(workingDirectory),
                $"WorkingDirectory '{workingDirectory}' does not exist.");

            var startInfo = new ProcessStartInfo
                            {
                                FileName = toolPath,
                                Arguments = arguments ?? string.Empty,
                                WorkingDirectory = workingDirectory ?? string.Empty,
                                RedirectStandardOutput = redirectOutput,
                                RedirectStandardError = redirectOutput,
                                UseShellExecute = false
                            };

            // TODO: environment variables
            PrintEnvironmentVariables(startInfo);

            var process = Process.Start(startInfo);
            if (process == null)
                return null;

            BlockingCollection<Output> output = null;
            if (redirectOutput)
            {
                output = new BlockingCollection<Output>();
                process.OutputDataReceived += (s, e) => AddNotNullData(output, e, OutputType.Std);
                process.ErrorDataReceived += (s, e) => AddNotNullData(output, e, OutputType.Err);
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
            }

            return new Process2(process, timeout, output, outputFilter ?? (x => x));
        }

        private static void AddNotNullData (BlockingCollection<Output> output, DataReceivedEventArgs e, OutputType outputType)
        {
            if (e.Data != null)
                output.Add(new Output { Text = e.Data, Type = outputType });
        }

        private static void PrintEnvironmentVariables (ProcessStartInfo startInfo)
        {
            Logger.Trace("Environment variables:");
            foreach (var pair in startInfo.Environment.OrderBy(x => x.Key, StringComparer.OrdinalIgnoreCase))
            {
                void Trace(string key, string value) => Logger.Trace($"  - {key} = {value}");
                if (pair.Key.Equals("path", StringComparison.OrdinalIgnoreCase))
                {
                    var paths = pair.Value.Split(';');
                    for (var i = 0; i < paths.Length; i++)
                        Trace($"{pair.Key}[{i}]", paths[i]);
                }
                else
                {
                    Trace(pair.Key, pair.Value);
                }
            }
        }
    }
}
