using BCnEncoder.Decoder;
using BCnEncoder.Shared;
using RotMGAssetExtractor.UnityExtractor.resextractor;

namespace RotMGAssetExtractor.UnityExtractor
{
    internal static class AdvancedImaging
    {
        internal static bool TryConvertTextureToBgra32(Texture2D tex, BcDecoder dec, out byte[] bgra)
        {
            bgra = null;
            int w = tex.Width, h = tex.Height;
            if (w == 0 || h == 0 || tex.ImageData == null) return false;

            int topLen;
            switch (tex.TextureFormat)
            {
                case TextureFormat.RGBA32:
                case TextureFormat.ARGB32:
                case TextureFormat.BGRA32:
                    topLen = w * h * 4;
                    if (tex.ImageData.Length < topLen) return false;

                    bgra = new byte[topLen];
                    for (int i = 0; i < topLen; i += 4)
                    {
                        bgra[i] = tex.ImageData[i + 2];
                        bgra[i + 1] = tex.ImageData[i + 1];
                        bgra[i + 2] = tex.ImageData[i];
                        bgra[i + 3] = tex.ImageData[i + 3];
                    }
                    return true;

                case TextureFormat.RGB24:
                    topLen = w * h * 3;
                    if (tex.ImageData.Length < topLen) return false;

                    bgra = new byte[w * h * 4];
                    for (int src = 0, dst = 0; src < topLen; src += 3, dst += 4)
                    {
                        bgra[dst] = tex.ImageData[src + 2];
                        bgra[dst + 1] = tex.ImageData[src + 1];
                        bgra[dst + 2] = tex.ImageData[src];
                        bgra[dst + 3] = 255;
                    }
                    return true;

                case TextureFormat.Alpha8:
                    topLen = w * h;
                    if (tex.ImageData.Length < topLen) return false;

                    bgra = new byte[w * h * 4];
                    for (int i = 0; i < topLen; ++i)
                    {
                        int p = i * 4;
                        bgra[p] = bgra[p + 1] = bgra[p + 2] = 255;
                        bgra[p + 3] = tex.ImageData[i];
                    }
                    return true;

                case TextureFormat.DXT1:
                case TextureFormat.DXT5:
                case TextureFormat.BC7:
                    return TryDecodeBlockCompressedTexture(tex, dec, out bgra);

                default:
                    return false;
            }
        }

        internal static bool TryDecodeBlockCompressedTexture(Texture2D tex, BcDecoder dec, out byte[] bgra)
        {
            int w = tex.Width, h = tex.Height;
            int blockSize = tex.TextureFormat == TextureFormat.DXT1 ? 8 : 16;
            int byteCount = ((w + 3) / 4) * ((h + 3) / 4) * blockSize;
            if (tex.ImageData.Length < byteCount) { bgra = null; return false; }

            var topMip = new byte[byteCount];
            Buffer.BlockCopy(tex.ImageData, 0, topMip, 0, byteCount);
            var pixels = dec.DecodeRaw(topMip, w, h, tex.TextureFormat switch
            {
                TextureFormat.DXT1 => CompressionFormat.Bc1,
                TextureFormat.DXT5 => CompressionFormat.Bc3,
                TextureFormat.BC7 => CompressionFormat.Bc7,
                _ => throw new NotSupportedException()
            });

            if (pixels == null) { bgra = null; return false; }

            bgra = new byte[w * h * 4];
            for (int i = 0, o = 0; i < pixels.Length; i++, o += 4)
            {
                bgra[o] = pixels[i].b;
                bgra[o + 1] = pixels[i].g;
                bgra[o + 2] = pixels[i].r;
                bgra[o + 3] = pixels[i].a;
            }
            return true;
        }
    }
}
