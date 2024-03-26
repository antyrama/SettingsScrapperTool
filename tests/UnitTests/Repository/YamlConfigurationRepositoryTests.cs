using System.Text;
using Antyrama.Tools.Scribe.Core;
using Antyrama.Tools.Scribe.Core.Repository;
using FluentAssertions;

namespace UnitTests.Repository;

public sealed class YamlConfigurationRepositoryTests
{
    [Theory]
    [InlineData(EndOfLine.CrLf, "\r\n")]
    [InlineData(EndOfLine.Cr, "\r")]
    [InlineData(EndOfLine.Lf, "\n")]
    public void ShouldWriteAndReadConfigFileWithAllPossibleEndOfLines(EndOfLine? eol, string eolChars)
    {
        // arrange
        var options = new ToolInternalOptions
        {
            Eol = eol,
            YamlVariableName = "app_config"
        };
        var sut = new YamlConfigurationRepository(options);

        using var stream = new MemoryStream();

        // act
        sut.Save(stream,
            new IReadOnlyDictionary<string, object>[]
            {
                new Dictionary<string, object> { { "key1", "value1" }, { "key2", "value2" } }
            });

        // assert
        stream.Seek(0, SeekOrigin.Begin);

        using var reader = new StreamReader(stream, Encoding.UTF8);
        var result = reader.ReadToEnd();

        result.Should().Contain(eolChars);
    }

    [Theory]
    [InlineData(" ")]
    [InlineData("variables:")]
    [InlineData("variables:\r\n  app_config: -|")]
    [InlineData("variables")]
    public void ShouldEmptyResultWhenVariablesIsNullOrEmpty(string yaml)
    {
        // arrange
        var options = new ToolInternalOptions
        {
            YamlVariableName = "app_config"
        };
        var sut = new YamlConfigurationRepository(options);

        using var stream = new MemoryStream();
        using var writer = new StreamWriter(stream, Encoding.UTF8);
        writer.Write(yaml);
        writer.Flush();
        stream.Seek(0, SeekOrigin.Begin);

        // act
        var result = sut.Load(stream);

        // assert
        result.Should().BeEmpty();
    }
}
