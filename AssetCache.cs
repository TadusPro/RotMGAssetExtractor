using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace RotMGAssetExtractor
{
    public class AssetCache
    {
        public string BuildHash { get; set; } = "";
        public string BuildVersion { get; set; } = "";
    }
}