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
        public static Dictionary<string, byte[]> BuildImages { get; } = new Dictionary<string, byte[]>();
        public static byte[] BuildSpritesheetf = new byte[0];
        internal static ExtractionType[] ExtractionTypes { get; set; } = Array.Empty<ExtractionType>();

        public static Task InitAsync(string basePath)
        {
            return InitAsync(basePath, ExtractionType.All);
        }
        public static async Task InitAsync(string basePath, params ExtractionType[] extractionTypes)
        {
            ExtractionTypes = extractionTypes;
            CacheDirectory = Path.Combine(basePath, "GameData");
            var downloader = new Downloading.Downloader();
            var (_, fetchedBuildHash) = await downloader.FetchBuildInfoAsync();

            if (string.IsNullOrEmpty(fetchedBuildHash))
                throw new Exception("Failed to fetch build information.");

            BuildHash = fetchedBuildHash;
            if (await LoadCacheAsync())
            {
                await ImageBuffer.LoadAllAtlasesAsync();
                Debug.WriteLine("[GameData] Initialized from cache.");
                return;
            }

            Debug.WriteLine("[GameData] Downloading new gamedata.");
            await DownloadAll(downloader);
            await SaveCacheAsync();
            await ImageBuffer.LoadAllAtlasesAsync();
            Debug.WriteLine("[GameData] Initialized.");
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

                // Images
                BuildImages.Clear();
                var imagesPath = Path.Combine(CacheDirectory, ImagesDirectoryName);
                if (Directory.Exists(imagesPath))
                {
                    foreach (var file in Directory.GetFiles(imagesPath))
                    {
                        var fileName = Path.GetFileName(file);
                        BuildImages[fileName] = await File.ReadAllBytesAsync(file);
                    }
                }

                // Spritesheet
                if (!TryLoadSpritesheet(Path.Combine(CacheDirectory, SpritesheetFileName)))
                    return false;

                // Models
                BuildModelsByType.Clear();
                var modelsPath = Path.Combine(CacheDirectory, ModelsDirectoryName);
                if (Directory.Exists(modelsPath))
                {
                    var modelTypes = Assembly.GetExecutingAssembly().GetTypes()
                        .Where(t => t.Namespace == "RotMGAssetExtractor.Model" && !t.IsAbstract).ToArray();

                    foreach (var file in Directory.GetFiles(modelsPath, "*.xml"))
                    {
                        var typeName = Path.GetFileNameWithoutExtension(file);
                        var modelType = modelTypes.FirstOrDefault(t => t.Name == typeName);
                        if (modelType != null)
                        {
                            var listType = typeof(List<>).MakeGenericType(modelType);
                            var modelSerializer = new XmlSerializer(listType);
                            using var modelStream = new FileStream(file, FileMode.Open);
                            var models = (IList)modelSerializer.Deserialize(modelStream);
                            if (models != null)
                                BuildModelsByType[typeName] = models.Cast<object>().ToList();
                        }
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading cache: {ex.Message}");
                return false;
            }
        }


        private static async Task SaveCacheAsync()
        {
            Directory.CreateDirectory(CacheDirectory);
            var imagesPath = Path.Combine(CacheDirectory, ImagesDirectoryName);
            Directory.CreateDirectory(imagesPath);
            var modelsPath = Path.Combine(CacheDirectory, ModelsDirectoryName);
            Directory.CreateDirectory(modelsPath);

            // Metadata
            var cache = new AssetCache { BuildHash = BuildHash, BuildVersion = BuildVersion };
            var metaSerializer = new XmlSerializer(typeof(AssetCache));
            using (var fs = File.Create(Path.Combine(CacheDirectory, MetadataFileName)))
                metaSerializer.Serialize(fs, cache);

            // Images
            foreach (var entry in BuildImages)
            {
                var imagePath = Path.Combine(imagesPath, entry.Key);
                Directory.CreateDirectory(Path.GetDirectoryName(imagePath)!);
                await File.WriteAllBytesAsync(imagePath, entry.Value);
            }

            // Spritesheet (sync serialize)
            WriteSpritesheet(Path.Combine(CacheDirectory, SpritesheetFileName));

            // Models (sync serialize)
            SaveModels(modelsPath);

            Debug.WriteLine("[GameData] Cache saved.");
        }

        private static string Clean(string s)
        {
            if (string.IsNullOrEmpty(s)) return string.Empty;
            var sb = new StringBuilder(s.Length);
            foreach (var ch in s)
                if (ch == '\t' || ch == '\n' || ch == '\r' || ch >= 0x20)
                    sb.Append(ch); // keep valid XML chars
            return sb.ToString();
        }

        private static void WriteSpritesheet(string path)
        {
            var decompiled = new DecompiledSpriteSheet();
            var spriteGroups = SpriteFlatBuffer.GetSprites();

            foreach (var group in spriteGroups)
            {
                var cleanedGroupName = Clean(group.Key);
                var sg = new SpriteGroup { Name = cleanedGroupName };

                foreach (var s in group.Value)
                {
                    var spriteInfo = new SpriteInfo
                    {
                        Index = s.Key,
                        AtlasId = s.Value.AtlasId,
                        X = s.Value.Coords[0],
                        Y = s.Value.Coords[1],
                        W = s.Value.Coords[2],
                        H = s.Value.Coords[3]
                    };
                    sg.Sprites.Add(spriteInfo);
                }
                decompiled.SpriteGroups.Add(sg);
            }
            int count = 0;
            foreach (var group in decompiled.SpriteGroups)
                foreach (var sprite in group.Sprites)
                    count++;

            var ser = new XmlSerializer(typeof(DecompiledSpriteSheet));

            try
            {
                using var fs = File.Create(path);
                ser.Serialize(fs, decompiled);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[GameData] ERROR: An exception occurred during serialization: {ex}");
                throw; // Re-throw the exception to not hide the error
            }
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

            // Process build info and file list as before.
            var (baseCdnUrl, fetchedBuildHash) = await downloader.FetchBuildInfoAsync();
            if (string.IsNullOrEmpty(baseCdnUrl) || string.IsNullOrEmpty(fetchedBuildHash))
            {
                throw new Exception("Failed to fetch build information.");
            }
            // Save the fetched build hash in the public property.
            BuildHash = fetchedBuildHash;

            var fileList = await downloader.FetchFileListAsync(baseCdnUrl, BuildHash);
            if (fileList is null || fileList.Count == 0)
            {
                throw new Exception("No files found.");
            }

            // Global metadata extraction // if extractiontype all or gameversion is set
            if (ExtractionTypes.Contains(ExtractionType.GameVersion) && ExtractionTypes.Contains(ExtractionType.All))
            {
                var meta = fileList.FirstOrDefault(f => f.file == "global-metadata.dat");
                if (meta != null)
                {
                    var data = await downloader.DownloadAndDecompressFileAsync(meta.url);
                    BuildVersion = unityExtractor.GetUnityVersionFromMetadata(data);
                    fileList.Remove(meta);
                }
            }
            

            // Download and process each relevant resource file
            var processedResources = new List<Resources>();
            var assetFiles = fileList.Where(f => f.file.EndsWith(".assets")).ToList();
            var resSFiles = fileList.Where(f => f.file.EndsWith(".resS"))
                                    .ToDictionary(f => f.file, f => f);


            if (ExtractionTypes.Contains(ExtractionType.All) || 
                ExtractionTypes.Contains(ExtractionType.Models) || 
                ExtractionTypes.Contains(ExtractionType.Spritesheet) || 
                ExtractionTypes.Contains(ExtractionType.ImagesLight) || 
                ExtractionTypes.Contains(ExtractionType.ImagesLight))
            {
                // inside here, only download and process the file "resources.assets" nothing more
                var assetFiles2 = assetFiles.Where(f => f.file == "resources.assets").ToList();
                if (assetFiles2.Count == 0)
                {
                    throw new Exception("No resources.assets file found in the file list.");
                }
                var assetData = await downloader.DownloadAndDecompressFileAsync(assetFiles2.FirstOrDefault().url);
                processedResources.Add(unityExtractor.ProccessResource("resources.assets", assetData));

            }

            if (ExtractionTypes.Contains(ExtractionType.All))
            {
                // inside here skip the file "resources.assets" and process all other asset files remove it from the list
                assetFiles = assetFiles.Where(f => f.file != "resources.assets").ToList();

                foreach (var assetFile in assetFiles)
                {
                    var assetData = await downloader.DownloadAndDecompressFileAsync(assetFile.url);
                    byte[]? resSData = null;

                    // The corresponding .resS file has the same name but with a .resS extension
                    var resSFileName = Path.ChangeExtension(assetFile.file, ".resS");
                    if (resSFiles.TryGetValue(resSFileName, out var resSFile))
                    {
                        resSData = await downloader.DownloadAndDecompressFileAsync(resSFile.url);
                    }

                    processedResources.Add(unityExtractor.ProccessResource(assetFile.file, assetData, resSData));
                }
            }

            

            // Now that all resources are parsed, perform the combined processing steps
            if (ExtractionTypes.Contains(ExtractionType.ImagesLight) || ExtractionTypes.Contains(ExtractionType.ImagesAll) || ExtractionTypes.Contains(ExtractionType.All))
            {
                unityExtractor.ExportAllTexturesAsPng(processedResources);

            }
            if (ExtractionTypes.Contains(ExtractionType.Models) || ExtractionTypes.Contains(ExtractionType.All))
            {
                unityExtractor.LoadXmlTextAssets(processedResources);
            }
            if (ExtractionTypes.Contains(ExtractionType.Spritesheet) || ExtractionTypes.Contains(ExtractionType.All))
            {
                unityExtractor.ExportSpritesheet(processedResources);
                SpriteFlatBuffer.Reload();
            }

            //unityExtractor.UnusedNodesDebug();

            GC.Collect();

            var summary = new StringBuilder("[GameData] Successfully parsed new assets.");
            if (ExtractionTypes.Contains(ExtractionType.All) || ExtractionTypes.Contains(ExtractionType.ImagesLight) || ExtractionTypes.Contains(ExtractionType.ImagesAll))
            {
                summary.Append($" Images: {BuildImages.Count()}.");
            }

            if (ExtractionTypes.Contains(ExtractionType.All) || ExtractionTypes.Contains(ExtractionType.Models))
            {
                int totalModels = BuildModelsByType.Values.Sum(list => list.Count);
                summary.Append($" Model types: {BuildModelsByType.Count()}, Total Models: {totalModels}.");
            }
            if (ExtractionTypes.Contains(ExtractionType.Spritesheet) || ExtractionTypes.Contains(ExtractionType.All))
            {
                int spriteCount = SpriteFlatBuffer.GetSprites().Sum(g => g.Value.Count);
                summary.Append($" spritesheet keys: {spriteCount}.");
                
            }
            Debug.WriteLine(summary.ToString());
        }
        private static bool TryLoadSpritesheet(string path)
        {
            if (!File.Exists(path)) return true; // nothing to load

            var fi = new FileInfo(path);
            if (fi.Length == 0) return false; // empty -> bad cache

            // quick sanity check
            var first = File.ReadLines(path).FirstOrDefault(l => !string.IsNullOrWhiteSpace(l)) ?? "";
            if (!first.Contains("<")) return false;

            // this can still throw if XML is truncated; keep a local try
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
