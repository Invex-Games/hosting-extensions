namespace Invex.Extensions.Hosting.Tests;

[TestFixture]
public class ServiceCollectionExtensionsTests
{
    private interface IInterface1;

    private interface IInterface2;

    private interface IInterface3;

    private interface IInterface4;

    private interface IInterface5;

    private class Implementation : IHostedService, IInterface1, IInterface2, IInterface3, IInterface4, IInterface5
    {
        public Task StartAsync(CancellationToken cancellationToken) =>
            Task.CompletedTask;

        public Task StopAsync(CancellationToken cancellationToken) =>
            Task.CompletedTask;
    }

    [Test]
    public void Microsoft_DependencyInjection_DoesNotShareInstance()
    {
        // Arrange
        IServiceCollection services = new ServiceCollection();

        // Act
        services = services.AddSingleton<Implementation>();
        services = services.AddSingleton<IInterface1, Implementation>();
        services = services.AddSingleton<IInterface2, Implementation>();

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var interface1 = serviceProvider.GetRequiredService<IInterface1>();
        var interface2 = serviceProvider.GetRequiredService<IInterface2>();
        var implementation = serviceProvider.GetRequiredService<Implementation>();

        interface1.ShouldNotBe(implementation);
        interface2.ShouldNotBe(implementation);
    }

    [Test]
    public void AddSingleton_WithTwoInterfacesAndImplementation_ShouldRegisterAllServices()
    {
        // Arrange
        IServiceCollection services = new ServiceCollection();

        // Act
        services = services.AddSingleton<IInterface1, IInterface2, Implementation>();

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var interface1 = serviceProvider.GetRequiredService<IInterface1>();
        var interface2 = serviceProvider.GetRequiredService<IInterface2>();
        var implementation = serviceProvider.GetRequiredService<Implementation>();

        interface1.ShouldBe(implementation);
        interface2.ShouldBe(implementation);
    }

    [Test]
    public void AddSingleton_WithThreeInterfacesAndImplementation_ShouldRegisterAllServices()
    {
        // Arrange
        IServiceCollection services = new ServiceCollection();

        // Act
        services = services.AddSingleton<IInterface1, IInterface2, IInterface3, Implementation>();

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var interface1 = serviceProvider.GetRequiredService<IInterface1>();
        var interface2 = serviceProvider.GetRequiredService<IInterface2>();
        var interface3 = serviceProvider.GetRequiredService<IInterface3>();
        var implementation = serviceProvider.GetRequiredService<Implementation>();

        interface1.ShouldBe(implementation);
        interface2.ShouldBe(implementation);
        interface3.ShouldBe(implementation);
    }

    [Test]
    public void AddSingleton_WithFourInterfacesAndImplementation_ShouldRegisterAllServices()
    {
        // Arrange
        IServiceCollection services = new ServiceCollection();

        // Act
        services = services.AddSingleton<IInterface1, IInterface2, IInterface3, IInterface4, Implementation>();

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var interface1 = serviceProvider.GetRequiredService<IInterface1>();
        var interface2 = serviceProvider.GetRequiredService<IInterface2>();
        var interface3 = serviceProvider.GetRequiredService<IInterface3>();
        var interface4 = serviceProvider.GetRequiredService<IInterface4>();
        var implementation = serviceProvider.GetRequiredService<Implementation>();

        interface1.ShouldBe(implementation);
        interface2.ShouldBe(implementation);
        interface3.ShouldBe(implementation);
        interface4.ShouldBe(implementation);
    }

    [Test]
    public void AddSingleton_WithFiveInterfacesAndImplementation_ShouldRegisterAllServices()
    {
        // Arrange
        IServiceCollection services = new ServiceCollection();

        // Act
        services = services
            .AddSingleton<IInterface1, IInterface2, IInterface3, IInterface4, IInterface5, Implementation>();

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var interface1 = serviceProvider.GetRequiredService<IInterface1>();
        var interface2 = serviceProvider.GetRequiredService<IInterface2>();
        var interface3 = serviceProvider.GetRequiredService<IInterface3>();
        var interface4 = serviceProvider.GetRequiredService<IInterface4>();
        var interface5 = serviceProvider.GetRequiredService<IInterface5>();
        var implementation = serviceProvider.GetRequiredService<Implementation>();

        interface1.ShouldBe(implementation);
        interface2.ShouldBe(implementation);
        interface3.ShouldBe(implementation);
        interface4.ShouldBe(implementation);
        interface5.ShouldBe(implementation);
    }

    [Test]
    public void AddScoped_WithTwoInterfacesAndImplementation_ShouldRegisterAllServices()
    {
        // Arrange
        IServiceCollection services = new ServiceCollection();

        // Act
        services = services.AddScoped<IInterface1, IInterface2, Implementation>();

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var scope = serviceProvider.CreateScope();
        var interface1 = scope.ServiceProvider.GetRequiredService<IInterface1>();
        var interface2 = scope.ServiceProvider.GetRequiredService<IInterface2>();
        var implementation = scope.ServiceProvider.GetRequiredService<Implementation>();

        interface1.ShouldBe(implementation);
        interface2.ShouldBe(implementation);
    }

    [Test]
    public void AddScoped_WithThreeInterfacesAndImplementation_ShouldRegisterAllServices()
    {
        // Arrange
        IServiceCollection services = new ServiceCollection();

        // Act
        services = services.AddScoped<IInterface1, IInterface2, IInterface3, Implementation>();

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var scope = serviceProvider.CreateScope();
        var interface1 = scope.ServiceProvider.GetRequiredService<IInterface1>();
        var interface2 = scope.ServiceProvider.GetRequiredService<IInterface2>();
        var interface3 = scope.ServiceProvider.GetRequiredService<IInterface3>();
        var implementation = scope.ServiceProvider.GetRequiredService<Implementation>();

        interface1.ShouldBe(implementation);
        interface2.ShouldBe(implementation);
        interface3.ShouldBe(implementation);
    }

    [Test]
    public void AddScoped_WithFourInterfacesAndImplementation_ShouldRegisterAllServices()
    {
        // Arrange
        IServiceCollection services = new ServiceCollection();

        // Act
        services = services.AddScoped<IInterface1, IInterface2, IInterface3, IInterface4, Implementation>();

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var scope = serviceProvider.CreateScope();
        var interface1 = scope.ServiceProvider.GetRequiredService<IInterface1>();
        var interface2 = scope.ServiceProvider.GetRequiredService<IInterface2>();
        var interface3 = scope.ServiceProvider.GetRequiredService<IInterface3>();
        var interface4 = scope.ServiceProvider.GetRequiredService<IInterface4>();
        var implementation = scope.ServiceProvider.GetRequiredService<Implementation>();

        interface1.ShouldBe(implementation);
        interface2.ShouldBe(implementation);
        interface3.ShouldBe(implementation);
        interface4.ShouldBe(implementation);
    }

    [Test]
    public void AddScoped_WithFiveInterfacesAndImplementation_ShouldRegisterAllServices()
    {
        // Arrange
        IServiceCollection services = new ServiceCollection();

        // Act
        services = services
            .AddScoped<IInterface1, IInterface2, IInterface3, IInterface4, IInterface5, Implementation>();

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var scope = serviceProvider.CreateScope();
        var interface1 = scope.ServiceProvider.GetRequiredService<IInterface1>();
        var interface2 = scope.ServiceProvider.GetRequiredService<IInterface2>();
        var interface3 = scope.ServiceProvider.GetRequiredService<IInterface3>();
        var interface4 = scope.ServiceProvider.GetRequiredService<IInterface4>();
        var interface5 = scope.ServiceProvider.GetRequiredService<IInterface5>();
        var implementation = scope.ServiceProvider.GetRequiredService<Implementation>();

        interface1.ShouldBe(implementation);
        interface2.ShouldBe(implementation);
        interface3.ShouldBe(implementation);
        interface4.ShouldBe(implementation);
        interface5.ShouldBe(implementation);
    }

    [Test]
    public void AddHostedService_WithInterfaceAndImplementation_ShouldRegisterAllServices()
    {
        // Arrange
        IServiceCollection services = new ServiceCollection();

        // Act
        services = services.AddHostedService<IInterface1, Implementation>();

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var hostedService = serviceProvider.GetRequiredService<IInterface1>();
        var implementation = serviceProvider.GetRequiredService<Implementation>();

        hostedService.ShouldBe(implementation);
    }

    [Test]
    public void AddHostedService_WithTwoInterfacesAndImplementation_ShouldRegisterAllServices()
    {
        // Arrange
        IServiceCollection services = new ServiceCollection();

        // Act
        services = services.AddHostedService<IInterface1, IInterface2, Implementation>();

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var hostedService1 = serviceProvider.GetRequiredService<IInterface1>();
        var hostedService2 = serviceProvider.GetRequiredService<IInterface2>();
        var implementation = serviceProvider.GetRequiredService<Implementation>();

        hostedService1.ShouldBe(implementation);
        hostedService2.ShouldBe(implementation);
    }

    [Test]
    public void AddHostedService_WithThreeInterfacesAndImplementation_ShouldRegisterAllServices()
    {
        // Arrange
        IServiceCollection services = new ServiceCollection();

        // Act
        services = services.AddHostedService<IInterface1, IInterface2, IInterface3, Implementation>();

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var hostedService1 = serviceProvider.GetRequiredService<IInterface1>();
        var hostedService2 = serviceProvider.GetRequiredService<IInterface2>();
        var hostedService3 = serviceProvider.GetRequiredService<IInterface3>();
        var implementation = serviceProvider.GetRequiredService<Implementation>();

        hostedService1.ShouldBe(implementation);
        hostedService2.ShouldBe(implementation);
        hostedService3.ShouldBe(implementation);
    }

    [Test]
    public void AddHostedService_WithFourInterfacesAndImplementation_ShouldRegisterAllServices()
    {
        // Arrange
        IServiceCollection services = new ServiceCollection();

        // Act
        services = services.AddHostedService<IInterface1, IInterface2, IInterface3, IInterface4, Implementation>();

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var hostedService1 = serviceProvider.GetRequiredService<IInterface1>();
        var hostedService2 = serviceProvider.GetRequiredService<IInterface2>();
        var hostedService3 = serviceProvider.GetRequiredService<IInterface3>();
        var hostedService4 = serviceProvider.GetRequiredService<IInterface4>();
        var implementation = serviceProvider.GetRequiredService<Implementation>();

        hostedService1.ShouldBe(implementation);
        hostedService2.ShouldBe(implementation);
        hostedService3.ShouldBe(implementation);
        hostedService4.ShouldBe(implementation);
    }

    [Test]
    public void AddHostedService_WithFiveInterfacesAndImplementation_ShouldRegisterAllServices()
    {
        // Arrange
        IServiceCollection services = new ServiceCollection();

        // Act
        services = services
            .AddHostedService<IInterface1, IInterface2, IInterface3, IInterface4, IInterface5, Implementation>();

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var hostedService1 = serviceProvider.GetRequiredService<IInterface1>();
        var hostedService2 = serviceProvider.GetRequiredService<IInterface2>();
        var hostedService3 = serviceProvider.GetRequiredService<IInterface3>();
        var hostedService4 = serviceProvider.GetRequiredService<IInterface4>();
        var hostedService5 = serviceProvider.GetRequiredService<IInterface5>();
        var implementation = serviceProvider.GetRequiredService<Implementation>();

        hostedService1.ShouldBe(implementation);
        hostedService2.ShouldBe(implementation);
        hostedService3.ShouldBe(implementation);
        hostedService4.ShouldBe(implementation);
        hostedService5.ShouldBe(implementation);
    }
}
