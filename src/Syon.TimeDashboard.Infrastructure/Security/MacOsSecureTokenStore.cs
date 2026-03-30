using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using Syon.TimeDashboard.Core.Interfaces;

namespace Syon.TimeDashboard.Infrastructure.Security;

public sealed class MacOsSecureTokenStore : ISecureTokenStore
{
    private const string ServiceName = "SyonTimeDashboard";

    public async Task<string?> GetTokenAsync(string key, CancellationToken cancellationToken = default)
    {
        var (exitCode, output) = await RunSecurityAsync(
            $"find-generic-password -s \"{ServiceName}\" -a \"{key}\" -w",
            cancellationToken);

        return exitCode == 0 ? output.Trim() : null;
    }

    public async Task SetTokenAsync(string key, string value, CancellationToken cancellationToken = default)
    {
        // Delete existing first (ignore errors)
        await RunSecurityAsync(
            $"delete-generic-password -s \"{ServiceName}\" -a \"{key}\"",
            cancellationToken);

        await RunSecurityAsync(
            $"add-generic-password -s \"{ServiceName}\" -a \"{key}\" -w \"{value}\" -U",
            cancellationToken);
    }

    public async Task RemoveTokenAsync(string key, CancellationToken cancellationToken = default)
    {
        await RunSecurityAsync(
            $"delete-generic-password -s \"{ServiceName}\" -a \"{key}\"",
            cancellationToken);
    }

    private static async Task<(int ExitCode, string Output)> RunSecurityAsync(string arguments, CancellationToken cancellationToken)
    {
        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "/usr/bin/security",
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        process.Start();
        var output = await process.StandardOutput.ReadToEndAsync(cancellationToken);
        await process.WaitForExitAsync(cancellationToken);
        return (process.ExitCode, output);
    }
}
