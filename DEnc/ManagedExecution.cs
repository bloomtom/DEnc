using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace DEnc
{
    internal static class ManagedExecution
    {
        public static ExecutionResult Start(string path, string arguments, Action<string> outputCallback = null, Action<string> errorCallback = null, CancellationToken cancel = default)
        {
            int exitCode = -1;
            var output = new List<string>();
            var error = new List<string>();

            using (var process = new Process()
            {
                StartInfo = new ProcessStartInfo(path, arguments)
                {
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                },
                EnableRaisingEvents = true
            })
            {
                process.OutputDataReceived += (sender, e) =>
                {
                    output.Add(e.Data);
                    if (outputCallback != null) { outputCallback.Invoke(e.Data); }
                };

                process.ErrorDataReceived += (sender, e) =>
                {
                    error.Add(e.Data);
                    if (errorCallback != null) { errorCallback.Invoke(e.Data); }
                };

                cancel.ThrowIfCancellationRequested();

                try
                {
                    process.Start();
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();

                    while (!process.HasExited)
                    {
                        if (cancel.IsCancellationRequested)
                        {
                            process.StandardInput.WriteLine("\x3"); // Send Ctrl+C
                            process.WaitForExit(1000);
                            if (!process.HasExited)
                            {
                                process.Kill();
                            }
                            cancel.ThrowIfCancellationRequested();
                        }
                        process.WaitForExit(1000);
                    }
                    process.WaitForExit();

                    exitCode = process.ExitCode;
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    throw new Exception($"Failed to execute {path} with arguments {arguments}. Ex: {ex}");
                }
            }

            return new ExecutionResult(exitCode, output, error);
        }
    }

    internal class ExecutionResult
    {
        public ExecutionResult(int exitCode, IEnumerable<string> output, IEnumerable<string> error)
        {
            ExitCode = exitCode;
            Output = output;
            Error = error;
        }

        public IEnumerable<string> Error { get; private set; }
        public int ExitCode { get; private set; }
        public IEnumerable<string> Output { get; private set; }
    }
}