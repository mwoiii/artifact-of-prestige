using BepInEx;
using R2API;
using RoR2;
using UnityEngine.Networking;
using System.Collections.Generic;
using System;
using System.Text;
using UnityEngine;
using System.Linq;
using UnityEngine.SceneManagement;
using System.Reflection;

namespace ArtifactOfPrestige
{

    // don't touch these
    [BepInDependency(LanguageAPI.PluginGUID)]

    // This attribute is required, and lists metadata for your plugin.
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]


    public class ArtifactOfPrestige : BaseUnityPlugin
    {
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "Miyowi";
        public const string PluginName = "ArtifactOfPrestige";
        public const string PluginVersion = "1.0.0";

        public static PluginInfo pluginInfo;

        public List<ArtifactBase> Artifacts = new List<ArtifactBase>();
        public static int bonusRewardCount = 0;
        public static int shrineBonusStacks = 0;
        public static bool NetworkshowExtraBossesIndicator = false;

        public void Awake()
        {
            pluginInfo = Info;
            Log.Init(Logger);
            Assets.PopulateAssets();

            var ArtifactTypes = Assembly.GetExecutingAssembly().GetTypes().Where(type => !type.IsAbstract && type.IsSubclassOf(typeof(ArtifactBase)));
            foreach (var artifactType in ArtifactTypes)
            {
                ArtifactBase artifact = (ArtifactBase)Activator.CreateInstance(artifactType);
                if (ValidateArtifact(artifact, Artifacts))
                {
                    artifact.Init(Config);
                }
            }
        }

        public static void ResetValues()
        {
            bonusRewardCount = 0;
            shrineBonusStacks = 0;
            NetworkshowExtraBossesIndicator = false;
        }

        public bool ValidateArtifact(ArtifactBase artifact, List<ArtifactBase> artifactList)
        {
            var enabled = Config.Bind<bool>("Artifact: " + artifact.ArtifactName, "Enable Artifact?", true, "Should this artifact appear for selection?").Value;
            if (enabled)
            {
                artifactList.Add(artifact);
            }
            return enabled;
        }
    }
}
