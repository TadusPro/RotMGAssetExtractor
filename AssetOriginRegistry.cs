using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace RotMGAssetExtractor
{
    internal sealed class NodeDiagnostics
    {
        public Dictionary<string, int> UnknownNodes { get; }
        public Dictionary<string, Dictionary<string, int>> UnusedAttributesByNode { get; }
        public Dictionary<string, Dictionary<string, int>> UnusedChildrenByNode { get; }

        public NodeDiagnostics(
            Dictionary<string, int> unknown,
            Dictionary<string, Dictionary<string, int>> unusedAttr,
            Dictionary<string, Dictionary<string, int>> unusedChild)
        {
            UnknownNodes = unknown;
            UnusedAttributesByNode = unusedAttr;
            UnusedChildrenByNode = unusedChild;
        }
    }

    internal static partial class AssetOriginRegistry
    {
        /* ---------------- TEXTURES ---------------- */
        private static readonly Dictionary<long, string> _texturePathToAsset = new();
        private static readonly Dictionary<string, string> _textureNameToAsset = new(StringComparer.OrdinalIgnoreCase);

        internal static void RecordTexture(long pathId, string textureName, string assetFile)
        {
            if (!_texturePathToAsset.ContainsKey(pathId))
                _texturePathToAsset[pathId] = assetFile;
            if (!_textureNameToAsset.ContainsKey(textureName))
                _textureNameToAsset[textureName] = assetFile;
        }

        internal static string? GetTextureAssetFor(string textureName) =>
            _textureNameToAsset.TryGetValue(textureName, out var v) ? v : null;

        internal static void LogSuggestionsForUnknownImages(ExtractionSelection sel)
        {
            if (!sel.WantsImages) return;
            foreach (var raw in sel.ImageNames)
            {
                var bare = System.IO.Path.GetFileNameWithoutExtension(raw);
                if (KnownImageMap.TryGetByTextureName(bare, out _))
                    continue;

                var asset = GetTextureAssetFor(bare);
                if (asset != null)
                {
                    Debug.WriteLine(
                        $"[ImageMapSuggestion] Unmapped requested image '{bare}' found in '{asset}'. " +
                        $"Add mapping: {{ KnownImage.YourEnumValue, (\"{bare}\", \"{asset}\") }}");
                }
                else
                {
                    Debug.WriteLine($"[ImageMapSuggestion] Requested image '{bare}' not found in scanned assets.");
                }
            }
        }

        /* ---------------- MODELS ---------------- */
        // ModelType -> (HashSet ids, HashSet assetFiles)
        private static readonly Dictionary<string, HashSet<string>> _modelIdsByType = new(StringComparer.OrdinalIgnoreCase);
        private static readonly Dictionary<string, HashSet<string>> _modelTypeAssets = new(StringComparer.OrdinalIgnoreCase);

        // TextAsset name -> asset file (intermediate mapping in case needed)
        private static readonly Dictionary<string, string> _textAssetOrigins = new(StringComparer.OrdinalIgnoreCase);

        internal static void RecordTextAsset(string textAssetName, string assetFile)
        {
            if (!_textAssetOrigins.ContainsKey(textAssetName))
                _textAssetOrigins[textAssetName] = assetFile;
        }

        internal static void RecordModelInstance(string modelType, object instance, string sourceAssetFile)
        {
            if (!_modelTypeAssets.TryGetValue(modelType, out var files))
            {
                files = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                _modelTypeAssets[modelType] = files;
            }
            files.Add(sourceAssetFile);

            if (!_modelIdsByType.TryGetValue(modelType, out var idSet))
            {
                idSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                _modelIdsByType[modelType] = idSet;
            }

            var id = ExtractModelId(instance);
            if (!string.IsNullOrEmpty(id))
                idSet.Add(id);
        }

        private static string? ExtractModelId(object instance)
        {
            var t = instance.GetType();
            PropertyInfo? prop = t.GetProperty("Id") ??
                                 t.GetProperty("ID") ??
                                 t.GetProperty("Name");
            return prop?.GetValue(instance)?.ToString();
        }

        internal static void LogSuggestionsForModels(ExtractionSelection sel)
        {
            if (!sel.WantsModels) return;

            // Missing requested types
            foreach (var requestedType in sel.ModelTypes)
            {
                if (!_modelTypeAssets.ContainsKey(requestedType))
                {
                    Debug.WriteLine($"[ModelSuggestion] Requested model type '{requestedType}' not found in any parsed assets.");
                }
            }

            // Requested specific IDs
            foreach (var kv in sel.ModelIdsByType)
            {
                var typeName = kv.Key;
                var requestedIds = kv.Value;
                if (!_modelIdsByType.TryGetValue(typeName, out var availableIds))
                {
                    Debug.WriteLine($"[ModelSuggestion] Requested model type '{typeName}' not found while expecting IDs: {string.Join(", ", requestedIds)}");
                    continue;
                }

                var missing = requestedIds.Where(id => !availableIds.Contains(id)).ToList();
                if (missing.Count > 0)
                {
                    var sample = string.Join(", ", availableIds.Take(10)) +
                                 (availableIds.Count > 10 ? " ..." : "");
                    Debug.WriteLine($"[ModelSuggestion] Missing {missing.Count} {typeName} IDs: {string.Join(", ", missing)}. " +
                                    $"Available sample ({availableIds.Count} total): {sample}");
                }
            }
        }

        internal static void LogAllSuggestions(ExtractionSelection sel)
        {
            LogSuggestionsForUnknownImages(sel);
            LogSuggestionsForModels(sel);
        }

        internal static void LogAllDiagnostics(ExtractionSelection sel, NodeDiagnostics nodeDiag)
        {
            // 1. Node / XML mapping diagnostics
            LogNodeDiagnostics(nodeDiag);

            // 2. Image suggestions
            LogSuggestionsForUnknownImages(sel);

            // 3. Model suggestions
            LogSuggestionsForModels(sel);
        }

        internal static void LogNodeDiagnostics(NodeDiagnostics d)
        {
            if (d == null) return;
            PrintCounts("Unknown XML nodes", d.UnknownNodes);
            PrintNestedCounts("Unused attributes per node", d.UnusedAttributesByNode, "@");
            PrintNestedCounts("Unused child elements per node", d.UnusedChildrenByNode, "<", ">");
        }

        private static void PrintCounts(string header, Dictionary<string, int> counts, string prefix = "", string suffix = "")
        {
            if (counts == null || counts.Count == 0) return;
            Debug.WriteLine($"{header} (desc):");
            foreach (var (k, v) in counts.OrderByDescending(kv => kv.Value))
                Debug.WriteLine($"  {prefix}{k}{suffix}: {v}");
        }

        private static void PrintNestedCounts(string header,
            Dictionary<string, Dictionary<string, int>> dict,
            string prefix = "", string suffix = "")
        {
            if (dict == null || dict.Count == 0) return;
            Debug.WriteLine($"{header} (desc):");
            foreach (var node in dict.OrderBy(k => k.Key))
            {
                Debug.WriteLine($"- <{node.Key}>:");
                foreach (var (name, cnt) in node.Value.OrderByDescending(kv => kv.Value))
                    Debug.WriteLine($"    {prefix}{name}{suffix}: {cnt}");
            }
        }
    }
}