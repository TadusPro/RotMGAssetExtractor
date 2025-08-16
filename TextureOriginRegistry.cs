using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace RotMGAssetExtractor
{
    internal static class TextureOriginRegistry
    {
        private static readonly Dictionary<long, string> _pathToAsset = new();
        private static readonly Dictionary<string, string> _nameToAsset = new(StringComparer.OrdinalIgnoreCase);

        public static void Record(long pathId, string textureName, string assetFile)
        {
            if (!_pathToAsset.ContainsKey(pathId))
                _pathToAsset[pathId] = assetFile;

            // Keep first discovered asset file for a texture name
            if (!_nameToAsset.ContainsKey(textureName))
                _nameToAsset[textureName] = assetFile;
        }

        public static string? GetAssetForPath(long pathId) =>
            _pathToAsset.TryGetValue(pathId, out var v) ? v : null;

        public static string? GetAssetForName(string name) =>
            _nameToAsset.TryGetValue(name, out var v) ? v : null;

        public static void LogSuggestionsForUnknownImages(ExtractionSelection sel)
        {
            if (sel.ImageNames.Count == 0) return;

            foreach (var raw in sel.ImageNames)
            {
                var bare = System.IO.Path.GetFileNameWithoutExtension(raw);

                // Skip if already mapped in KnownImageMap
                if (KnownImageMap.TryGetByTextureName(bare, out _))
                    continue;

                var asset = GetAssetForName(bare);
                if (asset != null)
                {
                    Debug.WriteLine(
                        $"[ImageMapSuggestion] Unmapped requested image '{bare}' found in '{asset}'. " +
                        $"Add to KnownImage enum + map:  {{ KnownImage.YourEnumValue, (\"{bare}\", \"{asset}\") }}");
                }
                else
                {
                    Debug.WriteLine($"[ImageMapSuggestion] Requested image '{bare}' not found in scanned assets (yet).");
                }
            }
        }
    }
} 