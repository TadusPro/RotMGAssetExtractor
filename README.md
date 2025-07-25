# RotMGAssetExtractor

Extracts **Realm of the Mad God** (Unity) game assets automatically.

## Features
- Fetches latest build hash & version.
- Parses Unity `.assets` / `.resS` bundles.
- Exports textures to PNG (`gameData/images/`).
- Builds a sprite sheet + `spritesheet.xml`.
- **Parses TextAssets (XML) into strongly-typed C# model objects** — each "Object" XML element becomes an instance of its class (e.g. `Player`, `Equipment`, `FameBonus`) stored in `BuildModelsByType`.
- Caching to `gameData/` for fast startup.

## Requirements
- .NET 8 SDK
- Windows/macOS/Linux
- Internet access on first run

## Usage

```csharp
await RotMGAssetExtractor.RotMGAssetExtractor
    .InitAsync(Microsoft.Maui.Storage.FileSystem.AppDataDirectory);

// Access models
var players = RotMGAssetExtractor.RotMGAssetExtractor
    .BuildModelsByType["Player"];

