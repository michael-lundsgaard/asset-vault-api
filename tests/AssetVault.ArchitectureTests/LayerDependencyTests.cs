using AssetVault.API.Extensions;
using AssetVault.Application;
using AssetVault.Domain.Common;
using AssetVault.Infrastructure.Persistence;
using NetArchTest.Rules;

namespace AssetVault.ArchitectureTests;

public class LayerDependencyTests
{
    private static readonly string DomainNs = typeof(BaseEntity).Assembly.GetName().Name!;
    private static readonly string ApplicationNs = typeof(AssemblyMarker).Assembly.GetName().Name!;
    private static readonly string InfrastructureNs = typeof(AppDbContext).Assembly.GetName().Name!;
    private static readonly string ApiNs = typeof(ExpandParser).Assembly.GetName().Name!;

    [Fact]
    public void Domain_ShouldNotDependOnApplication()
    {
        var result = Types.InAssembly(typeof(BaseEntity).Assembly)
            .ShouldNot()
            .HaveDependencyOn(ApplicationNs)
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            because: "Domain must not depend on Application: {0}", string.Join(", ", result.FailingTypeNames ?? []));
    }

    [Fact]
    public void Domain_ShouldNotDependOnInfrastructure()
    {
        var result = Types.InAssembly(typeof(BaseEntity).Assembly)
            .ShouldNot()
            .HaveDependencyOn(InfrastructureNs)
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            because: "Domain must not depend on Infrastructure: {0}", string.Join(", ", result.FailingTypeNames ?? []));
    }

    [Fact]
    public void Domain_ShouldNotDependOnApi()
    {
        var result = Types.InAssembly(typeof(BaseEntity).Assembly)
            .ShouldNot()
            .HaveDependencyOn(ApiNs)
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            because: "Domain must not depend on API: {0}", string.Join(", ", result.FailingTypeNames ?? []));
    }

    [Fact]
    public void Application_ShouldNotDependOnInfrastructure()
    {
        var result = Types.InAssembly(typeof(AssemblyMarker).Assembly)
            .ShouldNot()
            .HaveDependencyOn(InfrastructureNs)
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            because: "Application must not depend on Infrastructure: {0}", string.Join(", ", result.FailingTypeNames ?? []));
    }

    [Fact]
    public void Application_ShouldNotDependOnApi()
    {
        var result = Types.InAssembly(typeof(AssemblyMarker).Assembly)
            .ShouldNot()
            .HaveDependencyOn(ApiNs)
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            because: "Application must not depend on API: {0}", string.Join(", ", result.FailingTypeNames ?? []));
    }

    [Fact]
    public void Infrastructure_ShouldNotDependOnApi()
    {
        var result = Types.InAssembly(typeof(AppDbContext).Assembly)
            .ShouldNot()
            .HaveDependencyOn(ApiNs)
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            because: "Infrastructure must not depend on API: {0}", string.Join(", ", result.FailingTypeNames ?? []));
    }

    [Fact]
    public void Handlers_ShouldResideInApplicationAssembly()
    {
        var result = Types.InAssembly(typeof(AssemblyMarker).Assembly)
            .That()
            .HaveNameEndingWith("Handler")
            .Should()
            .ResideInNamespace("AssetVault.Application")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            because: "Handlers must live in the Application layer: {0}", string.Join(", ", result.FailingTypeNames ?? []));
    }

    [Fact]
    public void Controllers_ShouldResideInApiNamespace()
    {
        var result = Types.InAssembly(typeof(ExpandParser).Assembly)
            .That()
            .HaveNameEndingWith("Controller")
            .Should()
            .ResideInNamespace("AssetVault.API")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            because: "Controllers must live in the API layer: {0}", string.Join(", ", result.FailingTypeNames ?? []));
    }
}
