using Antyrama.Tools.Scribe.Core.Extensions;
using FluentAssertions;

namespace UnitTests.Extensions;

public class StringExtensionsTests
{
    [Theory]
    [InlineData(
        "{\"name\":\"ApplicationInsights__ConnectionString\",\"value\":\",\\\"In.com/\",\"slotSetting\":false},",
        "{\"name\": \"ApplicationInsights__ConnectionString\", \"value\": \",\\\"In.com/\", \"slotSetting\": false},")]
    [InlineData("\"name\":", "\"name\": ")]
    public void ShouldBeautifyJson(string input, string output)
    {
        input.BeautifyJson().Should().Be(output);
    }
}
