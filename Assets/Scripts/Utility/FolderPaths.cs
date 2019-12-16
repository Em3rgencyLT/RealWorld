using UnityEngine;

namespace Utility
{
    public static class FolderPaths
    {
        public static string SRTMData => Application.streamingAssetsPath + Slash() + "SRTMCache";
        public static string OSMData => Application.streamingAssetsPath + Slash() + "OSMCache";
        public static string ConfigDir => Application.persistentDataPath + Slash() + "Config";
        public static string ConfigFile => ConfigDir + Slash() + "config.cfg";
        
        public static string Slash()
        {
            switch (Application.platform)
            {
                //Fuck you Mr. Gates
                case RuntimePlatform.WindowsEditor:
                case RuntimePlatform.WindowsPlayer:
                    return @"\";
                default:
                    return "/";
            }
        }
    }
}