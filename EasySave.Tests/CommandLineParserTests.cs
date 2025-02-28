using Xunit;

namespace EasySave.Tests;

public class CommandLineParserTests
{
    [Fact]
    public void ParseInput_WithRange_ReturnsCorrectNumbers()
    {
        // Arrange
        var input = "1-3";

        // Act
        var result = CommandLineParser.ParseInput(input);

        // Assert
        Assert.Equal(new[] { 1, 2, 3 }, result);
    }

    [Fact]
    public void ParseInput_WithMultipleSelection_ReturnsCorrectNumbers()
    {
        // Arrange
        var input = "1;3;5";

        // Act
        var result = CommandLineParser.ParseInput(input);

        // Assert
        Assert.Equal(new[] { 1, 3, 5 }, result);
    }

    [Fact]
    public void ParseInput_WithInvalidFormat_ReturnsEmptyArray()
    {
        // Arrange
        var input = "invalid";

        // Act
        var result = CommandLineParser.ParseInput(input);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void ParseInput_WithMixedFormat_ReturnsCorrectNumbers()
    {
        // Arrange
        var input = "1-3;5;7-9";

        // Act
        var result = CommandLineParser.ParseInput(input);

        // Assert
        Assert.Equal(new[] { 1, 2, 3, 5, 7, 8, 9 }, result);
    }

    [Fact]
    public void ParseInput_WithNullOrEmpty_ReturnsEmptyArray()
    {
        // Act & Assert
        Assert.Empty(CommandLineParser.ParseInput(null));
        Assert.Empty(CommandLineParser.ParseInput(""));
        Assert.Empty(CommandLineParser.ParseInput(" "));
    }
}
