using RotMGAssetExtractor.Flatc;
using RotMGAssetExtractor.UnityExtractor.resextractor;
using System.Collections;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Xml.Serialization;

namespace RotMGAssetExtractor
{
    public static class RotMGAssetExtractor
    {
        private static string CacheDirectory = "GameData";
        private const string MetadataFileName = "meta.xml";
        private const string ImagesDirectoryName = "images";
        private const string ModelsDirectoryName = "models";
        private const string SpritesheetFileName = "spritesheet.xml";

        public static string BuildHash { get; set; } = "";
        public static string BuildVersion { get; set; } = "";
        public static Dictionary<string, List<object>> BuildModelsByType { get; } = new(StringComparer.OrdinalIgnoreCase);
        public static Dictionary<string, byte[]> BuildImages { get; } = new(StringComparer.OrdinalIgnoreCase);
        public static byte[] BuildSpritesheetf = Array.Empty<byte>();
        internal static ExtractionSelection Selection { get; set; } = new();

        // Backward style "all" initializer (now just uses Selection)
        public static Task InitAsync(string basePath) =>
            InitAsync(basePath, ExtractionSelection.All());

        public static async Task InitAsync(string basePath, ExtractionSelection selection)
        {
            return InitInternalAsync(basePath, selection ?? throw new ArgumentNullException(nameof(selection)));
        }

        private static async Task InitInternalAsync(string basePath, ExtractionSelection selection)
        {
            Selection = selection;
            CacheDirectory = Path.Combine(basePath, "GameData");
            var downloader = new Downloading.Downloader();
            var (_, fetchedBuildHash) = await downloader.FetchBuildInfoAsync();
            if (string.IsNullOrEmpty(fetchedBuildHash))
                throw new Exception("Failed to fetch build information.");
            BuildHash = fetchedBuildHash;

            // Always fetch version (cheap): skip cache mismatch issues
            // (If you want to allow disabling, add optional flag later.)
            // Cache load first (needs BuildHash set)
            if (await LoadCacheAsync())
            {
                ApplySelectionFilters();
                await ImageBuffer.LoadAllAtlasesAsync();
                Debug.WriteLine("[GameData] Initialized from cache (filtered).");
            }
            else
            {
                Debug.WriteLine("[GameData] Downloading new gamedata.");
                await DownloadAll(new Downloading.Downloader());
                ApplySelectionFilters();
                await SaveCacheAsync();
                await ImageBuffer.LoadAllAtlasesAsync();
                Debug.WriteLine("[GameData] Initialized.");
            }
        }

        private static async Task<bool> LoadCacheAsync()
        {
            var metadataPath = Path.Combine(CacheDirectory, MetadataFileName);
            if (!File.Exists(metadataPath))
                return false;
            try
            {
                var serializer = new XmlSerializer(typeof(AssetCache));
                using var stream = new FileStream(metadataPath, FileMode.Open);
                var cache = (AssetCache?)serializer.Deserialize(stream);
                if (cache == null || cache.BuildHash != BuildHash)
                    return false;

                BuildVersion = cache.BuildVersion;

                BuildImages.Clear();
                if (Selection.WantsImages)
                {
                    var imagesPath = Path.Combine(CacheDirectory, ImagesDirectoryName);
                    if (Directory.Exists(imagesPath))
                        foreach (var file in Directory.GetFiles(imagesPath))
                            BuildImages[Path.GetFileName(file)] = await File.ReadAllBytesAsync(file);
                }

                BuildModelsByType.Clear();
                if (Selection.WantsModels)
                {
                    var modelsPath = Path.Combine(CacheDirectory, ModelsDirectoryName);
                    if (Directory.Exists(modelsPath))
                    {
                        var modelTypes = Assembly.GetExecutingAssembly().GetTypes()
                            .Where(t => t.Namespace == "RotMGAssetExtractor.Model" && !t.IsAbstract).ToArray();
                        foreach (var file in Directory.GetFiles(modelsPath, "*.xml"))
                        {
                            var typeName = Path.GetFileNameWithoutExtension(file);
                            var modelType = modelTypes.FirstOrDefault(t => t.Name == typeName);
                            if (modelType == null) continue;
                            var listType = typeof(List<>).MakeGenericType(modelType);
                            var ser = new XmlSerializer(listType);
                            using var modelStream = new FileStream(file, FileMode.Open);
                            if (ser.Deserialize(modelStream) is IList list)
                                BuildModelsByType[typeName] = list.Cast<object>().ToList();
                        }
                    }
                }

                if (Selection.WantsSpritesheet)
                {
                    if (!TryLoadSpritesheet(Path.Combine(CacheDirectory, SpritesheetFileName)))
                        return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[GameData] Error loading cache: {ex.Message}");
                return false;
            }
        }

        private static async Task SaveCacheAsync()
        {
            Directory.CreateDirectory(CacheDirectory);

            var cache = new AssetCache { BuildHash = BuildHash, BuildVersion = BuildVersion };
            var ser = new XmlSerializer(typeof(AssetCache));
            using (var fs = File.Create(Path.Combine(CacheDirectory, MetadataFileName)))
                ser.Serialize(fs, cache);

            if (Selection.WantsImages && BuildImages.Count > 0)
            {
                var imagesDir = Path.Combine(CacheDirectory, ImagesDirectoryName);
                Directory.CreateDirectory(imagesDir);
                foreach (var kv in BuildImages)
                {
                    var path = Path.Combine(imagesDir, kv.Key);
                    Directory.CreateDirectory(Path.GetDirectoryName(path)!);
                    await File.WriteAllBytesAsync(path, kv.Value);
                }
            }

            if (Selection.WantsModels && BuildModelsByType.Count > 0)
            {
                var modelsDir = Path.Combine(CacheDirectory, ModelsDirectoryName);
                Directory.CreateDirectory(modelsDir);
                SaveModels(modelsDir);
            }

            if (Selection.WantsSpritesheet)
                WriteSpritesheet(Path.Combine(CacheDirectory, SpritesheetFileName));

            Debug.WriteLine("[GameData] Cache saved.");
        }

        private static void SaveModels(string modelsPath)
        {
            var modelTypes = Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => t.Namespace == "RotMGAssetExtractor.Model" && !t.IsAbstract).ToArray();
            foreach (var entry in BuildModelsByType)
            {
                var modelType = modelTypes.FirstOrDefault(t => t.Name == entry.Key);
                if (modelType == null) continue;
                var listType = typeof(List<>).MakeGenericType(modelType);
                var typedList = (IList)Activator.CreateInstance(listType)!;
                foreach (var item in entry.Value) typedList.Add(item);
                var ser = new XmlSerializer(listType);
                using var fs = File.Create(Path.Combine(modelsPath, $"{entry.Key}.xml"));
                ser.Serialize(fs, typedList);
            }
        }

        internal static async Task DownloadAll(Downloading.Downloader downloader)
        {
            var unityExtractor = new Parser.UnityExtractor();

            var (baseCdnUrl, fetchedBuildHash) = await downloader.FetchBuildInfoAsync();
            if (string.IsNullOrEmpty(baseCdnUrl) || string.IsNullOrEmpty(fetchedBuildHash))
                throw new Exception("Failed to fetch build information.");
            BuildHash = fetchedBuildHash;

            var fileList = await downloader.FetchFileListAsync(baseCdnUrl, BuildHash)
                          ?? throw new Exception("No files found.");
            if (fileList.Count == 0) throw new Exception("No files found.");

            // Always obtain version
            var meta = fileList.FirstOrDefault(f => f.file == "global-metadata.dat");
            if (meta != null)
            {
                var data = await downloader.DownloadAndDecompressFileAsync(meta.url);
                BuildVersion = unityExtractor.GetUnityVersionFromMetadata(data);
                fileList.Remove(meta);
            }

            if (!Selection.RequiresResourcesAssets)
            {
                Debug.WriteLine("[GameData] Only version requested.");
                return;
            }

            bool imageFallbackNeeded = false;

            // Determine targeting (images only & fully mapped)
            bool onlyImages = Selection.WantsImages && !Selection.WantsModels && !Selection.WantsSpritesheet;
            HashSet<string>? targetedAssets = null;
            bool canTarget = false;

            if (onlyImages)
            {
                targetedAssets = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                foreach (var ki in Selection.KnownImages)
                    targetedAssets.Add(KnownImageMap.Map[ki].AssetFile);

                foreach (var name in Selection.ImageNames)
                {
                    if (KnownImageMap.TryGetByTextureName(Path.GetFileNameWithoutExtension(name), out var resolved))
                        targetedAssets.Add(resolved.info.AssetFile);
                    else
                    {
                        if (Selection.FallbackFullScanForUnknownImages)
                        {
                            imageFallbackNeeded = true;
                            targetedAssets = null;
                            break;
                        }
                    }
                }

                if (!imageFallbackNeeded && targetedAssets != null && targetedAssets.Count > 0)
                    canTarget = true;
            }

            var processed = new List<Resources>();
            var assetFiles = fileList.Where(f => f.file.EndsWith(".assets")).ToDictionary(f => f.file, f => f);
            var resSFiles = fileList.Where(f => f.file.EndsWith(".resS")).ToDictionary(f => f.file, f => f);

            IEnumerable<string> assetsToDownload;
            if (canTarget)
            {
                assetsToDownload = targetedAssets!;
            }
            else
            {
                // Base set always includes resources.assets for models or general
                var list = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                list.Add("resources.assets");

                if (Selection.RequiresFullScan(imageFallbackNeeded))
                {
                    foreach (var f in assetFiles.Keys)
                        list.Add(f);
                }
                assetsToDownload = list;
            }

            foreach (var assetName in assetsToDownload)
            {
                if (!assetFiles.TryGetValue(assetName, out var entry))
                {
                    Debug.WriteLine($"[GameData] WARNING: Asset '{assetName}' missing.");
                    continue;
                }
                var data = await downloader.DownloadAndDecompressFileAsync(entry.url);
                byte[]? resSData = null;
                var resSName = Path.ChangeExtension(assetName, ".resS");
                if (resSFiles.TryGetValue(resSName, out var resS))
                    resSData = await downloader.DownloadAndDecompressFileAsync(resS.url);
                processed.Add(unityExtractor.ProccessResource(assetName, data, resSData));
            }

            if (Selection.WantsImages)
                unityExtractor.ExportAllTexturesAsPng(processed);
            if (Selection.WantsModels)
                unityExtractor.LoadXmlTextAssets(processed);
            if (Selection.WantsSpritesheet)
            {
                unityExtractor.ExportSpritesheet(processed);
                SpriteFlatBuffer.Reload();
            }

            var summary = new StringBuilder("[GameData] Parsed new assets.");
            if (Selection.WantsImages) summary.Append($" Images: {BuildImages.Count}.");
            if (Selection.WantsModels)
            {
                int totalModels = BuildModelsByType.Values.Sum(l => l.Count);
                summary.Append($" Model types: {BuildModelsByType.Count}, Total Models: {totalModels}.");
            }
            if (Selection.WantsSpritesheet)
            {
                int spriteCount = SpriteFlatBuffer.GetSprites().Sum(g => g.Value.Count);
                summary.Append($" Spritesheet entries: {spriteCount}.");
            }
            if (canTarget) summary.Append(" (Targeted image mode)");
            Debug.WriteLine(summary.ToString());

            AssetOriginRegistry.LogAllDiagnostics(Selection, unityExtractor.GetNodeDiagnostics());
        }

        private static void ApplySelectionFilters()
        {
            if (Selection.WantsImages && (Selection.ImageNames.Count > 0 || Selection.KnownImages.Count > 0) && !Selection.AllImagesRequested)
            {
                var keep = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (var n in Selection.ImageNames)
                {
                    keep.Add(n);
                    keep.Add(Path.GetFileNameWithoutExtension(n));
                }
                foreach (var ki in Selection.KnownImages)
                {
                    var mapped = KnownImageMap.Map[ki].TextureName;
                    keep.Add(mapped);
                    keep.Add(mapped + ".png");
                }
                var remove = new List<string>();
                foreach (var kv in BuildImages)
                {
                    var bare = Path.GetFileNameWithoutExtension(kv.Key);
                    if (!keep.Contains(kv.Key) && !keep.Contains(bare))
                        remove.Add(kv.Key);
                }
                foreach (var r in remove) BuildImages.Remove(r);
            }

            if (Selection.WantsModels && (Selection.ModelTypes.Count > 0 || Selection.ModelIdsByType.Count > 0))
            {
                var removeTypes = new List<string>();
                foreach (var entry in BuildModelsByType)
                {
                    var typeName = entry.Key;
                    bool keepAll = Selection.ModelTypes.Contains(typeName);
                    if (keepAll) continue;

                    var filtered = entry.Value.Where(obj =>
                    {
                        if (Selection.ModelTypes.Contains(typeName)) return true;
                        if (Selection.ModelIdsByType.TryGetValue(typeName, out var ids))
                        {
                            var t = obj.GetType();
                            var prop = t.GetProperty("Id") ?? t.GetProperty("ID") ?? t.GetProperty("Name");
                            var val = prop?.GetValue(obj)?.ToString();
                            return val != null && ids.Contains(val);
                        }
                        return false;
                    }).ToList();

                    if (filtered.Count == 0) removeTypes.Add(typeName);
                    else BuildModelsByType[typeName] = filtered;
                }
                foreach (var t in removeTypes) BuildModelsByType.Remove(t);
            }
        }

        private static bool TryLoadSpritesheet(string path)
        {
            if (!File.Exists(path)) return true;
            var fi = new FileInfo(path);
            if (fi.Length == 0) return false;
            var first = File.ReadLines(path).FirstOrDefault(l => !string.IsNullOrWhiteSpace(l)) ?? "";
            if (!first.Contains("<")) return false;
            try
            {
                var xs = new XmlSerializer(typeof(DecompiledSpriteSheet));
                using var fs = new FileStream(path, FileMode.Open);
                var sheet = (DecompiledSpriteSheet)xs.Deserialize(fs);
                SpriteFlatBuffer.LoadFromDecompiled(sheet);
                return true;
            }
            catch { return false; }
        }
    }
}
