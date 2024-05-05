using UnityEngine;
using RoR2.ContentManagement;
using System.Collections;
using Path = System.IO.Path;

namespace ArtifactOfPrestige
{
    public static class Assets
    {
        public static AssetBundle mainAssetBundle = null;
        internal static string assetBundleName = "icons";

        internal static string assemblyDir
        {
            get
            {
                return Path.GetDirectoryName(ArtifactOfPrestige.pluginInfo.Location);
            }
        }

        public static void PopulateAssets()
        {
            mainAssetBundle = AssetBundle.LoadFromFile(Path.Combine(assemblyDir, assetBundleName));
        }
    }
}