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
using R2API.Utils;
using RiskOfOptions;
using BepInEx.Configuration;


namespace ArtifactOfPrestige
{
    [BepInDependency("com.rune580.riskofoptions", BepInDependency.DependencyFlags.SoftDependency)]

    [BepInDependency("com.KingEnderBrine.ProperSave", BepInDependency.DependencyFlags.SoftDependency)]

    [BepInDependency(LanguageAPI.PluginGUID)]

    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]

    public class ArtifactOfPrestige : BaseUnityPlugin
    {
        public static ArtifactOfPrestige instance;

        internal static GameObject CentralNetworkObject;
        private static GameObject _centralNetworkObjectSpawned;

        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "Miyowi";
        public const string PluginName = "ArtifactOfPrestige";
        public const string PluginVersion = "1.3.2";

        public static ConfigEntry<bool> stackingIndicators { get; set; }
        public static ConfigEntry<bool> colouredIndicators { get; set; }
        public static ConfigEntry<Color> indicatorColor { get; set; }
        public static ConfigEntry<bool> stackOutsidePrestige { get; set; }
        public static ConfigEntry<bool> colouredOutsidePrestige { get; set; }

        public static BepInEx.PluginInfo pluginInfo;

        public List<ArtifactBase> Artifacts = new List<ArtifactBase>();
        public static int bonusRewardCount = 0;
        public static int shrineBonusStacks = 0;
        public static int offset = 0;
        public static bool NetworkshowExtraBossesIndicator = false;
        public static List<GameObject> localIndicators = [];

        public void Awake()
        {
            instance = this;
            pluginInfo = Info;
            Log.Init(Logger);
            Assets.PopulateAssets();

            var tmpGo = new GameObject("tmpGo");
            tmpGo.AddComponent<NetworkIdentity>();
            CentralNetworkObject = tmpGo.InstantiateClone("mwmwArtifactOfPrestige");
            GameObject.Destroy(tmpGo);
            CentralNetworkObject.AddComponent<Networking>();

            stackingIndicators = Config.Bind("Artifact of Prestige", "Stacking Indicators", true, "When more than one Shrine of the Mountain is activated, Shrine of the Mountain indicators stack above each other on the teleporter.");
            colouredIndicators = Config.Bind("Artifact of Prestige", "Coloured Indicator", true, "Shrine of the Mountain indicators are coloured differently to normal.");
            indicatorColor = Config.Bind("Artifact of Prestige", "Indicator Colour", new Color(0.8f, 0.13f, 0.6f, 1.0f), "If 'Coloured Indicator' is enabled, the colour that indicators are changed to.");
            stackOutsidePrestige = Config.Bind("General", "Stacking Indicators Outside Prestige", false, "Stacking Shrine of the Mountain indicators occurs in regular runs. Overrides 'Stacking Indicators'.");
            colouredOutsidePrestige = Config.Bind("General", "Coloured Indicators Outside Prestige", false, "Shrine of the Mountain indicators are coloured to the value set by 'Indicator Colour' in regular runs. Overrides 'Coloured Indicator'.");
           
            if (RiskOfOptionsCompatibility.enabled)
            {
                RiskOfOptionsCompatibility.OptionsInit();
            }

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
        
        public static void CheckNetworkObject()
        {
            if (NetworkServer.active)
            {
                if (!_centralNetworkObjectSpawned)
                {
                    _centralNetworkObjectSpawned = GameObject.Instantiate(CentralNetworkObject);
                    NetworkServer.Spawn(_centralNetworkObjectSpawned);
                }
            }
        }
        
        public static void ResetValues()
        {
            ArtifactOfPrestige.localIndicators = [];
            bonusRewardCount = 0;
            shrineBonusStacks = 0;
            offset = 0;
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
