﻿using System.Text;
using Antyrama.Tools.Scribe.Core;
using Antyrama.Tools.Scribe.Core.Repository;
using FluentAssertions;

namespace UnitTests.Repository;

public sealed class JsonConfigurationRepositoryTests
{
    [Theory]
    [InlineData(EndOfLine.CrLf, "\r\n")]
    [InlineData(EndOfLine.Cr, "\r")]
    [InlineData(EndOfLine.Lf, "\n")]
    public void ShouldWriteAndReadConfigFileWithAllPossibleEndOfLines(EndOfLine? eol, string eolChars)
    {
        // arrange
        var options = new ToolInternalOptions { Eol = eol };
        var sut = new JsonConfigurationRepository(options);

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
}
