using System.Diagnostics;

namespace Folleach.Vostok.Logging.Kafka.Tests;

internal record ExecutionResult(int ExitCode, string Output);

internal class Shell
{
    public static async Task<ExecutionResult> RunCommand(string command, string arguments)
    {
        var info = new ProcessStartInfo()
        {
            FileName = command,
            Arguments = arguments,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = new Process
        {
            StartInfo = info
        };
        process.Start();
        var output = await process.StandardOutput.ReadToEndAsync();
        await process.WaitForExitAsync();

        return new ExecutionResult(process.ExitCode, output.Trim());
    }
}
