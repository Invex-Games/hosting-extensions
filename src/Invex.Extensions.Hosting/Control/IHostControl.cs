namespace Invex.Extensions.Hosting.Control;

/// <summary>
///     Provides control over the host application lifetime, extending
///     <see cref="IHostApplicationLifetime" /> with the ability to stop the application with a specific exit code.
/// </summary>
/// <remarks>
///     <para>
///         Register this service by calling
///         <see cref="HostControlHostExtensions.AddHostControl(IServiceCollection)" />, then inject it in place of
///         <see cref="IHostApplicationLifetime" /> wherever the extra control is needed. All members inherited from
///         <see cref="IHostApplicationLifetime" /> delegate to the host's lifetime implementation, so existing
///         lifetime behavior is unchanged.
///     </para>
/// </remarks>
/// <example>
///     <code>
///         public sealed class Worker(IHostControl hostControl)
///         {
///             public void FailFast() =&gt; hostControl.StopApplication(exitCode: 1);
///         }
///     </code>
/// </example>
[PublicAPI]
public interface IHostControl : IHostApplicationLifetime
{
    /// <summary>
    ///     Requests termination of the current application and sets the process exit code.
    /// </summary>
    /// <param name="exitCode">
    ///     The exit code that will be returned to the operating system when the application exits.
    ///     By convention, <c>0</c> indicates success and any non-zero value indicates failure.
    /// </param>
    /// <remarks>
    ///     This sets <see cref="Environment.ExitCode" /> to <paramref name="exitCode" /> and then calls
    ///     <see cref="IHostApplicationLifetime.StopApplication" /> to begin a graceful shutdown.
    ///     Like <see cref="IHostApplicationLifetime.StopApplication" />, this method returns immediately;
    ///     it does not wait for the shutdown to complete.
    /// </remarks>
    void StopApplication(int exitCode = 0);
}
