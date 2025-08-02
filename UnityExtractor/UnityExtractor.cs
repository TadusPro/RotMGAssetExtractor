using BCnEncoder.Decoder;
using RotMGAssetExtractor.Flatc;
using RotMGAssetExtractor.UnityExtractor;
using RotMGAssetExtractor.UnityExtractor.resextractor;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Xml.Serialization;
using ModelObject = RotMGAssetExtractor.Model.Object;

namespace RotMGAssetExtractor.Parser
{
    public class UnityExtractor
    {
        private readonly Dictionary<string, int> unrecognizedElementCounts = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, Dictionary<string, int>> unusedAttrByNode = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, Dictionary<string, int>> unusedChildByNode = new(StringComparer.OrdinalIgnoreCase);

        public string GetUnityVersionFromMetadata(byte[] metadata)
        {
            var text = Encoding.ASCII.GetString(metadata);
            var match = Regex.Matches(text, @"\b\d+\.\d+\.\d+\.\d+\.\d+\b");
            return match.Count == 1 ? match[0].Value : throw new InvalidOperationException("Version not found / multiple found");
        }
        public Resources ProccessResource(string name, byte[] assetsData, byte[]? resSData = null)
        {
            // hook “child-node” warning stores
            unusedAttrByNodeStatic = unusedAttrByNode;
            unusedChildByNodeStatic = unusedChildByNode;
            var res = new Resources(assetsData, resSData);
            var sb = new StringBuilder("[GameData] Processing ").Append(name);

            var types = RotMGAssetExtractor.ExtractionTypes;

            if (types.Contains(ExtractionType.All) && res.assetSpriteAtlases.Any())
            {
                sb.Append(" | SpriteAtlases: ").Append(res.assetSpriteAtlases.Count);
            }

            if ((types.Contains(ExtractionType.All) || types.Contains(ExtractionType.ImagesLight) || types.Contains(ExtractionType.ImagesAll)) && res.assetTexture2Ds.Any())
            {
                sb.Append(" | Texture2Ds: ").Append(res.assetTexture2Ds.Count);
            }

            if ((types.Contains(ExtractionType.All) || types.Contains(ExtractionType.Models)) && res.assetTextAssets.Any())
            {
                sb.Append(" | TextAssets: ").Append(res.assetTextAssets.Count);
            }
            if (types.Contains(ExtractionType.All) && res.spritesheet != null)
            {
                sb.Append(" | Spritesheet: ").Append(res.spritesheet.Name);
            }
            Debug.WriteLine(sb.ToString());
            return res;
        }

        internal void UnusedNodesDebug()
        {
            // summary diagnostics
            PrintCounts("Unknown nodes", unrecognizedElementCounts);
            PrintNestedCounts("Unused attributes per node", unusedAttrByNode, "@");
            PrintNestedCounts("Unused child elements per node", unusedChildByNode, "<", ">");
        }

        private static void PrintCounts(string header, Dictionary<string, int> counts, string prefix = "", string suffix = "")
        {
            if (counts.Count == 0) return;
            Debug.WriteLine($"{header} (desc):");
            foreach (var (k, v) in counts.OrderByDescending(kv => kv.Value))
                Debug.WriteLine($"  {prefix}{k}{suffix}: {v}");
        }

        private static void PrintNestedCounts(string header, Dictionary<string, Dictionary<string, int>> dict, string prefix = "", string suffix = "")
        {
            if (dict.Count == 0) return;
            Debug.WriteLine($"{header} (desc):");
            foreach (var node in dict.OrderBy(k => k.Key))
            {
                Debug.WriteLine($"- <{node.Key}>:");
                foreach (var (name, cnt) in node.Value.OrderByDescending(kv => kv.Value))
                    Debug.WriteLine($"    {prefix}{name}{suffix}: {cnt}");
            }
        }
        public void ParseAssetXmlToModels(Stream xml)
        {
            var xdoc = XDocument.Load(xml);
            var modelTypes = typeof(ModelObject).Assembly.GetTypes()
                                .Where(t => t.Namespace == "RotMGAssetExtractor.Model")
                                .ToDictionary(t => t.Name, StringComparer.OrdinalIgnoreCase);

            foreach (var el in xdoc.Root.Elements())
            {
                var typeName = el.Element("Class")?.Value?.Trim();
                if (string.IsNullOrEmpty(typeName)) typeName = el.Name.LocalName;


                if (modelTypes.TryGetValue(typeName, out var targetType))
                {
                    var obj = typeof(UnityExtractor)
                        .GetMethod(nameof(MapXmlElementToObject), BindingFlags.NonPublic | BindingFlags.Static)!
                        .MakeGenericMethod(targetType)
                        .Invoke(null, new object[] { el });

                    if (!RotMGAssetExtractor.BuildModelsByType.TryGetValue(typeName, out var list))
                    {
                        list = new List<object>();
                        RotMGAssetExtractor.BuildModelsByType[typeName] = list;
                    }
                    list.Add(obj!);
                }
                else
                {
                    Increment(unrecognizedElementCounts, typeName);
                }
            }
        }

        private static T MapXmlElementToObject<T>(XElement el) where T : new()
        {
            var obj = new T();
            var props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                 .Where(p => p.CanWrite);

            var used = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var p in props)
            {
                /* figure out the XML tag / attr name -------------------------------- */
                var xmlAttr = p.GetCustomAttribute<XmlAttributeAttribute>();
                var xmlElem = p.GetCustomAttribute<XmlElementAttribute>();
                var xmlTxt = p.GetCustomAttribute<XmlTextAttribute>() != null;

                var name = xmlAttr?.AttributeName
                       ?? xmlElem?.ElementName
                       ?? p.Name;

                /* ----- ARRAY-OF-OBJECTS  (e.g. LevelIncrease[]) -------------------- */
                if (p.PropertyType.IsArray &&
                    p.PropertyType.GetElementType()!.IsClass &&
                    p.PropertyType != typeof(string[]))
                {
                    var elemType = p.PropertyType.GetElementType()!;
                    var items = el.Elements(name)
                                  .Select(c => typeof(UnityExtractor)
                                      .GetMethod(nameof(MapXmlElementToObject),
                                                 BindingFlags.NonPublic | BindingFlags.Static)!
                                      .MakeGenericMethod(elemType)
                                      .Invoke(null, new object[] { c }))
                                  .ToArray();

                    var arr = Array.CreateInstance(elemType, items.Length);
                    for (int i = 0; i < items.Length; i++) arr.SetValue(items[i], i);

                    p.SetValue(obj, arr);
                    if (items.Length > 0) used.Add(name);
                    continue;                   // skip the scalar switch
                }

                /* lookup (case-insensitive) ----------------------------------------- */
                var attr = el.Attributes()
                              .FirstOrDefault(a => a.Name.LocalName.Equals(name, StringComparison.OrdinalIgnoreCase));
                var child = el.Elements()
                              .FirstOrDefault(c => c.Name.LocalName.Equals(name, StringComparison.OrdinalIgnoreCase));

                string? raw = xmlTxt ? el.Value : attr?.Value ?? child?.Value;
                if (raw == null && child == null && !xmlTxt) continue;

                /* scalar / nested-object mapping ------------------------------------ */
                object val = p.PropertyType switch
                {
                    Type t when t == typeof(bool) =>
                        raw != null ? bool.TryParse(raw, out var b) && b : attr != null,

                    Type t when t == typeof(int[]) =>
                        string.IsNullOrWhiteSpace(raw)
                            ? Array.Empty<int>()
                            : raw.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries)
                                 .Select(int.Parse).ToArray(),

                    Type t when t == typeof(int) =>
                        raw != null && raw.StartsWith("0x", StringComparison.OrdinalIgnoreCase)
                            ? Convert.ToInt32(raw, 16)
                            : int.TryParse(raw, NumberStyles.Integer,
                                           CultureInfo.InvariantCulture, out var i) ? i : 0,

                    Type t when t == typeof(double) =>
                        double.TryParse(raw, NumberStyles.Float,
                                        CultureInfo.InvariantCulture, out var d) ? d : 0d,

                    Type t when t == typeof(float) =>
                        float.TryParse(raw, NumberStyles.Float,
                                       CultureInfo.InvariantCulture, out var f) ? f : 0f,

                    _ when p.PropertyType.IsClass && p.PropertyType != typeof(string) =>
                        typeof(UnityExtractor)
                            .GetMethod(nameof(MapXmlElementToObject),
                                       BindingFlags.NonPublic | BindingFlags.Static)!
                            .MakeGenericMethod(p.PropertyType)
                            .Invoke(null, new object[] { child ?? el })!,

                    _ => raw ?? string.Empty
                };

                p.SetValue(obj, val);
                used.Add(xmlTxt ? "#text" : name);
            }

            /* unused attributes / children ----------------------------------------- */
            foreach (var a in el.Attributes())
                if (!used.Contains(a.Name.LocalName))
                    IncrementNestedStatic(unusedAttrByNodeStatic,
                                          typeof(T).Name, a.Name.LocalName);

            foreach (var c in el.Elements())
                if (!used.Contains(c.Name.LocalName))
                    IncrementNestedStatic(unusedChildByNodeStatic,
                                          typeof(T).Name, c.Name.LocalName);

            return obj;
        }


        // these are only used inside static mapper; they proxy to outer instance via lazy re‑attach in RunExtractionPipeline.
        private static Dictionary<string, Dictionary<string, int>> unusedAttrByNodeStatic;
        private static Dictionary<string, Dictionary<string, int>> unusedChildByNodeStatic;

        static void IncrementNestedStatic(Dictionary<string, Dictionary<string, int>> dict, string parent, string key)
        {
            if (dict == null) return; // instance not set yet
            if (!dict.TryGetValue(parent, out var inner))
            {
                inner = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                dict[parent] = inner;
            }
            inner[key] = inner.TryGetValue(key, out var v) ? v + 1 : 1;
        }
        private static void Increment(Dictionary<string, int> dict, string key) =>
            dict[key] = dict.TryGetValue(key, out var v) ? v + 1 : 1;

        private static void IncrementNested(Dictionary<string, Dictionary<string, int>> dict, string parent, string key)
        {
            if (!dict.TryGetValue(parent, out var inner))
            {
                inner = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                dict[parent] = inner;
            }
            inner[key] = inner.TryGetValue(key, out var v) ? v + 1 : 1;
        }
        public void LoadXmlTextAssets(IEnumerable<Resources> resources)
        {
            var allTextAssets = resources.SelectMany(r => r.assetTextAssets);
            if (allTextAssets.Any())
            {
                Debug.WriteLine("[GameData] Processing " + allTextAssets.Count() + " TextAssets");
            }
            foreach (var ta in allTextAssets)
            {
                if (TextAsset.NonXmlFiles.Contains(ta.Name))
                    continue;
                try
                {
                    using var ms = new MemoryStream(ta.Script);
                    ParseAssetXmlToModels(ms);
                }
                catch (System.Xml.XmlException)
                {
                    Debug.WriteLine($"[Warning] Failed to parse XML for TextAsset: {ta.Name}. Skipping.");
                }
            }
        }

        public void ExportSpritesheet(IEnumerable<Resources> resources)
        {
            var spritesheetAsset = resources.Select(r => r.spritesheet).FirstOrDefault(s => s != null);
            if (spritesheetAsset != null)
            {
                Debug.WriteLine("[GameData] Storing and processing spritesheetf");
                RotMGAssetExtractor.BuildSpritesheetf = spritesheetAsset.Script;
            }
        }

        public void ExportAllTexturesAsPng(IEnumerable<Resources> resources)
        {
            var allTextures = resources.SelectMany(r => r.assetTexture2Ds).ToList();

            Debug.WriteLine($"[Debug] Found {allTextures.Count} total Texture2D assets.");
            // expetion here? System.ArgumentException: 'An item with the same key has already been added. Key: 3'
            var textureMap = allTextures
                .GroupBy(t => t.PathId)
                .ToDictionary(g => g.Key, g => g.First());

            var decoder = new BcDecoder();
            
            int processedCount = 0;
            foreach (var tex in allTextures)
            {

                //[Debug] Processing texture 'characters_masks' which is a known sprite sheet.
                //[GameData] Storing texture: characters_masks.png

                //Debug.WriteLine($"[Debug] Processing texture '{tex.Name}' with pathid '{tex.PathId}'which is a known sprite sheet.");
                if(tex.Name == "characters_masks" || tex.Name == "mapObjects")
                {
                    Debug.WriteLine(tex);
                }

                if (!AdvancedImaging.TryConvertTextureToBgra32(tex, decoder, out var bgra))
                {
                    //Debug.WriteLine($"[Warning] SKIPPING {tex.Name} (unsupported format: {tex.TextureFormat})");
                    continue;
                }

                using var img = Image.LoadPixelData<Bgra32>(bgra, tex.Width, tex.Height);
                img.Mutate(i => i.Flip(FlipMode.Vertical));

                using var ms = new MemoryStream();
                img.Save(ms, new PngEncoder());
                var png = ms.ToArray();

                string name = $"{tex.Name}.png";
                Debug.WriteLine("[GameData] Storing texture: " + name);
                RotMGAssetExtractor.BuildImages[name] = png;
                processedCount++;
            }
            Debug.WriteLine($"[Debug] Finished exporting textures. Processed {processedCount} images.");
        }
    }
}
