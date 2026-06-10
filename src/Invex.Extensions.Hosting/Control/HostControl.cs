namespace Invex.Extensions.Hosting.Control;

/// <inheritdoc />
internal sealed class HostControl(IHostApplicationLifetime hostApplicationLifetime) : IHostControl
{
    /// <inheritdoc />
    public void StopApplication(int exitCode)
    {
        Environment.ExitCode = exitCode;
        StopApplication();
    }

    /// <inheritdoc />
    public void StopApplication() =>
        hostApplicationLifetime.StopApplication();

    /// <inheritdoc />
    public CancellationToken ApplicationStarted => hostApplicationLifetime.ApplicationStarted;

    /// <inheritdoc />
    public CancellationToken ApplicationStopping => hostApplicationLifetime.ApplicationStopping;

    /// <inheritdoc />
    public CancellationToken ApplicationStopped => hostApplicationLifetime.ApplicationStopped;
}
