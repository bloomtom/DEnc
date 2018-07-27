using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace DEnc
{
    internal class ExecutionResult
    {
        public int ExitCode { get; private set; }
        public IEnumerable<string> Output { get; private set; }
        public IEnumerable<string> Error { get; private set; }

        public ExecutionResult(int exitCode, IEnumerable<string> output, IEnumerable<string> error)
        {
            ExitCode = exitCode;
            Output = output;
            Error = error;
        }
    }

    internal static class ManagedExecution
    {
        public static ExecutionResult Start(string path, string arguments, Action<string> outputCallback = null, Action<string> errorCallback = null)
        {
            int exitCode = -1;
            var output = new List<string>();
            var error = new List<string>();

            using (var process = new Process()
            {
                StartInfo = new ProcessStartInfo(path, arguments)
                {
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

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                process.WaitForExit();
                exitCode = process.ExitCode;
            }

            return new ExecutionResult(exitCode, output, error);
        }
    }
}
