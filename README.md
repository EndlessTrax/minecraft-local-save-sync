# Minecraft Local Save Sync (MLSS)

MLSS is a simple command-line tool to help you synchronize your Minecraft Java Edition local saves across multiple computers using a cloud or network drive.

## Features

- **Push & Pull**: Easily sync your local saves to and from a backup directory.
- **Configuration**: Support for a YAML configuration file to define backup paths and specific saves.
- **Selective Sync**: Sync all saves or only specific worlds defined in your config.
- **Auto-Discovery**: Automatically locates your Minecraft saves folder on Windows.

## Usage

The CLI supports two main commands: `push` and `pull`.

### Options

- `--path <directory>`: The target backup directory (e.g., a cloud or network drive).
- `--config <file>`: Path to a YAML configuration file.
- `--save <world-name>`: Sync a specific save/world. Overrides the saves list in config.
- `--help`: Show help information.

### Examples

**Using a direct path:**

```bash
mlss push --path "G:\My Drive\MinecraftBackups"
mlss pull --path "G:\My Drive\MinecraftBackups"
```

**Using a config file:**

```bash
mlss push --config config.yaml
mlss pull --config config.yaml
```

**Syncing a single save/world:**

```bash
# Sync only "My World" to the specified path
mlss push --path "G:\My Drive\MinecraftBackups" --save "My World"

# Use config for path, but sync only a specific save
mlss push --config config.yaml --save "Creative Test"
```

## Configuration

You can use a YAML file to store your backup path and list specific worlds to sync.

**Example `config.yaml`:**

```yaml
path: G:\My Drive\MinecraftBackups
saves:
  - Survival World
  - Creative Test
```

- `path`: The absolute path to your backup location.
- `saves`: A list of folder names for the worlds you want to sync. If omitted, **all** worlds will be synced.

**Note:** The `--save` command-line flag overrides the `saves` list in the config file. All other config values (like `path`) are still respected.

## Building from Source

You can also build and run the project from the source code.

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)

### Building

1. Clone the repository:

   ```bash
   git clone https://github.com/EndlessTrax/minecraft-local-save-sync.git
   cd minecraft-local-save-sync
   ```

2. Build the project:

   ```bash
   dotnet build --configuration Release
   ```

   The executable will be located in `bin/Release/net10.0/`.

### Running Locally

You can run the application directly using `dotnet run`.
