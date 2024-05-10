using System;
using System.Collections.Generic;
using System.Text;
using BepInEx.Configuration;
using UnityEngine.Networking;
using UnityEngine;
using BepInEx;
using R2API;
using RoR2;
using System.Linq;
using UnityEngine.SceneManagement;
using static RoR2.TeleporterInteraction;
using UnityEngine.UIElements;


namespace ArtifactOfPrestige
{    class Artifact : ArtifactBase
    {
        public static ConfigEntry<int> TimesToPrintMessageOnStart;
        public override string ArtifactName => "Artifact of Prestige";
        public override string ArtifactLangTokenName => "ARTIFACT_OF_PRESTIGE";
        public override string ArtifactDescription => "At least one Shrine of the Mountain spawns every stage. Shrine of the Mountain effects are permanent.";
        public override Sprite ArtifactEnabledIcon => Assets.mainAssetBundle.LoadAsset<Sprite>("enabled.png");
        public override Sprite ArtifactDisabledIcon => Assets.mainAssetBundle.LoadAsset<Sprite>("disabled.png");
        public Color pink = new Color(0.8f, 0.13f, 0.6f, 1.0f);
        public override void Init(ConfigFile config)
        {
            CreateLang();
            CreateArtifact();
            Hooks();
        }

        public override void Hooks()
        {
            Run.onRunStartGlobal += ResetValues;
            Stage.onStageStartGlobal += SetValues;
            TeleporterInteraction.onTeleporterChargedGlobal += HideLocalIndicators;
            On.RoR2.TeleporterInteraction.AddShrineStack += UpdateValues;
            On.RoR2.SceneDirector.PopulateScene += SpawnShrine;
            On.RoR2.ShrineBossBehavior.Start += ShrineMat;
            On.RoR2.TeleporterInteraction.Awake += TPMat;
        }

        private void UpdateValues(On.RoR2.TeleporterInteraction.orig_AddShrineStack orig, TeleporterInteraction self)
        {
            if (self.activationState <= ActivationState.IdleToCharging)
            {
                ArtifactOfPrestige.bonusRewardCount++;
                ArtifactOfPrestige.NetworkshowExtraBossesIndicator = true;
                Networking.InvokeAddIndicator(ArtifactEnabled);
            }
            orig(self);
        }

        private void SetValues(Stage stage)
        {    
            if ((TeleporterInteraction.instance ?? false) && (ArtifactEnabled || ArtifactOfPrestige.stackOutsidePrestige.Value))
            {
                ArtifactOfPrestige.localIndicators = [];
                ArtifactOfPrestige.offset = 0;
                var tp = TeleporterInteraction.instance;
                if (NetworkServer.active)
                {
                    tp.bossGroup.bonusRewardCount = ArtifactOfPrestige.bonusRewardCount;
                    tp.shrineBonusStacks = ArtifactOfPrestige.shrineBonusStacks;
                    tp.NetworkshowExtraBossesIndicator = ArtifactOfPrestige.NetworkshowExtraBossesIndicator;
                }
                if (ArtifactOfPrestige.shrineBonusStacks > 1 && (ArtifactOfPrestige.stackingIndicators.Value || ArtifactOfPrestige.stackOutsidePrestige.Value))
                {
                    for (int i = 0; i < ArtifactOfPrestige.shrineBonusStacks - 1; i++)
                    {
                        LocalAddIndicator();
                    }
                }
            }
        }

        private void ShrineMat(On.RoR2.ShrineBossBehavior.orig_Start orig, ShrineBossBehavior self)
        {
            if ((ArtifactEnabled && ArtifactOfPrestige.colouredIndicators.Value) || ArtifactOfPrestige.colouredOutsidePrestige.Value)
            {
                var myRend = self.gameObject.transform.Find("Symbol").GetComponent<MeshRenderer>();
                Material[] materials = myRend.materials;
                materials[0].SetColor("_TintColor", ArtifactOfPrestige.indicatorColor.Value);
                myRend.materials = materials;
            }
            orig(self);
        }

        private void TPMat(On.RoR2.TeleporterInteraction.orig_Awake orig, TeleporterInteraction self)
        {
            orig(self);
            if ((ArtifactEnabled && ArtifactOfPrestige.colouredIndicators.Value) || ArtifactOfPrestige.colouredOutsidePrestige.Value)
            {
                var myRend = self.bossShrineIndicator.GetComponent<MeshRenderer>();
                Material[] materials = myRend.materials;
                materials[0].SetColor("_TintColor", ArtifactOfPrestige.indicatorColor.Value);
                myRend.materials = materials;
            }
        }

        private void ResetValues(Run run)
        {
            if (ArtifactEnabled) { ArtifactOfPrestige.ResetValues(); }
        }

        private void SpawnShrine(On.RoR2.SceneDirector.orig_PopulateScene orig, SceneDirector self)
        {
            if (ArtifactEnabled && SceneInfo.instance.countsAsStage && (bool)self.teleporterSpawnCard)
            {
                Xoroshiro128Plus xoroshiro128Plus2 = new Xoroshiro128Plus(self.rng.nextUlong);

                DirectorCore.instance.TrySpawnObject(new DirectorSpawnRequest(LegacyResourcesAPI.Load<SpawnCard>("SpawnCards/InteractableSpawnCard/iscShrineBoss"), new DirectorPlacementRule
                {
                    placementMode = DirectorPlacementRule.PlacementMode.Random
                }, xoroshiro128Plus2));
            }
            orig(self);
        }

        private void HideLocalIndicators(TeleporterInteraction teleporterInteraction)
        {
            if (ArtifactEnabled)
            {
                foreach (var indicator in ArtifactOfPrestige.localIndicators)
                {
                    indicator.SetActive(false);
                }
            }
        }

        private void LocalAddIndicator()
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
