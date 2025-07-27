using RotMGAssetExtractor.ModelHelpers;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.Diagnostics;
using System.Text;

namespace RotMGAssetExtractor.Flatc
{
    /// <summary>
    /// Image lookup + caching for spritesheets. Exposes **one** public entry point:
    ///     <c>ImageBuffer.GetImage(Texture texture)</c>
    /// Everything else is private implementation detail.
    /// </summary>
    public static class ImageBuffer
    {
        private static readonly Dictionary<int, Image<Rgba32>> images = new();
        private static readonly Dictionary<int, float[]> colors = new();
        private static Image<Rgba32>? emptyImg;

        private static readonly string[] spriteSheets = { 
            "characters", 
            "groundTiles", 
            "characters_masks", 
            "mapObjects" 
        };
        private static Image<Rgba32>?[] bigImages = new Image<Rgba32>?[spriteSheets.Length];
        private static bool _areAtlasesLoaded = false;
        private static readonly object _loadLock = new object();

        public static Task LoadAllAtlasesAsync()
        {
            lock (_loadLock)
            {
                if (_areAtlasesLoaded)
                {
                    return Task.CompletedTask;
                }
            }

            return Task.Run(() =>
            {
                lock (_loadLock)
                {
                    if (_areAtlasesLoaded) return;

                    for (int i = 0; i < spriteSheets.Length; i++)
                    {
                        if (bigImages[i] == null)
                        {
                            var key = spriteSheets[i] + ".png";
                            if (RotMGAssetExtractor.BuildImages.TryGetValue(key, out var bytes))
                            {
                                try
                                {
                                    bigImages[i] = Image.Load<Rgba32>(bytes);
                                }
                                catch (Exception ex)
                                {
                                    Debug.WriteLine($"[ImageBuffer] Error loading atlas '{key}': {ex.Message}");
                                }
                            }
                            else
                            {
                                Debug.WriteLine($"[ImageBuffer] Could not find atlas data for '{key}'.");
                            }
                        }
                    }
                    _areAtlasesLoaded = true;
                }
            });
        }

        /// <summary>
        /// Returns the sprite image for the given Texture, or null if not found.
        /// </summary>
        public static Image<Rgba32>? GetImage(ITexture texture)
        {
            if (texture == null)
            {
                return null;
            }

            string? fileName = texture.File;
            int itemOffset = texture.Index;

            if (string.IsNullOrEmpty(fileName) || itemOffset == -1)
            {
                return null;
            }

            var spriteCoords = SpriteFlatBuffer.GetSpriteCoordinates(fileName, itemOffset);
            if (spriteCoords == null || spriteCoords.Length < 4)
            {
                Debug.WriteLine($"[ImageBuffer] Warning: Could not find sprite for '{fileName}' at index {itemOffset}.");
                return null;
            }

            if (spriteCoords[0] == 0 && spriteCoords[1] == 0 && spriteCoords[2] == 0 && spriteCoords[3] == 0)
            {
                //Debug.WriteLine($"[ImageBuffer] Warning: Sprite coordinates for '{fileName}' at index {itemOffset} are all zero.");
            }

            int atlasId = SpriteFlatBuffer.GetSpriteAtlasId(fileName, itemOffset);
            if (atlasId == 0)
            {
                return null;
            }

            return GetSpriteInternal(spriteCoords, atlasId, fileName, itemOffset);
        }

        private static Image<Rgba32>? GetSpriteInternal(int[] coords, int atlasId, string fileName, int itemOffset)
        {
            if (coords.Length < 4) return null;

            int atlasFileIndex = atlasId - 1;
            if (atlasFileIndex < 0 || atlasFileIndex >= spriteSheets.Length) return null;

            if (bigImages[atlasFileIndex] == null)
            {
                Debug.WriteLine($"[ImageBuffer] Atlas '{spriteSheets[atlasFileIndex]}' not loaded.");
                return null;
            }

            var img = bigImages[atlasFileIndex]!;
            var cropRect = new Rectangle(coords[0], coords[1], coords[2], coords[3]);

            // Validate/clamp
            if (cropRect.Width <= 0 || cropRect.Height <= 0) return EmptyImage();
            if (cropRect.Right > img.Width || cropRect.Bottom > img.Height ||
                cropRect.X < 0 || cropRect.Y < 0)
            {
                Debug.WriteLine($"[ImageBuffer] Warning: crop out of bounds for '{fileName}' at index {itemOffset}. " +
                                $"AtlasId: {atlasFileIndex}, " +
                                $"CropRect: {cropRect.X},{cropRect.Y},{cropRect.Width},{cropRect.Height}, " +
                                $"ImageSize: {img.Width}x{img.Height}");

                Debug.WriteLine("[ImageBuffer] Currently loaded atlases:");
                for (int i = 0; i < bigImages.Length; i++)
                {
                    var atlasImage = bigImages[i];
                    var atlasName = spriteSheets[i];
                    var currentAtlasId = i + 1;
                    if (atlasImage != null)
                    {
                        Debug.WriteLine($" - Name: {atlasName}, Size: {atlasImage.Width}x{atlasImage.Height}, AtlasId: {currentAtlasId}");
                    }
                    else
                    {
                        Debug.WriteLine($" - Name: {atlasName}, Not loaded, AtlasId: {currentAtlasId}");
                    }
                }
                return null;
            }

            try { return img.Clone(c => c.Crop(cropRect)); }
            catch (Exception ex)
            {
                Debug.WriteLine("[ImageBuffer] Crop error: " + ex.Message);
                return null;
            }
        }


        private static Image<Rgba32> CropImage(Image<Rgba32> source, Rectangle cropRect)
            => source.Clone(ctx => ctx.Crop(cropRect));

        private static Image<Rgba32> EmptyImage()
        {
            if (emptyImg != null) return emptyImg;
            emptyImg = CreateTransparentImage(8, 8);
            return emptyImg;
        }

        public static Image<Rgba32> GetEmptyImg() => EmptyImage();

        public static Image<Rgba32> CreateTransparentImage(int width, int height)
        {
            var image = new Image<Rgba32>(width, height);
            image.Mutate(ctx => ctx.Clear(SixLabors.ImageSharp.Color.Transparent));
            return image;
        }

        public static void Clear()
        {
            foreach (var img in images.Values) img.Dispose();
            images.Clear();

            foreach (var img in bigImages) img?.Dispose();
            bigImages = new Image<Rgba32>?[spriteSheets.Length];

            colors.Clear();
            emptyImg?.Dispose();
            emptyImg = null;
            _areAtlasesLoaded = false;
        }
    }
}
