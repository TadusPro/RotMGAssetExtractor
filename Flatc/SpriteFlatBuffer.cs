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

            var root = _spritesheetRoot.Value;

            // Process static sprites
            var totalSheets = root.SpritesLength;
            Debug.WriteLine($"[SpriteFlatBuffer] BuildSpriteMap: Found {totalSheets} static sprite sheets.");
            for (int i = 0; i < totalSheets; i++)
            {
                var sheet = root.Sprites(i);
                if (!sheet.HasValue) continue;

                var currentSheet = sheet.Value;
                var sheetName = CleanName(currentSheet.Name);
                var sheetAtlasId = (int)currentSheet.AtlasId;

                if (string.IsNullOrEmpty(sheetName)) continue;

                if (!_spriteMap.ContainsKey(sheetName))
                {
                    _spriteMap[sheetName] = new Dictionary<int, (int AtlasId, int[] Coords)>();
                }

                for (int j = 0; j < currentSheet.SpritesLength; j++)
                {
                    var sprite = currentSheet.Sprites(j);
                    if (!sprite.HasValue || !sprite.Value.Position.HasValue) continue;

                    var pos = sprite.Value.Position.Value;
                    _spriteMap[sheetName][(int)sprite.Value.Index] =
                        (sheetAtlasId, new[] { (int)pos.X, (int)pos.Y, (int)pos.W, (int)pos.H });
                }
            }

            // Process animated sprites
            var totalAnimatedSheets = root.AnimatedSpritesLength;
            Debug.WriteLine($"[SpriteFlatBuffer] BuildSpriteMap: Found {totalAnimatedSheets} animated sprite sheets.");
            for (int i = 0; i < totalAnimatedSheets; i++)
            {
                var animatedSheet = root.AnimatedSprites(i);
                if (!animatedSheet.HasValue || !animatedSheet.Value.Sprite.HasValue || !animatedSheet.Value.Sprite.Value.Position.HasValue) continue;

                var currentAnimatedSheet = animatedSheet.Value;
                var sheetName = CleanName(currentAnimatedSheet.Name);
                var sprite = currentAnimatedSheet.Sprite.Value;
                var pos = sprite.Position.Value;
                
                // Animated sprites seem to have their AtlasId on the nested sprite object itself.
                var atlasId = (int)sprite.AtlasId; 

                if (string.IsNullOrEmpty(sheetName)) continue;

                if (!_spriteMap.ContainsKey(sheetName))
                {
                    _spriteMap[sheetName] = new Dictionary<int, (int AtlasId, int[] Coords)>();
                }

                _spriteMap[sheetName][(int)currentAnimatedSheet.Index] =
                    (atlasId, new[] { (int)pos.X, (int)pos.Y, (int)pos.W, (int)pos.H });
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