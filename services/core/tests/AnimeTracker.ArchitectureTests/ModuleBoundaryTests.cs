using NetArchTest.Rules;
using Shouldly;

namespace AnimeTracker.ArchitectureTests;

public class ModuleBoundaryTests
{
    public static TheoryData<string> Modules => new() { "Catalog", "Tracking", "Calendar" };

    [Theory, MemberData(nameof(Modules))]
    public void Domain_no_depende_de_capas_externas(string module)
    {
        var result = Types.InAssembly(AssemblyHelper.Of(module, "Domain"))
            .ShouldNot()
            .HaveDependencyOnAny(
                $"AnimeTracker.{module}.Application",
                $"AnimeTracker.{module}.Infrastructure",
                $"AnimeTracker.{module}.Api",
                "Microsoft.EntityFrameworkCore")
            .GetResult();

        result.IsSuccessful.ShouldBeTrue(
            $"Tipos con dependencias prohibidas: {string.Join(", ", result.FailingTypeNames ?? [])}");
    }

    [Theory, MemberData(nameof(Modules))]
    public void Api_no_depende_de_Infrastructure(string module)
    {
        var result = Types.InAssembly(AssemblyHelper.Of(module, "Api"))
            .ShouldNot()
            .HaveDependencyOnAny($"AnimeTracker.{module}.Infrastructure")
            .GetResult();

        result.IsSuccessful.ShouldBeTrue(
            $"Tipos con dependencias prohibidas: {string.Join(", ", result.FailingTypeNames ?? [])}");
    }

    [Theory, MemberData(nameof(Modules))]
    public void Application_no_depende_de_Infrastructure_ni_Api(string module)
    {
        var result = Types.InAssembly(AssemblyHelper.Of(module, "Application"))
            .ShouldNot()
            .HaveDependencyOnAny(
                $"AnimeTracker.{module}.Infrastructure",
                $"AnimeTracker.{module}.Api")
            .GetResult();

        result.IsSuccessful.ShouldBeTrue(
            $"Tipos con dependencias prohibidas: {string.Join(", ", result.FailingTypeNames ?? [])}");
    }

    [Theory]
    [InlineData("Tracking", "Catalog")]
    [InlineData("Calendar", "Catalog")]
    [InlineData("Catalog", "Tracking")]
    [InlineData("Catalog", "Calendar")]
    [InlineData("Tracking", "Calendar")]
    [InlineData("Calendar", "Tracking")]
    public void Modulo_solo_ve_Contracts_de_otro(string origin, string target)
    {
        var result = Types.InNamespace($"AnimeTracker.{origin}")
            .ShouldNot()
            .HaveDependencyOnAny(
                $"AnimeTracker.{target}.Domain",
                $"AnimeTracker.{target}.Application",
                $"AnimeTracker.{target}.Infrastructure",
                $"AnimeTracker.{target}.Api")
            .GetResult();

        result.IsSuccessful.ShouldBeTrue(
            $"Tipos de {origin} que referencian internals de {target}: " +
            $"{string.Join(", ", result.FailingTypeNames ?? [])}");
    }

    [Theory, MemberData(nameof(Modules))]
    public void Contracts_no_depende_de_ninguna_capa_interna(string module)
    {
        var result = Types.InAssembly(AssemblyHelper.Of(module, "Contracts"))
            .ShouldNot()
            .HaveDependencyOnAny(
                $"AnimeTracker.{module}.Domain",
                $"AnimeTracker.{module}.Application",
                $"AnimeTracker.{module}.Infrastructure",
                $"AnimeTracker.{module}.Api")
            .GetResult();

        result.IsSuccessful.ShouldBeTrue(
            $"Contracts de {module} tiene dependencias internas: " +
            $"{string.Join(", ", result.FailingTypeNames ?? [])}");
    }
}
