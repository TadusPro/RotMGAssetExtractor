# RotMGAssetExtractor

Extracts **Realm of the Mad God** (Unity) game assets automatically.

## Features
- Fetches latest build hash & version.
- Parses Unity `.assets` / `.resS` bundles.
- Exports textures to PNG (`gameData/images/`).
- Builds a sprite sheet + `spritesheet.xml`.
- **Parses TextAssets (XML) into strongly-typed C# model objects** — each "Object" XML element becomes an instance of its class (e.g. `Player`, `Equipment`, `FameBonus`) stored in `BuildModelsByType`.
- Caching to `gameData/` for fast startup.
- **Selective Asset Extraction**: Choose exactly which asset types to download and process.

## Requirements
- .NET 8 SDK
- Windows/macOS/Linux
- Internet access on first run

## Usage

Initialize the extractor by calling `InitAsync`. You can either extract all assets (the default behavior) or specify which asset types you need.

### Extract All Assets

This is the simplest way to use the extractor. It will download and process all available game assets.

