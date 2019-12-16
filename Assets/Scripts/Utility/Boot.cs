using System.IO;
using Services;
using UnityEngine;

namespace Utility
{
    public class Boot
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void OnBeforeSceneLoad()
        {
            Directory.CreateDirectory(FolderPaths.OSMData);
            Directory.CreateDirectory(FolderPaths.SRTMData);
            Directory.CreateDirectory(FolderPaths.ConfigDir);

            //Make sure .cfg is up to date and exists
            var configService = new ConfigurationService(FolderPaths.ConfigFile);
        }
    }
}