namespace RotMGAssetExtractor.UnityExtractor.resextractor
{
    public enum Platform
    {
        UnknownPlatform = 3716,
        DashboardWidget = 1,
        StandaloneOSX = 2,
        StandaloneOSXPPC = 3,
        StandaloneOSXIntel = 4,
        StandaloneWindows = 5,
        WebPlayer = 6,
        WebPlayerStreamed = 7,
        Wii = 8,
        iOS = 9,
        PS3 = 10,
        XBOX360 = 11,
        Android = 13,
        StandaloneGLESEmu = 14,
        NaCl = 16,
        StandaloneLinux = 17,
        FlashPlayer = 18,
        StandaloneWindows64 = 19,
        WebGL = 20,
        WSAPlayer = 21,
        StandaloneLinux64 = 24,
        StandaloneLinuxUniversal = 25,
        WP8Player = 26,
        StandaloneOSXIntel64 = 27,
        BlackBerry = 28,
        Tizen = 29,
        PSP2 = 30,
        PS4 = 31,
        PSM = 32,
        XboxOne = 33,
        SamsungTV = 34,
        N3DS = 35,
        WiiU = 36,
        tvOS = 37,
        Switch = 38,
        NoTarget = 39 // Note: Changed index to avoid duplicate with StandaloneOSX
    }

    public static class PlatformMethods
    {
        private static readonly Dictionary<int, Platform> EnumByIndex;

        static PlatformMethods()
        {
            EnumByIndex = new Dictionary<int, Platform>();
            foreach (Platform platform in Enum.GetValues(typeof(Platform)))
            {
                EnumByIndex.Add((int)platform, platform);
            }
        }

        public static Platform ByOrdinal(int ord)
        {
            if (EnumByIndex.TryGetValue(ord, out Platform result))
            {
                return result;
            }
            return Platform.UnknownPlatform; // Return a default if not found, for safety.
        }
    }
}
