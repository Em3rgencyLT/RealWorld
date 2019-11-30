using UnityEngine;

namespace Utility
{
    public static class FolderPaths
    {
        public static string SRTMData => Application.dataPath + PickSlashDirection() + "SRTMData";
        
        private static string PickSlashDirection()
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