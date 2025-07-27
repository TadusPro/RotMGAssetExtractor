using Google.FlatBuffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace RotMGAssetExtractor.Flatc
{
    public static class SpriteFlatBuffer
    {
        private static SpriteSheetRoot? _spritesheetRoot;
        private static Dictionary<string, Dictionary<int, (int AtlasId, int[] Coords)>> _spriteMap = new();

        public static void Reload()
        {
            if (RotMGAssetExtractor.BuildSpritesheetf.Length == 0) return;
            var buf = new Google.FlatBuffers.ByteBuffer(RotMGAssetExtractor.BuildSpritesheetf);
            _spritesheetRoot = SpriteSheetRoot.GetRootAsSpriteSheetRoot(buf);
            BuildSpriteMap();
        }

        public static void LoadFromDecompiled(DecompiledSpriteSheet decompiledSheet)
        {
            _spriteMap.Clear();
            foreach (var group in decompiledSheet.SpriteGroups)
            {
                var spriteGroup = new Dictionary<int, (int AtlasId, int[] Coords)>();
                foreach (var sprite in group.Sprites)
                {
                    spriteGroup[sprite.Index] = (sprite.AtlasId, new[] { sprite.X, sprite.Y, sprite.W, sprite.H });
                }
                _spriteMap[group.Name] = spriteGroup;
            }
        }

        private static string CleanName(string s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            var filtered = new string(s.Where(c => c == '\t' || c == '\n' || c == '\r' || c >= 0x20).ToArray());
            return filtered.Trim();
        }

        private static void BuildSpriteMap()
        {
            if (!_spritesheetRoot.HasValue)
            {
                Debug.WriteLine("[SpriteFlatBuffer] BuildSpriteMap: _spritesheetRoot is null. Aborting.");
                return;
            }
            Debug.WriteLine("[SpriteFlatBuffer] BuildSpriteMap: Clearing existing sprite map.");
            _spriteMap.Clear();

            var totalSheets = _spritesheetRoot.Value.SpritesLength;
            Debug.WriteLine($"[SpriteFlatBuffer] BuildSpriteMap: Found {totalSheets} sprite sheets in the root.");

            for (int i = 0; i < totalSheets; i++)
            {
                var sheet = _spritesheetRoot.Value.Sprites(i);
                if (!sheet.HasValue)
                {
                    Debug.WriteLine($"[SpriteFlatBuffer] BuildSpriteMap: Sheet at index {i} is null. Skipping.");
                    continue;
                }

                var currentSheet = sheet.Value;
                var sheetName = CleanName(currentSheet.Name);
                var sheetAtlasId = (int)currentSheet.AtlasId;
                Debug.WriteLine($"[SpriteFlatBuffer] BuildSpriteMap: Processing sheet index {i}: Name='{sheetName}', AtlasId={sheetAtlasId}");


                if (string.IsNullOrEmpty(sheetName))
                {
                    sheetName = $"spritesheet_{sheetAtlasId}";
                    Debug.WriteLine($"[SpriteFlatBuffer] BuildSpriteMap: Sheet name was empty. Generated name: '{sheetName}'");
                }

                if (!_spriteMap.ContainsKey(sheetName))
                {
                    _spriteMap[sheetName] = new Dictionary<int, (int AtlasId, int[] Coords)>();
                    Debug.WriteLine($"[SpriteFlatBuffer] BuildSpriteMap: Created new sprite map entry for '{sheetName}'.");
                }

                var totalSpritesInSheet = currentSheet.SpritesLength;
                Debug.WriteLine($"[SpriteFlatBuffer] BuildSpriteMap: Sheet '{sheetName}' contains {totalSpritesInSheet} sprites.");


                for (int j = 0; j < totalSpritesInSheet; j++)
                {
                    var sprite = currentSheet.Sprites(j);
                    if (!sprite.HasValue || !sprite.Value.Position.HasValue)
                    {
                        Debug.WriteLine($"[SpriteFlatBuffer] BuildSpriteMap: Sprite at index {j} in sheet '{sheetName}' is null or has no position. Skipping.");
                        continue;
                    }

                    var pos = sprite.Value.Position.Value;
                    var spriteIndex = sprite.Value.Index;

                    var coords = new[] { (int)pos.X, (int)pos.Y, (int)pos.W, (int)pos.H };
                    _spriteMap[sheetName][spriteIndex] = (sheetAtlasId, coords);
                    
                    //Debug.WriteLine($"[SpriteFlatBuffer] BuildSpriteMap: Mapped sprite for '{sheetName} {spriteIndex}', AtlasId: {sheetAtlasId}, Coords: [{string.Join(",", coords)}]");
                }
            }
            Debug.WriteLine($"[SpriteFlatBuffer] BuildSpriteMap: Finished processing. Total sprite groups in map: {_spriteMap.Count}");

            // Add a warning if the 'players' or 'playerskins' sprite sheets are not found.
            if (!_spriteMap.ContainsKey("players"))
            {
                Debug.WriteLine("[SpriteFlatBuffer] WARNING: The sprite map does not contain the 'players' group.");
            }
            if (!_spriteMap.ContainsKey("playerskins"))
            {
                Debug.WriteLine("[SpriteFlatBuffer] WARNING: The sprite map does not contain the 'playerskins' group.");
            }
        }



        public static int[] GetSpriteCoordinates(string name, int index)
        {
            if (_spriteMap.TryGetValue(name, out var group) && group.TryGetValue(index, out var spriteInfo))
            {
                return spriteInfo.Coords;
            }
            return new int[] { 0, 0, 0, 0 };
        }

        public static int GetSpriteAtlasId(string name, int index)
        {
            if (_spriteMap.TryGetValue(name, out var group) && group.TryGetValue(index, out var spriteInfo))
            {
                return spriteInfo.AtlasId;
            }
            return 0;
        }

        public static Dictionary<string, Dictionary<int, (int AtlasId, int[] Coords)>> GetSprites()
        {
            return _spriteMap;
        }
    }
}