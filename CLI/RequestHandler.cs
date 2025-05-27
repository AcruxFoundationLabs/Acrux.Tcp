using System.Diagnostics;

public sealed class RequestHandler : IRequestHadler<string, string>
{
    public ITcpDataCodec<string> RequestCodec { get; }

    public ITcpDataCodec<string> ResponseCodec { get; }

    public Task<string> HandleRequestAsync(string request)
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "/bin/bash",
                Arguments = $"-c \"{request}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        process.Start();
        string output = process.StandardOutput.ReadToEnd();
        string error = process.StandardError.ReadToEnd();
        process.WaitForExit();

        return Task.FromResult(output);
    }

    public RequestHandler()
    {
        ITcpDataCodec<string> codec = new Codec();
        RequestCodec = codec;
        ResponseCodec = codec;
    }
}