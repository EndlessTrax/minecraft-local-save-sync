?using System.CommandLine;
using System.IO;
using System.Threading.Tasks;

class Program
{
    static async Task<int> Main(string[] args)
    {
        var pathOption = new Option<DirectoryInfo>(
            name: "--path",
            description: "The target backup directory (e.g., a cloud or network drive).")
        {
            IsRequired = true
        };

        var rootCommand = new RootCommand("Minecraft Local Save Sync (MLSS)");

        var pushCommand = new Command("push", "Push saves from the local Minecraft directory to the backup directory.");
        pushCommand.AddOption(pathOption);

        var pullCommand = new Command("pull", "Pull saves from the backup directory to the local Minecraft directory.");
        pullCommand.AddOption(pathOption);

        var minecraftSavesPath = GetMinecraftSavesPath();
        if (string.IsNullOrEmpty(minecraftSavesPath))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Error: Could not find the Minecraft saves directory.");
            Console.ResetColor();
            return 1;
        }

        pushCommand.SetHandler((path) =>
        {
            Console.WriteLine("Pushing saves to backup directory...");
            CopyDirectory(minecraftSavesPath, path.FullName, true);
            Console.WriteLine("Push complete.");
        }, pathOption);

        pullCommand.SetHandler((path) =>
        {
            Console.WriteLine("Pulling saves from backup directory...");
            CopyDirectory(path.FullName, minecraftSavesPath, true);
            Console.WriteLine("Pull complete.");
        }, pathOption);

        rootCommand.AddCommand(pushCommand);
        rootCommand.AddCommand(pullCommand);

        return await rootCommand.InvokeAsync(args);
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