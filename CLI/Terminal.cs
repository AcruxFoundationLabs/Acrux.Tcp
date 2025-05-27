using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

public static class InteractiveShell
{
    private static Process shell;
    private static StreamWriter shellInput;

    public static async Task StartAsync()
    {
        shell = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "/bin/bash",
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        shell.Start();

        shellInput = shell.StandardInput;

        // Read output asynchronously
        _ = Task.Run(() =>
        {
            string line;
            while ((line = shell.StandardOutput.ReadLine()) != null)
            {
                Console.WriteLine(line);
            }
        });

        // Read errors asynchronously
        _ = Task.Run(() =>
        {
            string line;
            while ((line = shell.StandardError.ReadLine()) != null)
            {
                Console.Error.WriteLine(line);
            }
        });
    }

    public static async Task UsePrompt(string command)
    {
        if (shellInput == null)
            throw new InvalidOperationException("Shell not started. Call StartAsync() first.");

        await shellInput.WriteLineAsync(command);
        await shellInput.FlushAsync();
    }

    public static async Task StopAsync()
    {
        if (shellInput != null)
        {
            await shellInput.WriteLineAsync("exit");
            await shellInput.FlushAsync();
        }

        shell?.WaitForExit();
        shellInput?.Dispose();
        shell?.Dispose();
    }
}
