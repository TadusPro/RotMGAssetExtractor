using Google.FlatBuffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace RotMGAssetExtractor.Flatc
{
    public static class SpriteFlatBuffer
    {
        private static SpriteSheet? _spritesheet;
        private static Dictionary<string, Dictionary<int, (int AtlasId, int[] Coords)>> _spriteMap = new();

        public static void Reload()
        {
            if (RotMGAssetExtractor.BuildSpritesheetf.Length == 0) return;
            var buf = new Google.FlatBuffers.ByteBuffer(RotMGAssetExtractor.BuildSpritesheetf);
            _spritesheet = SpriteSheet.GetRootAsSpriteSheet(buf);
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
            if (!_spritesheet.HasValue) return;
            _spriteMap.Clear();

            // try to use valid name; fallback to first discovered sheet name
            var raw = _spritesheet.Value.Name;
            var sheetName = CleanName(raw);

            _spriteMap[sheetName] = new Dictionary<int, (int AtlasId, int[] Coords)>();

            for (int i = 0; i < _spritesheet.Value.SpritesLength; i++)
            {
                var sprite = _spritesheet.Value.Sprites(i);
                if (sprite == null || sprite.Value.Position == null) continue;
                var pos = sprite.Value.Position.Value;
                _spriteMap[sheetName][sprite.Value.Index] =
                    ((int)sprite.Value.AtlasId, new[] { (int)pos.X, (int)pos.Y, (int)pos.W, (int)pos.H });
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