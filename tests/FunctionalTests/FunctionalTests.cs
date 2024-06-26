using System.Reflection;
using System.Runtime.CompilerServices;
using Antyrama.Tools.Scribe.Core;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using YamlDotNet.Serialization;

namespace FunctionalTests;

public class FunctionalTests : IClassFixture<WebApplicationFactory<Program>>
{
    private const string FileTemplate = "[prefix].configuration";

    private readonly WebApplicationFactory<Program> _factory;

    public FunctionalTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Theory]
    [InlineData(false, null)]
    [InlineData(false, EndOfLine.Cr)]
    [InlineData(false, EndOfLine.CrLf)]
    [InlineData(false, EndOfLine.Lf)]
    [InlineData(true, null)]
    [InlineData(true, EndOfLine.Cr)]
    [InlineData(true, EndOfLine.CrLf)]
    [InlineData(true, EndOfLine.Lf)]
    public void ShouldCreateEntryWhenFoundInAppSettingsJson(bool wrapInYaml, EndOfLine? eol)
    {
        // assign
        var serviceProvider = _factory.Server.Services;

        var options = CreateOptions(wrapInYaml, FileTemplate, eol);

        var sut = new AppServiceConfigurationGenerator(serviceProvider, options);

        // act
        sut.Generate();

        // assert
        using (var reader = File.OpenText(options.FilePathTemplate))
        {
            var configuration = Deserialize(options, reader, wrapInYaml);

            configuration!.SelectToken("[1].name")!.Value<string>().Should()
                .Be("Logging__ApplicationInsights__EnableAdaptiveSampling");
            configuration.SelectToken("[1].value")!.Value<bool>().Should().Be(false);
        }

        File.Delete(options.FilePathTemplate);
    }

    [Theory]
    [InlineData(false, null)]
    [InlineData(false, EndOfLine.Cr)]
    [InlineData(false, EndOfLine.CrLf)]
    [InlineData(false, EndOfLine.Lf)]
    [InlineData(true, null)]
    [InlineData(true, EndOfLine.Cr)]
    [InlineData(true, EndOfLine.CrLf)]
    [InlineData(true, EndOfLine.Lf)]
    public void ShouldExcludeEntryWhenSetInOptions(bool wrapInYaml, EndOfLine? eol)
    {
        // assign
        var serviceProvider = _factory.Server.Services;

        var options = CreateOptions(wrapInYaml, FileTemplate, eol);

        options.ExcludeKeys = new[] { "Logging__ApplicationInsights__EnableAdaptiveSampling" };

        var sut = new AppServiceConfigurationGenerator(serviceProvider, options);

        // act
        sut.Generate();

        // assert
        using (var reader = File.OpenText(options.FilePathTemplate))
        {
            var configuration = Deserialize(options, reader, wrapInYaml);

            configuration!.Any(d => d.SelectToken("name")!.Value<string>() == "Logging__ApplicationInsights__EnableAdaptiveSampling")
                .Should().BeFalse();
        }

        File.Delete(options.FilePathTemplate);
    }

    [Theory]
    [InlineData(false, null)]
    [InlineData(false, EndOfLine.Cr)]
    [InlineData(false, EndOfLine.CrLf)]
    [InlineData(false, EndOfLine.Lf)]
    [InlineData(true, null)]
    [InlineData(true, EndOfLine.Cr)]
    [InlineData(true, EndOfLine.CrLf)]
    [InlineData(true, EndOfLine.Lf)]
    public void ShouldIncludeEntryWhenSetInOptionsButNotSelectedByProvider(bool wrapInYaml, EndOfLine? eol)
    {
        // assign
        var varName = "MAGIC_ENVIRONMENT_VARIABLE";
        var varValue = "MAGIC_VALUE";
        Environment.SetEnvironmentVariable(varName, varValue);

        var serviceProvider = _factory.WithWebHostBuilder(_ => { }).Server.Services;

        var options = CreateOptions(wrapInYaml, FileTemplate, eol);

        options.IncludeKeys = new[] { varName };

        var sut = new AppServiceConfigurationGenerator(serviceProvider, options);

        // act
        sut.Generate();

        // assert
        using (var reader = File.OpenText(options.FilePathTemplate))
        {
            var configuration = Deserialize(options, reader, wrapInYaml);

            var selectToken = configuration!.SelectToken($"[?(@.name=='{varName}')]");
            selectToken.Should().NotBeNull();
            selectToken!.SelectToken("value")!.Value<string>().Should().Be(varValue);
        }

        File.Delete(options.FilePathTemplate);
    }

    [Theory]
    [InlineData(false, null)]
    [InlineData(false, EndOfLine.Cr)]
    [InlineData(false, EndOfLine.CrLf)]
    [InlineData(false, EndOfLine.Lf)]
    [InlineData(true, null)]
    [InlineData(true, EndOfLine.Cr)]
    [InlineData(true, EndOfLine.CrLf)]
    [InlineData(true, EndOfLine.Lf)]
    public void ShouldIncludeAllEnvironmentVariablesWhenProviderSelected(bool wrapInYaml, EndOfLine? eol)
    {
        // assign
        var varName = "MAGIC_ENVIRONMENT_VARIABLE";
        var varValue = "MAGIC_VALUE";
        Environment.SetEnvironmentVariable(varName, varValue);

        var serviceProvider = _factory.Server.Services;

        var options = CreateOptions(wrapInYaml, FileTemplate, eol);

        options.Providers = new[] { "EnvironmentVariablesConfigurationProvider" };

        var sut = new AppServiceConfigurationGenerator(serviceProvider, options);

        // act
        sut.Generate();

        // assert
        using (var reader = File.OpenText(options.FilePathTemplate))
        {
            var configuration = Deserialize(options, reader, wrapInYaml);

            configuration!.Count.Should().BeGreaterThan(1);
        }

        File.Delete(options.FilePathTemplate);
    }

    [Theory]
    [InlineData(false, null)]
    [InlineData(false, EndOfLine.Cr)]
    [InlineData(false, EndOfLine.CrLf)]
    [InlineData(false, EndOfLine.Lf)]
    [InlineData(true, null)]
    [InlineData(true, EndOfLine.Cr)]
    [InlineData(true, EndOfLine.CrLf)]
    [InlineData(true, EndOfLine.Lf)]
    public void ShouldNotChangeAnExistingConfigurationEntryValueWhenSettingChanged(bool wrapInYaml, EndOfLine? eol)
    {
        // assign
        var settingKey = "Logging:ApplicationInsights:EnableAdaptiveSampling";

        var factory = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((_, configurationBuilder) =>
            {
                configurationBuilder.AddInMemoryCollection(new[]
                {
                    new KeyValuePair<string, string?>(settingKey, "True")
                });
            });
        });

        var serviceProvider = factory.Server.Services;

        var options = CreateOptions(wrapInYaml, FileTemplate, eol);

        var sut = new AppServiceConfigurationGenerator(serviceProvider, options);

        var extension = wrapInYaml ? "yaml" : "json";
        CopyEmbeddedFile($"configuration.changed.value.{extension}", options.FilePathTemplate);

        // act
        sut.Generate();

        // assert
        // make sure a value of our setting is overriden
        serviceProvider.GetRequiredService<IConfiguration>().GetValue<bool>(settingKey).Should().BeTrue();

        using (var reader = File.OpenText(options.FilePathTemplate))
        {
            var configuration = Deserialize(options, reader, wrapInYaml);

            var selectToken = configuration!.SelectToken($"[?(@.name=='{settingKey.Replace(":", "__")}')]");
            selectToken.Should().NotBeNull();
            selectToken!.SelectToken("value")!.Value<bool>().Should().Be(false);
        }

        File.Delete(options.FilePathTemplate);
    }

    [Theory]
    [InlineData(false, null)]
    [InlineData(false, EndOfLine.Cr)]
    [InlineData(false, EndOfLine.CrLf)]
    [InlineData(false, EndOfLine.Lf)]
    [InlineData(true, null)]
    [InlineData(true, EndOfLine.Cr)]
    [InlineData(true, EndOfLine.CrLf)]
    [InlineData(true, EndOfLine.Lf)]
    public void ShouldNotRemoveExtraPropertiesOnAnExistingConfigurationEntry(bool wrapInYaml, EndOfLine? eol)
    {
        // assign
        var settingKey = "Logging__ApplicationInsights__EnableAdaptiveSampling";

        var serviceProvider = _factory.Server.Services;

        var options = CreateOptions(wrapInYaml, FileTemplate, eol);

        var sut = new AppServiceConfigurationGenerator(serviceProvider, options);

        var extension = wrapInYaml ? "yaml" : "json";
        CopyEmbeddedFile($"configuration.extra.property.{extension}", options.FilePathTemplate);

        // act
        sut.Generate();

        // assert
        using (var reader = File.OpenText(options.FilePathTemplate))
        {
            var configuration = Deserialize(options, reader, wrapInYaml);

            var selectToken = configuration!.SelectToken($"[?(@.name=='{settingKey}')]");
            selectToken.Should().NotBeNull();
            selectToken!.SelectToken("extraProperty")!.Value<string>().Should().Be("extra value");
        }

        File.Delete(options.FilePathTemplate);
    }

    [Theory]
    [InlineData(false, null)]
    [InlineData(false, EndOfLine.Cr)]
    [InlineData(false, EndOfLine.CrLf)]
    [InlineData(false, EndOfLine.Lf)]
    [InlineData(true, null)]
    [InlineData(true, EndOfLine.Cr)]
    [InlineData(true, EndOfLine.CrLf)]
    [InlineData(true, EndOfLine.Lf)]
    public void ShouldGenerateFilePerEnvironmentGivenInOptions(bool wrapInYaml, EndOfLine? eol)
    {
        // assign
        var serviceProvider = _factory.Server.Services;

        var options = CreateOptions(wrapInYaml, $"{FileTemplate}.{{0}}", eol);

        options.Environments = new[] { "dev", "test", "prod" };

        var sut = new AppServiceConfigurationGenerator(serviceProvider, options);

        // act
        sut.Generate();

        // assert
        foreach (var environment in options.Environments)
        {
            var fileName = string.Format(options.FilePathTemplate, environment);
            using (var reader = File.OpenText(fileName))
            {
                var configuration = Deserialize(options, reader, wrapInYaml);

                configuration.Should().HaveCount(4);
            }

            File.Delete(fileName);
        }
    }

    [Theory]
    [InlineData(false, null)]
    [InlineData(false, EndOfLine.Cr)]
    [InlineData(false, EndOfLine.CrLf)]
    [InlineData(false, EndOfLine.Lf)]
    [InlineData(true, null)]
    [InlineData(true, EndOfLine.Cr)]
    [InlineData(true, EndOfLine.CrLf)]
    [InlineData(true, EndOfLine.Lf)]
    public void ShouldHandleRelativePaths(bool wrapInYaml, EndOfLine? eof)
    {
        // assign
        var serviceProvider = _factory.Server.Services;

        var options = CreateOptions(wrapInYaml, $"..\\{FileTemplate}", eof);

        var sut = new AppServiceConfigurationGenerator(serviceProvider, options);

        // act
        sut.Generate();

        // assert
        File.Exists(options.FilePathTemplate).Should().BeTrue();
        File.Delete(options.FilePathTemplate);
    }

    [Fact]
    public async Task ShouldExcludeExactSetting()
    {
        // assign
        var serviceProvider = _factory.Server.Services;

        var options = CreateOptions(false, $"..\\{FileTemplate}", null, new[] { "AllowedHosts" });

        var sut = new AppServiceConfigurationGenerator(serviceProvider, options);

        // act
        sut.Generate();

        // assert
        var json = await File.ReadAllTextAsync(options.FilePathTemplate);
        await VerifyJson(json);

        File.Delete(options.FilePathTemplate);
    }

    [Fact]
    public async Task ShouldExcludeWithAllNestedSettings()
    {
        // assign
        var serviceProvider = _factory.Server.Services;

        var options = CreateOptions(false, $"..\\{FileTemplate}", null, new[] { "Logging:LogLevel" });

        var sut = new AppServiceConfigurationGenerator(serviceProvider, options);

        // act
        sut.Generate();

        // assert
        var json = await File.ReadAllTextAsync(options.FilePathTemplate);
        await VerifyJson(json);

        File.Delete(options.FilePathTemplate);
    }

    private static JArray? Deserialize(ToolInternalOptions options, TextReader reader, bool wrapInYaml)
    {
        string? content;
        if (wrapInYaml)
        {
            var deserializer = new DeserializerBuilder().Build();
            var dictionary = deserializer.Deserialize<Dictionary<object, object>>(reader);
            dictionary.ContainsKey("variables").Should().BeTrue();

            var variables = dictionary["variables"] as Dictionary<object, object>;
            variables.Should().NotBeNull();
            variables!.ContainsKey(options.YamlVariableName).Should().BeTrue();

            content = variables[options.YamlVariableName].ToString();
            content.Should().NotBeNullOrWhiteSpace();
        }
        else
        {
            content = reader.ReadToEnd();
        }

        return JsonConvert.DeserializeObject<JArray>(content!);
    }

    private static ToolInternalOptions CreateOptions(bool wrapInYaml, string fileTemplate, EndOfLine? eof,
        string[]? exclude = null, [CallerMemberName] string prefix = "")
    {
        var options = new ToolInternalOptions
        {
            PathSeparator = "__",
            Providers = new[] { "JsonConfigurationProvider" },
            Assembly = "assembly",
            FilePathTemplate = Path.Combine(Environment.CurrentDirectory, fileTemplate.Replace("[prefix]", prefix)),
            Environments = Array.Empty<string>(),
            ExcludeKeys = exclude ?? Array.Empty<string>(),
            IncludeKeys = Array.Empty<string>(),
            YamlVariableName = "some_name",
            WrapInYaml = wrapInYaml,
            Eol = eof
        };
        return options;
    }

    private static void CopyEmbeddedFile(string source, string destination)
    {
        var assembly = Assembly.GetCallingAssembly();
        using var stream = assembly.GetManifestResourceStream($"{assembly.GetName().Name!}.{source}");
        using Stream outStream = new FileStream(destination, FileMode.Create, FileAccess.Write);
        stream!.CopyTo(outStream);
    }
}
