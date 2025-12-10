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
        global::Program.ExecuteSync(new DirectoryInfo(_backupPath), null, null, _minecraftSavesPath, global::Program.SyncDirection.Push);

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
        global::Program.ExecuteSync(new DirectoryInfo(_backupPath), new FileInfo(configPath), null, _minecraftSavesPath, global::Program.SyncDirection.Pull);

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

    [Fact]
    public void Push_WithSaveFlag_CopiesOnlySpecifiedSave()
    {
        // Arrange
        CreateTestSave("World1", _minecraftSavesPath);
        CreateTestSave("World2", _minecraftSavesPath);

        // Act
        global::Program.ExecuteSync(new DirectoryInfo(_backupPath), null, "World1", _minecraftSavesPath, global::Program.SyncDirection.Push);

        // Assert
        Assert.True(Directory.Exists(Path.Combine(_backupPath, "World1")));
        Assert.True(File.Exists(Path.Combine(_backupPath, "World1", "level.dat")));
        Assert.False(Directory.Exists(Path.Combine(_backupPath, "World2")));
    }

    [Fact]
    public void Pull_WithSaveFlag_CopiesOnlySpecifiedSave()
    {
        // Arrange
        CreateTestSave("World1", _backupPath);
        CreateTestSave("World2", _backupPath);

        // Act
        global::Program.ExecuteSync(new DirectoryInfo(_backupPath), null, "World2", _minecraftSavesPath, global::Program.SyncDirection.Pull);

        // Assert
        Assert.False(Directory.Exists(Path.Combine(_minecraftSavesPath, "World1")));
        Assert.True(Directory.Exists(Path.Combine(_minecraftSavesPath, "World2")));
        Assert.True(File.Exists(Path.Combine(_minecraftSavesPath, "World2", "level.dat")));
    }

    [Fact]
    public void Push_SaveFlagOverridesConfig_CopiesOnlyFlaggedSave()
    {
        // Arrange
        CreateTestSave("World1", _minecraftSavesPath);
        CreateTestSave("World2", _minecraftSavesPath);
        CreateTestSave("World3", _minecraftSavesPath);
        
        // Config specifies World1 and World2
        var config = new global::Program.Config { Saves = new List<string> { "World1", "World2" } };
        var configPath = Path.Combine(_testRoot, "config.yaml");
        var serializer = new SerializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance).Build();
        File.WriteAllText(configPath, serializer.Serialize(config));

        // Act - but --save flag should override config
        global::Program.ExecuteSync(new DirectoryInfo(_backupPath), new FileInfo(configPath), "World3", _minecraftSavesPath, global::Program.SyncDirection.Push);

        // Assert - only World3 should be copied
        Assert.False(Directory.Exists(Path.Combine(_backupPath, "World1")));
        Assert.False(Directory.Exists(Path.Combine(_backupPath, "World2")));
        Assert.True(Directory.Exists(Path.Combine(_backupPath, "World3")));
        Assert.True(File.Exists(Path.Combine(_backupPath, "World3", "level.dat")));
    }

    [Fact]
    public void Push_SaveFlagWithConfigPath_UsesConfigPathAndFlaggedSave()
    {
        // Arrange
        CreateTestSave("World1", _minecraftSavesPath);
        CreateTestSave("World2", _minecraftSavesPath);
        
        // Config with a path and saves list
        var config = new global::Program.Config 
        { 
            Path = _backupPath,
            Saves = new List<string> { "World1" } 
        };
        var configPath = Path.Combine(_testRoot, "config.yaml");
        var serializer = new SerializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance).Build();
        File.WriteAllText(configPath, serializer.Serialize(config));

        // Act - using config for path, but --save flag for specific save
        global::Program.ExecuteSync(null, new FileInfo(configPath), "World2", _minecraftSavesPath, global::Program.SyncDirection.Push);

        // Assert - path from config, but only World2 (from --save flag) should be copied
        Assert.False(Directory.Exists(Path.Combine(_backupPath, "World1")));
        Assert.True(Directory.Exists(Path.Combine(_backupPath, "World2")));
        Assert.True(File.Exists(Path.Combine(_backupPath, "World2", "level.dat")));
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