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

        private static readonly string[] spriteSheets = { "groundTiles", "characters", "characters_masks", "mapObjects" };
        private static Image<Rgba32>?[] bigImages = new Image<Rgba32>?[spriteSheets.Length];

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

            return GetSpriteInternal(spriteCoords, atlasId);
        }

        private static Image<Rgba32>? GetSpriteInternal(int[] coords, int atlasId)
        {
            if (coords.Length < 4)
            {
                return null;
            }

            int atlasFileIndex = atlasId - 1; // atlasId is 1-based.
            if (atlasFileIndex < 0 || atlasFileIndex >= spriteSheets.Length)
            {
                Debug.WriteLine($"[ImageBuffer] Warning: AtlasFileIndex {atlasFileIndex} is out of bounds for atlasId {atlasId}.");
                return null;
            }

            if (bigImages[atlasFileIndex] == null)
            {
                string key = spriteSheets[atlasFileIndex] + ".png";
                if (!RotMGAssetExtractor.BuildImages.TryGetValue(key, out var bytes) || bytes == null)
                {
                    Debug.WriteLine($"[ImageBuffer] Warning: Could not find image data for key: {key}");
                    return null;
                }
                try
                {
                    bigImages[atlasFileIndex] = Image.Load<Rgba32>(bytes);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[ImageBuffer] Error loading spritesheet {spriteSheets[atlasFileIndex]}: {ex.Message}");
                    return null;
                }
            }

            var cropRect = new Rectangle(coords[0], coords[1], coords[2], coords[3]);
            if (cropRect.Width == 0 || cropRect.Height == 0)
            {
                return EmptyImage();
            }

            return CropImage(bigImages[atlasFileIndex]!, cropRect);
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
        }
    }
}
