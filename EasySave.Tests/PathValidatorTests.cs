using Xunit;

namespace EasySave.Tests;

public class PathValidatorTests
{
    private readonly string testDir;

    public PathValidatorTests()
    {
        // Créer un répertoire de test temporaire
        testDir = Path.Combine(Path.GetTempPath(), "EasySaveTests");
        if (!Directory.Exists(testDir))
        {
            Directory.CreateDirectory(testDir);
        }
    }

    [Fact]
    public void ValidateSourcePath_WithValidPath_ReturnsTrue()
    {
        // Arrange
        var sourcePath = testDir;

        // Act
        var result = PathValidator.ValidateSourcePath(sourcePath);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void ValidateSourcePath_WithInvalidPath_ReturnsFalse()
    {
        // Arrange
        var sourcePath = Path.Combine(testDir, "NonExistentDirectory");

        // Act
        var result = PathValidator.ValidateSourcePath(sourcePath);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ValidateDestinationPath_CreatesDirectory()
    {
        // Arrange
        var destPath = Path.Combine(testDir, "NewDirectory");
        if (Directory.Exists(destPath))
        {
            Directory.Delete(destPath);
        }

        // Act
        var result = PathValidator.ValidateDestinationPath(destPath);

        // Assert
        Assert.True(result);
        Assert.True(Directory.Exists(destPath));
    }

    [Fact]
    public void ToUncPath_WithTildePath_ReturnsExpandedPath()
    {
        // Arrange
        var tildeBasedPath = "~/Documents";
        var homeDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var expectedPath = Path.GetFullPath(Path.Combine(homeDir, "Documents"));

        // Act
        var result = PathValidator.ToUncPath(tildeBasedPath);

        // Assert
        Assert.Equal(expectedPath, result);
    }

    [Fact]
    public void ValidateSourcePath_WithNullOrEmpty_ReturnsFalse()
    {
        // Act & Assert
        Assert.False(PathValidator.ValidateSourcePath(null));
        Assert.False(PathValidator.ValidateSourcePath(""));
        Assert.False(PathValidator.ValidateSourcePath(" "));
    }
}
