# ReysRitoFixer by ProjectRey
Welcome to **ReysRitoFixer**, a tool designed to help the League of Legends modding community resolve issues related to texture paths in `.bin` files. With ReysRitoFixer, you can make use of two separate modes, `ReysRitoFixer 25.S1.3` and `ReysRitoFixer 25.S1.4 (PBE)`, which were created to address specific issues within Mod modifications.
## Features
- Automatically **fix texture parameter issues** where `TextureName` must be replaced with `TexturePath`.
- Simplifies the process of extracting, modifying, and repacking `.wad.client` files.
- Includes an additional replacement layer in **PBE mode** for handling more edge cases.
- Provides automatic backups of installed files to ensure safe modding.

## How It Works
1. **Identify and Fix Parameter Issues**:
    - The tool replaces instances of `TextureName` with `TexturePath` in `.py` files.
    - This fixes the "white texture issue" where invalid mappings are referenced in the game.

2. **Automated Workflow Pipeline**:
    - Extracts `.wad.client` files.
    - Converts `.bin` files to `.py` for easier editing.
    - Replaces instances of problematic parameters.
    - Recompiles `.py` files into `.bin` files.
    - Recreates the `.wad.client` files.

3. **Backup Capability**:
    - The tool safely backs up your installed files so you can restore them if needed.

4. **Handle Hash Files**:
    - Ensures necessary hash files are downloaded and in place before proceeding.

## Known Issues
### ❗ White Texture Issue
The **white texture issue** occurs when incorrect parameter mappings are loaded into the game. This happens because the parameter `TextureName` is no longer appropriate for certain engine versions.
- **Fixable Issue**:
The tool resolves cases where the parameter needs to be updated to `TexturePath`.

✅ Example Fix:
``` py
  textureName: string = "sample_textures/example.dds"
```
➡
``` py
  texturePath: string = "sample_textures/example.dds"
```
- **Unfixable Issue**:
If a texture is **missing entirely**, the tool cannot recover or generate missing files. This issue must be resolved by manually adding or downloading the relevant texture files.

## Modes of Operation
The tool offers two modes:
### 1️⃣ **ReysRitoFixer 25.S1.3**
This mode uses a single layer of replacement logic for fixing texture-related issues, targeting only `TextureName` to `TexturePath` replacements.
### 2️⃣ **ReysRitoFixer 25.S1.4 (PBE)**
This mode includes **additional fixes** for newer engine versions. It handles not only `TextureName` replacements but also issues with `SamplerName` parameters.
## Prerequisites
Before using ReysRitoFixer, ensure the following are set up:
1. **CSLoL Manager**
The tool requires **CSLoL Manager** to be running for access to the installed mods directory.
2. **Required Hash Files**
Ensure the tool has access to the necessary hash files (`hashes.game.txt`, `hashes.lcu.txt`, etc.). If not, ReysRitoFixer will download them automatically.

## Installation & Setup (Developer only)
1. Clone or download the ReysRitoFixer tool into a desired directory.
2. Install **.NET 8.0 Runtime** (if not already installed) as the tool was developed in C# targeting `.NET 8.0`.
3. Run the tool using the following command:
``` bash
   dotnet run --project ReysRitoFixer
```
1. Follow the on-screen instructions.

## Usage Instructions
- Run the program and choose a mode:
    - **Option 1 - ReysRitoFixer 25.S1.3**: Basic parameter replacement fixes.
    - **Option 2 - ReysRitoFixer 25.S1.4 (PBE)**: Extended fixes for PBE or newer clients.
    - **Option 3**: Exit the program.

- Ensure **CSLoL Manager** is running.
- Allow the tool to process the necessary files.

## Contributing
To contribute to the development of ReysRitoFixer:
1. Fork the repository.
2. Create a feature branch.
3. Submit a Pull Request (PR) describing your changes.

We welcome all contributions to improve the functionality and usability of this tool!
## Credits
ReysRitoFixer is proudly developed by **ProjectRey** with contributions from the League of Legends modding community.
It uses the Custom Skin Manager Tools by moonshadow565 / LeagueToolkit: https://github.com/LeagueToolkit/cslol-manager
