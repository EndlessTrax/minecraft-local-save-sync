# Minecraft Local Save Sync (MLSS)

MLSS is a CLI simple tool that helps a Minecraft player to use the same local game save (world) across multiple machines. 

The CLI will `push` and `pull` save game data to a cloud or external drive.

## Basic Features

- Copy save files to and from a given drive as a backup.
- Support basic config file for easy use
- Support save locations for Minecraft Java
- Support Windows OS

## Potential future features

- Support MacOS and Linux
- Support Minecraft Bedrock

## CLI commands and flags

- `push`: Copy files from the `--path` location
- `pull`: Copy files to the `--path` location
- `--path`: The external, cloud, or network drive location. If no path is given, then check for a config file.
- `--config`: The path of the config file. Should have an OS specific default
- `--help`: Show the CLI help

CLI flags should override and take presedence over config parameters.

## Build

The CLI should be made available as a OS specific binary.
