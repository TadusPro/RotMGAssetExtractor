using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RotMGAssetExtractor
{
    // Add entries as you confirm their asset file origin.
    public enum KnownImage
    {
        GroundTiles,
        Characters,
        CharactersMasks,
        MapObjects
        // Add more known texture groups here...
    }

    internal static class KnownImageMap
    {
        // (KnownImage) -> (textureNameInUnity, assetFileName)
        // If a texture lives in resources.assets just put that; otherwise the exact *.assets filename.
        internal static readonly Dictionary<KnownImage, (string TextureName, string AssetFile)> Map =
            new()
            {
                { KnownImage.GroundTiles, ("groundTiles", "resources.assets") },
                { KnownImage.Characters, ("characters", "resources.assets") },
                { KnownImage.CharactersMasks, ("characters_masks", "resources.assets") },
                //{ KnownImage.MapObjects, ("mapObjects", "resources.assets") }
            };

        internal static bool TryGetByTextureName(string textureName, out (KnownImage ki, (string TextureName, string AssetFile) info) result)
        {
            foreach (var kv in Map)
            {
                if (string.Equals(kv.Value.TextureName, textureName, StringComparison.OrdinalIgnoreCase))
                {
                    result = (kv.Key, kv.Value);
                    return true;
                }
            }
            result = default;
            return false;
        }
    }
}
