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


namespace ArtifactOfPrestige
{

    // don't touch these
    [BepInDependency(LanguageAPI.PluginGUID)]

    // This attribute is required, and lists metadata for your plugin.
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]

    public class ArtifactOfPrestige : BaseUnityPlugin
    {
        public static ArtifactOfPrestige instance;
        //Static references so we do not need to do tricky things with passing references.
        internal static GameObject CentralNetworkObject;
        private static GameObject _centralNetworkObjectSpawned;


        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "Miyowi";
        public const string PluginName = "ArtifactOfPrestige";
        public const string PluginVersion = "1.2.0";

        public static PluginInfo pluginInfo;

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
            CentralNetworkObject = tmpGo.InstantiateClone("somethingUnique");
            GameObject.Destroy(tmpGo);
            CentralNetworkObject.AddComponent<Networking>();

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

    public class Networking : NetworkBehaviour
    {
        private static Networking instance;

        private void Awake()
        {
            instance = this;
        }
        public static void InvokeAddIndicator()
        {
            ArtifactOfPrestige.CheckNetworkObject();
            instance.RpcAddIndicator();
        }

        // Called when a mountain shrine is hit
        [ClientRpc]
        private void RpcAddIndicator()
        {
            ArtifactOfPrestige.shrineBonusStacks++;
            if (ArtifactOfPrestige.shrineBonusStacks > 1)
            {
                var instance = TeleporterInteraction.instance;
                var original = instance.bossShrineIndicator;
                var child = GameObject.Instantiate(original);
                child.transform.parent = instance.gameObject.transform;
                child.transform.position = original.transform.position + new Vector3(0, ArtifactOfPrestige.offset + 2, 0);
                ArtifactOfPrestige.offset += 2;
                child.SetActive(true);
                ArtifactOfPrestige.localIndicators.Add(child);
            }
        }
    }
}
