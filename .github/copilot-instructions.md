# Minecraft Local Save Sync (MLSS)

MLSS is a CLI simple tool that helps a Minecraft player to use the same local game save (world) across multiple machines. 

The CLI will `push` and `pull` save game data to a cloud or external drive.

## Basic Features

- Copy save files to and from a given drive as a backup.
- Support YAML config file for easy use.
- Support syncing all saves or specific saves defined in config.
- Support save locations for Minecraft Java.
- Support Windows OS.

## Potential future features

- Support MacOS and Linux
- Support Minecraft Bedrock

## CLI commands and flags

- `push`: Copy files from the local Minecraft saves directory to the backup location.
- `pull`: Copy files from the backup location to the local Minecraft saves directory.
- `--path`: The external, cloud, or network drive location. If no path is given, then check for a config file.
- `--config`: The path of the YAML config file.
- `--help`: Show the CLI help.

CLI flags should override and take precedence over config parameters.

## Configuration

The configuration file is a YAML file with the following structure:

```yaml
path: /path/to/backup/drive
saves:
  - World1
  - World2
```

- `path`: The backup directory path.
- `saves`: A list of specific save folder names to sync. If omitted or empty, all saves are synced.

## Build

The project is built using .NET 10.0.
The CLI is compiled into an executable named `mlss`.

## Project Structure

- `src/`: Contains the main application code.
- `tests/`: Contains unit tests using xUnit.
