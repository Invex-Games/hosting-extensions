namespace Invex.Extensions.Hosting.Tests.Control;

[TestFixture]
public class HostControlTests
{
    [Test]
    public void HostControl_Correctly_Wraps_IHostApplicationLifetime()
    {
        // Arrange
        var hostApplicationLifetime = A.Fake<IHostApplicationLifetime>();
        var hostControl = new HostControl(hostApplicationLifetime);

        // Act
        _ = hostControl.ApplicationStarted;
        _ = hostControl.ApplicationStopping;
        _ = hostControl.ApplicationStopped;
        hostControl.StopApplication();

        // Assert
        A
            .CallTo(() => hostApplicationLifetime.ApplicationStarted)
            .MustHaveHappenedOnceExactly();

        A
            .CallTo(() => hostApplicationLifetime.ApplicationStopping)
            .MustHaveHappenedOnceExactly();

        A
            .CallTo(() => hostApplicationLifetime.ApplicationStopped)
            .MustHaveHappenedOnceExactly();

        A
            .CallTo(() => hostApplicationLifetime.StopApplication())
            .MustHaveHappenedOnceExactly();
    }

    [Test]
    public void StopApplication_Sets_Environment_ExitCode()
    {
        // Arrange
        var hostApplicationLifetime = A.Fake<IHostApplicationLifetime>();
        var hostControl = new HostControl(hostApplicationLifetime);
        const int exitCode = 123;

        // Act
        hostControl.StopApplication(exitCode);

        // Assert
        Environment.ExitCode.ShouldBe(exitCode);
    }

    [Test]
    public void StopApplication_Calls_StopApplication_On_HostApplicationLifetime()
    {
        // Arrange
        var hostApplicationLifetime = A.Fake<IHostApplicationLifetime>();
        var hostControl = new HostControl(hostApplicationLifetime);

        // Act
        hostControl.StopApplication();

        // Assert
        A
            .CallTo(() => hostApplicationLifetime.StopApplication())
            .MustHaveHappenedOnceExactly();
    }
}
