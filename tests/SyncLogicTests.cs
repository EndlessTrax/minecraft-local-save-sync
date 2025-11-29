using System.IO;
using Xunit;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

public class SyncLogicTests : IDisposable
{
    private readonly string _testRoot;
    private readonly string _minecraftSavesPath;
    private readonly string _backupPath;

    public SyncLogicTests()
    {
        _testRoot = Path.Combine(Path.GetTempPath(), "mlss_tests_" + Guid.NewGuid().ToString("N"));
        _minecraftSavesPath = Path.Combine(_testRoot, ".minecraft", "saves");
        _backupPath = Path.Combine(_testRoot, "backup");

        Directory.CreateDirectory(_minecraftSavesPath);
        Directory.CreateDirectory(_backupPath);
    }

    private (string, string) CreateTestSave(string saveName, string inPath)
    {
        var saveDir = Path.Combine(inPath, saveName);
        Directory.CreateDirectory(saveDir);
        var testFile = Path.Combine(saveDir, "level.dat");
        File.WriteAllText(testFile, "test_content");
        return (saveDir, testFile);
    }

    [Fact]
    public void Push_AllSaves_CopiesAll()
    {
        // Arrange
        CreateTestSave("World1", _minecraftSavesPath);
        CreateTestSave("World2", _minecraftSavesPath);
        // No-op: CLI wiring is tested in integration; here we call logic directly.

        // Act
        global::Program.ExecuteSync(new DirectoryInfo(_backupPath), null, _minecraftSavesPath, global::Program.SyncDirection.Push);

        // Assert
        Assert.True(Directory.Exists(Path.Combine(_backupPath, "World1")));
        Assert.True(File.Exists(Path.Combine(_backupPath, "World1", "level.dat")));
        Assert.True(Directory.Exists(Path.Combine(_backupPath, "World2")));
        Assert.True(File.Exists(Path.Combine(_backupPath, "World2", "level.dat")));
    }

    [Fact]
    public void Pull_SpecificSaves_CopiesOnlySpecified()
    {
        // Arrange
        CreateTestSave("World1", _backupPath);
        CreateTestSave("World2", _backupPath);
        var config = new global::Program.Config { Saves = new List<string> { "World1" } };
        var configPath = Path.Combine(_testRoot, "config.yaml");
        var serializer = new SerializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance).Build();
        File.WriteAllText(configPath, serializer.Serialize(config));

        // Act
        global::Program.ExecuteSync(new DirectoryInfo(_backupPath), new FileInfo(configPath), _minecraftSavesPath, global::Program.SyncDirection.Pull);

        // Assert
        Assert.True(Directory.Exists(Path.Combine(_minecraftSavesPath, "World1")));
        Assert.True(File.Exists(Path.Combine(_minecraftSavesPath, "World1", "level.dat")));
        Assert.False(Directory.Exists(Path.Combine(_minecraftSavesPath, "World2")));
    }

    [Fact]
    public void LoadConfig_ValidFile_ReturnsConfig()
    {
        // Arrange
        var configContent = "path: /fake/path\nsaves:\n  - World1";
        var configPath = Path.Combine(_testRoot, "test_config.yaml");
        File.WriteAllText(configPath, configContent);

        // Act
        var config = global::Program.LoadConfig(new FileInfo(configPath));

        // Assert
        Assert.NotNull(config);
        Assert.Equal("/fake/path", config.Path);
        Assert.Single(config.Saves);
        Assert.Equal("World1", config.Saves[0]);
    }

    [Fact]
    public void LoadConfig_NonExistentFile_ReturnsNull()
    {
        // Act
        var config = global::Program.LoadConfig(new FileInfo("non_existent_file.yaml"));

        // Assert
        Assert.Null(config);
    }

    public void Dispose()
    {
        if (Directory.Exists(_testRoot))
        {
            Directory.Delete(_testRoot, true);
        }
    }
}

// Removed test-only accessor. Tests now call Program APIs directly.