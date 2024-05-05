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


namespace ArtifactOfPrestige
{    class Artifact : ArtifactBase
    {
        public static ConfigEntry<int> TimesToPrintMessageOnStart;
        public override string ArtifactName => "Artifact of Prestige";
        public override string ArtifactLangTokenName => "ARTIFACT_OF_PRESTIGE";
        public override string ArtifactDescription => "At least one Shrine of the Mountain spawns every stage. Shrine of the Mountain effects are permanent.";
        public override Sprite ArtifactEnabledIcon => Assets.mainAssetBundle.LoadAsset<Sprite>("enabled.png");
        public override Sprite ArtifactDisabledIcon => Assets.mainAssetBundle.LoadAsset<Sprite>("disabled.png");
        public override void Init(ConfigFile config)
        {
            CreateLang();
            CreateArtifact();
            Hooks();
        }

        public override void Hooks()
        {
            Run.onRunStartGlobal += ResetValues;
            Stage.onServerStageBegin += SetValues;
            On.RoR2.TeleporterInteraction.AddShrineStack += UpdateValues;
            On.RoR2.SceneDirector.PopulateScene += SpawnShrine;
        }

        private void UpdateValues(On.RoR2.TeleporterInteraction.orig_AddShrineStack orig, TeleporterInteraction self)
        {
            if (NetworkServer.active && self.activationState <= ActivationState.IdleToCharging && ArtifactEnabled)
            {
                ArtifactOfPrestige.bonusRewardCount++;
                ArtifactOfPrestige.shrineBonusStacks++;
                ArtifactOfPrestige.NetworkshowExtraBossesIndicator = true;
            }
            orig(self);
        }

        private void SetValues(Stage stage)
        {
            if ((TeleporterInteraction.instance ?? false) && ArtifactEnabled)
            {
                var tp = TeleporterInteraction.instance;
                tp.bossGroup.bonusRewardCount = ArtifactOfPrestige.bonusRewardCount;
                tp.shrineBonusStacks = ArtifactOfPrestige.shrineBonusStacks;
                tp.NetworkshowExtraBossesIndicator = ArtifactOfPrestige.NetworkshowExtraBossesIndicator;
            }
        }

        private void ResetValues(Run run)
        {
            if (NetworkServer.active && ArtifactEnabled) { ArtifactOfPrestige.ResetValues(); }
        }

        private void SpawnShrine(On.RoR2.SceneDirector.orig_PopulateScene orig, SceneDirector self)
        {
            if (ArtifactEnabled && SceneInfo.instance.countsAsStage && (bool)self.teleporterSpawnCard)
            {
                Xoroshiro128Plus xoroshiro128Plus2 = new Xoroshiro128Plus(self.rng.nextUlong);
                var spawnCard = LegacyResourcesAPI.Load<SpawnCard>("SpawnCards/InteractableSpawnCard/iscShrineBoss");

                DirectorCore.instance.TrySpawnObject(new DirectorSpawnRequest(LegacyResourcesAPI.Load<SpawnCard>("SpawnCards/InteractableSpawnCard/iscShrineBoss"), new DirectorPlacementRule
                {
                    placementMode = DirectorPlacementRule.PlacementMode.Random
                }, xoroshiro128Plus2));
            }
            orig(self);
        }
    }
}
