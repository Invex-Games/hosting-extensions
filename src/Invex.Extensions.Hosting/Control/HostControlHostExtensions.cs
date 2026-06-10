namespace Invex.Extensions.Hosting.Control;

/// <summary>
///     Provides extension methods for <see cref="IServiceCollection" /> to add host control capabilities.
/// </summary>
[PublicAPI]
public static class HostControlHostExtensions
{
    /// <summary>
    ///     Adds the <see cref="IHostControl" /> service to the <see cref="IServiceCollection" />.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection" /> to add the <see cref="IHostControl" /> service to.</param>
    /// <returns>The <paramref name="services" /> instance so that additional calls can be chained.</returns>
    /// <remarks>
    ///     <see cref="IHostControl" /> is registered as a singleton that wraps the host's
    ///     <see cref="IHostApplicationLifetime" />, adding the ability to stop the application with a specific
    ///     exit code via <see cref="IHostControl.StopApplication(int)" />.
    /// </remarks>
    /// <example>
    ///     <code>
    ///         builder.Services.AddHostControl();
    ///     </code>
    /// </example>
    public static IServiceCollection AddHostControl(this IServiceCollection services) =>
        services.AddSingleton<IHostControl, HostControl>();
}
