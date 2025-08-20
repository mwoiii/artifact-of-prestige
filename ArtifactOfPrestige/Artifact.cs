﻿using System.Collections.Generic;
using BepInEx.Configuration;
using Newtonsoft.Json;
using R2API.Networking;
using R2API.Networking.Interfaces;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;


namespace ArtifactOfPrestige {
    class Artifact : ArtifactBase {
        public static ConfigEntry<int> TimesToPrintMessageOnStart;
        public override string ArtifactName => "Artifact of Prestige";
        public override string ArtifactLangTokenName => "ARTIFACT_OF_PRESTIGE";
        public override string ArtifactDescription => "At least one Shrine of the Mountain spawns every stage. Shrine of the Mountain effects are permanent.";
        public override Sprite ArtifactEnabledIcon => Assets.mainAssetBundle.LoadAsset<Sprite>("enabled.png");
        public override Sprite ArtifactDisabledIcon => Assets.mainAssetBundle.LoadAsset<Sprite>("disabled.png");

        public Color pink = new Color(0.8f, 0.13f, 0.6f, 1.0f);
        public override void Init(ConfigFile config) {
            CreateLang();
            CreateArtifact();
            Hooks();
        }

        public override void Hooks() {
            Run.onRunStartGlobal += ResetValues;
            Stage.onStageStartGlobal += SetValues;
            On.RoR2.ShrineBossBehavior.AddShrineStack += UpdateValues;
            On.RoR2.SceneDirector.PopulateScene += SpawnShrine;
            On.RoR2.ShrineBossBehavior.Start += AllowShrineAfterTeleporter;
            TeleporterInteraction.onTeleporterChargedGlobal += HideLocalIndicators;
            On.RoR2.ShrineBossBehavior.Start += ShrineMat;
            On.RoR2.TeleporterInteraction.Awake += TPMat;

            // ProperSave compatibility
            if (ProperSaveCompatibility.enabled) {
                ProperSaveCompatibility.AddEvent(SavePrestigeSettings);
            }
        }

        private void SavePrestigeSettings(Dictionary<string, object> dict) {
            if (ArtifactEnabled) {
                string jsonString = JsonConvert.SerializeObject(new ArtifactOfPrestige_ProperSaveObj());
                dict.Add("ArtifactOfPrestigeObj", jsonString);
            }
        }

        private void LoadProperSave() {
            if (ProperSaveCompatibility.enabled) {
                if (ArtifactEnabled && ProperSaveCompatibility.IsLoading) {
                    string jsonString = ProperSaveCompatibility.GetModdedData("ArtifactOfPrestigeObj");

                    ArtifactOfPrestige_ProperSaveObj dataObj = JsonConvert.DeserializeObject<ArtifactOfPrestige_ProperSaveObj>(jsonString);
                    ArtifactOfPrestige.bonusRewardCount = dataObj.bonusRewardCount;
                    ArtifactOfPrestige.shrineBonusStacks = dataObj.shrineBonusStacks;
                    ArtifactOfPrestige.NetworkshowExtraBossesIndicator = dataObj.NetworkshowExtraBossesIndicator;
                }
            }
        }

        private void UpdateValues(On.RoR2.ShrineBossBehavior.orig_AddShrineStack orig, ShrineBossBehavior self, Interactor interactor) {
            ArtifactOfPrestige.bonusRewardCount++;
            ArtifactOfPrestige.NetworkshowExtraBossesIndicator = true;
            // Networking.InvokeAddIndicator(ArtifactEnabled);
            NetMessageExtensions.Send(new SyncAddIndicator(ArtifactEnabled), (NetworkDestination)1);

            orig(self, interactor);
        }

        private void SetValues(Stage stage) {
            ArtifactOfPrestige.localIndicators = [];
            ArtifactOfPrestige.offset = 0;

            if (ArtifactOfPrestige.stackOutsidePrestige.Value && !ArtifactEnabled) {
                ArtifactOfPrestige.shrineBonusStacks = 0;
            }

            if ((TeleporterInteraction.instance ?? false) && ArtifactEnabled) {
                var tp = TeleporterInteraction.instance;
                if (NetworkServer.active) {
                    tp.bossGroup.bonusRewardCount = ArtifactOfPrestige.bonusRewardCount;
                    tp.shrineBonusStacks = ArtifactOfPrestige.shrineBonusStacks;
                    tp.NetworkshowExtraBossesIndicator = ArtifactOfPrestige.NetworkshowExtraBossesIndicator;
                }
                if (ArtifactOfPrestige.shrineBonusStacks > 1 && ArtifactOfPrestige.stackingIndicators.Value) {
                    for (int i = 0; i < ArtifactOfPrestige.shrineBonusStacks - 1; i++) {
                        LocalAddIndicator();
                    }
                }
            }
        }

        private void ShrineMat(On.RoR2.ShrineBossBehavior.orig_Start orig, ShrineBossBehavior self) {
            if ((ArtifactEnabled && ArtifactOfPrestige.colouredIndicators.Value) || ArtifactOfPrestige.colouredOutsidePrestige.Value) {
                var myRend = self.gameObject.transform.Find("Symbol").GetComponent<MeshRenderer>();
                Material[] materials = myRend.materials;
                materials[0].SetColor("_TintColor", ArtifactOfPrestige.indicatorColor.Value);
                myRend.materials = materials;
            }
            orig(self);
        }

        private void TPMat(On.RoR2.TeleporterInteraction.orig_Awake orig, TeleporterInteraction self) {
            orig(self);
            if ((ArtifactEnabled && ArtifactOfPrestige.colouredIndicators.Value) || ArtifactOfPrestige.colouredOutsidePrestige.Value) {
                var myRend = self.bossShrineIndicator.GetComponent<MeshRenderer>();
                Material[] materials = myRend.materials;
                materials[0].SetColor("_TintColor", ArtifactOfPrestige.indicatorColor.Value);
                myRend.materials = materials;
            }
        }

        private void ResetValues(Run run) {
            if (ArtifactEnabled || ArtifactOfPrestige.stackOutsidePrestige.Value) {
                ArtifactOfPrestige.ResetValues();
            }
            LoadProperSave(); // loading from ProperSave, if it's active
        }

        private void SpawnShrine(On.RoR2.SceneDirector.orig_PopulateScene orig, SceneDirector self) {
            if (ArtifactEnabled && SceneInfo.instance.countsAsStage && (bool)self.teleporterSpawnCard) {
                Xoroshiro128Plus xoroshiro128Plus2 = new Xoroshiro128Plus(self.rng.nextUlong);

                DirectorCore.instance.TrySpawnObject(new DirectorSpawnRequest(LegacyResourcesAPI.Load<SpawnCard>("SpawnCards/InteractableSpawnCard/iscShrineBoss"), new DirectorPlacementRule {
                    placementMode = DirectorPlacementRule.PlacementMode.Random
                }, xoroshiro128Plus2));
            }
            orig(self);
        }

        private void HideLocalIndicators(TeleporterInteraction teleporterInteraction) {
            if (ArtifactEnabled || ArtifactOfPrestige.stackOutsidePrestige.Value) {
                foreach (var indicator in ArtifactOfPrestige.localIndicators) {
                    indicator.SetActive(false);
                }
            }
        }

        private void LocalAddIndicator() {
            var instance = TeleporterInteraction.instance;
            var original = instance.bossShrineIndicator;
            var child = GameObject.Instantiate(original);
            child.transform.parent = instance.gameObject.transform;
            child.transform.position = original.transform.position + new Vector3(0, ArtifactOfPrestige.offset + 2, 0);
            ArtifactOfPrestige.offset += 2;
            child.SetActive(true);
            ArtifactOfPrestige.localIndicators.Add(child);
        }

        private void AllowShrineAfterTeleporter(On.RoR2.ShrineBossBehavior.orig_Start orig, ShrineBossBehavior self) {
            orig(self);

            self.purchaseInteraction.setUnavailableOnTeleporterActivated = !ArtifactEnabled;
        }
    }
}
