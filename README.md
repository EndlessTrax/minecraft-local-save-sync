# Minecraft Local Save Sync (MLSS)

MLSS is a simple command-line tool to help you synchronize your Minecraft Java Edition local saves across multiple computers using a cloud or network drive.

## Features

-   `push` your local saves to a backup directory.
-   `pull` your saves from a backup directory.
-   Automatically locates your Minecraft saves folder on Windows.

## Usage

To use the MLSS tool, you can run the `push` and `pull` commands with the path to your backup directory.

### Push
Copy your local Minecraft saves to your backup directory.

### Pull
Copy the saves from your backup directory to your local Minecraft saves folder.

## Building from Source

You can also build and run the project from the source code.

### Prerequisites

-   [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)

### Building

1.  Clone the repository:
    ```bash
    git clone https://github.com/EndlessTrax/minecraft-local-save-sync.git
    cd minecraft-local-save-sync
    ```

2.  Build the project:
    ```bash
    dotnet build --configuration Release
    ```
    The executable will be located in `bin/Release/net10.0/`.

### Running Locally

You can run the application directly using `dotnet run`.
