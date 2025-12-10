using System.CommandLine;
using System.IO;
using System.Threading.Tasks;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

public partial class Program
{
    public class Config
    {
        public string? Path { get; set; }
        public List<string>? Saves { get; set; }
    }

    static async Task<int> Main(string[] args)
    {
        var pathOption = new Option<DirectoryInfo>(
            name: "--path",
            description: "The target backup directory (e.g., a cloud or network drive).");

        var configOption = new Option<FileInfo>(
            name: "--config",
            description: "Path to the YAML configuration file.");

        var saveOption = new Option<string>(
            name: "--save",
            description: "Sync a specific save/world. Overrides the saves list in config.");

        var rootCommand = new RootCommand("Minecraft Local Save Sync (MLSS)");
        rootCommand.AddGlobalOption(configOption);

        var pushCommand = new Command("push", "Push saves from the local Minecraft directory to the backup directory.");
        pushCommand.AddOption(pathOption);
        pushCommand.AddOption(saveOption);

        var pullCommand = new Command("pull", "Pull saves from the backup directory to the local Minecraft directory.");
        pullCommand.AddOption(pathOption);
        pullCommand.AddOption(saveOption);

        var minecraftSavesPath = GetMinecraftSavesPath();
        if (string.IsNullOrEmpty(minecraftSavesPath))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Error: Could not find the Minecraft saves directory.");
            Console.ResetColor();
            return 1;
        }

        pushCommand.SetHandler((path, configFile, save) =>
        {
            ExecuteSync(path, configFile, save, minecraftSavesPath, SyncDirection.Push);
        }, pathOption, configOption, saveOption);

        pullCommand.SetHandler((path, configFile, save) =>
        {
            ExecuteSync(path, configFile, save, minecraftSavesPath, SyncDirection.Pull);
        }, pathOption, configOption, saveOption);

        rootCommand.AddCommand(pushCommand);
        rootCommand.AddCommand(pullCommand);

        return await rootCommand.InvokeAsync(args);
    }

    public enum SyncDirection { Push, Pull }

    public static void ExecuteSync(DirectoryInfo? path, FileInfo? configFile, string? save, string minecraftSavesPath, SyncDirection direction)
    {
        var config = LoadConfig(configFile);

        var backupPath = path?.FullName ?? config?.Path;
        if (string.IsNullOrEmpty(backupPath))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Error: Backup path must be provided via --path option or in the config file.");
            Console.ResetColor();
            return;
        }

        if (!Directory.Exists(backupPath))
        {
            // Error out if the directory does not exist.
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Error: Backup folder '{backupPath}' does not exist.");
            Console.ResetColor();
            return;
        }

        var action = direction == SyncDirection.Push ? "Pushing" : "Pulling";
        Console.WriteLine($"{action} saves...");

        // If --save flag is provided, it overrides config saves
        var savesToSync = !string.IsNullOrEmpty(save)
            ? new List<string> { save.Trim() }
            : config?.Saves?.Select(s => s.Trim()).ToList();

        if (savesToSync == null || savesToSync.Count == 0)
        {
            // Sync all saves
            var source = direction == SyncDirection.Push ? minecraftSavesPath : backupPath;
            var destination = direction == SyncDirection.Push ? backupPath : minecraftSavesPath;
            CopyDirectory(source, destination, true);
        }
        else
        {
            // Sync only specified saves
            foreach (var saveName in savesToSync)
            {
                var source = direction == SyncDirection.Push ? Path.Combine(minecraftSavesPath, saveName) : Path.Combine(backupPath, saveName);
                var destination = direction == SyncDirection.Push ? Path.Combine(backupPath, saveName) : Path.Combine(minecraftSavesPath, saveName);

                if (Directory.Exists(source))
                {
                    Console.WriteLine($"  - {saveName}");
                    CopyDirectory(source, destination, true);
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"Warning: Save folder '{saveName}' not found at source. Skipping.");
                    Console.ResetColor();
                }
            }
        }

        Console.WriteLine("Sync complete.");
    }

    public static Config? LoadConfig(FileInfo? configFile)
    {
        if (configFile == null || !configFile.Exists)
        {
            return null;
        }

        try
        {
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();
            return deserializer.Deserialize<Config>(File.ReadAllText(configFile.FullName));
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Error reading or parsing config file: {ex.Message}");
            Console.ResetColor();
            return null;
        }
    }

    static string? GetMinecraftSavesPath()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var minecraftPath = Path.Combine(appData, ".minecraft", "saves");

        return Directory.Exists(minecraftPath) ? minecraftPath : null;
    }

    static void CopyDirectory(string sourceDir, string destinationDir, bool recursive)
    {
        var dir = new DirectoryInfo(sourceDir);

        if (!dir.Exists)
            throw new DirectoryNotFoundException($"Source directory not found: {dir.FullName}");

        DirectoryInfo[] dirs = dir.GetDirectories();
        Directory.CreateDirectory(destinationDir);

        foreach (FileInfo file in dir.GetFiles())
        {
            string targetFilePath = Path.Combine(destinationDir, file.Name);
            file.CopyTo(targetFilePath, true);
        }

        if (recursive)
        {
            foreach (DirectoryInfo subDir in dirs)
            {
                string newDestinationDir = Path.Combine(destinationDir, subDir.Name);
                CopyDirectory(subDir.FullName, newDestinationDir, true);
            }
        }
    }
}