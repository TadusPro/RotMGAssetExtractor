using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RotMGAssetExtractor
{
    public sealed class ExtractionSelection
    {
        // Image + model intent comes solely from these sets:
        public HashSet<string> ImageNames { get; } = new(StringComparer.OrdinalIgnoreCase);
        public HashSet<KnownImage> KnownImages { get; } = new();
        public HashSet<string> ModelTypes { get; } = new(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, HashSet<string>> ModelIdsByType { get; } = new(StringComparer.OrdinalIgnoreCase);

        // Internal flags for optional extras
        internal bool SpritesheetRequested { get; private set; }
        internal bool AllImagesRequested { get; private set; }

        // Fallback logic for unmapped images
        public bool FallbackFullScanForUnknownImages { get; set; } = true;

        // Derived intents
        internal bool WantsImages => AllImagesRequested || ImageNames.Count > 0 || KnownImages.Count > 0;
        internal bool WantsModels => ModelTypes.Count > 0 || ModelIdsByType.Count > 0;
        internal bool WantsSpritesheet => SpritesheetRequested;
        internal bool WantsAnything => WantsImages || WantsModels || WantsSpritesheet;

        // Asset needs
        internal bool RequiresResourcesAssets => WantsImages || WantsModels || WantsSpritesheet;

        // Full scan needed if spritesheet OR (images need fallback)
        internal bool RequiresFullScan(bool imageFallbackNeeded) =>
            WantsSpritesheet || imageFallbackNeeded;

        // Factories / fluent
        public static ExtractionSelection ForModels(params string[] types)
        {
            var sel = new ExtractionSelection();
            if (types is { Length: > 0 })
                sel.ModelTypes.UnionWith(types);
            return sel;
        }

        public ExtractionSelection RequestSpritesheet()
        {
            SpritesheetRequested = true;
            return this;
        }

        public ExtractionSelection AllImages()
        {
            AllImagesRequested = true;
            return this;
        }

        public ExtractionSelection AddImages(params string[] names)
        {
            foreach (var n in names)
            {
                if (string.IsNullOrWhiteSpace(n)) continue;
                ImageNames.Add(Path.GetFileNameWithoutExtension(n));
            }
            return this;
        }

        public ExtractionSelection AddKnown(params KnownImage[] imgs)
        {
            foreach (var k in imgs) KnownImages.Add(k);
            return this;
        }

        public ExtractionSelection AddModelTypes(params string[] types)
        {
            foreach (var t in types) if (!string.IsNullOrWhiteSpace(t)) ModelTypes.Add(t);
            return this;
        }

        public ExtractionSelection AddModelIds(string typeName, params string[] ids)
        {
            if (string.IsNullOrWhiteSpace(typeName) || ids == null) return this;
            if (!ModelIdsByType.TryGetValue(typeName, out var set))
            {
                set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                ModelIdsByType[typeName] = set;
            }
            foreach (var id in ids) if (!string.IsNullOrWhiteSpace(id)) set.Add(id);
            return this;
        }

        public bool ShouldIncludeImage(string textureName)
        {
            if (!WantsImages) return false;
            if (AllImagesRequested) return true;

            var bare = Path.GetFileNameWithoutExtension(textureName);

            if (ImageNames.Contains(bare) || ImageNames.Contains(textureName))
                return true;

            foreach (var ki in KnownImages)
            {
                var mapped = KnownImageMap.Map[ki].TextureName;
                if (mapped.Equals(bare, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }

        public bool ShouldIncludeModel(string typeName, string modelIdOrName)
        {
            if (!WantsModels) return false;
            if (ModelTypes.Contains(typeName)) return true;
            if (ModelIdsByType.TryGetValue(typeName, out var ids))
                return ids.Contains(modelIdOrName);
            return false; // No blanket IncludeModels flag anymore
        }
    }
}