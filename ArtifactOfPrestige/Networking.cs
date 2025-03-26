using BepInEx;
using R2API;
using RoR2;
using UnityEngine.Networking;
using System.Collections.Generic;
using System;
using System.Text;
using UnityEngine;
using ArtifactOfPrestige;

namespace ArtifactOfPrestige
{ 
    public class Networking : NetworkBehaviour
    {
        private static Networking instance;

        private void Awake()
        {
            instance = this;
        }
        public static void InvokeAddIndicator(bool artifactEnabled)
        {
            ArtifactOfPrestige.CheckNetworkObject();
            instance.RpcAddIndicator(artifactEnabled);
        }

        // Called when a mountain shrine is hit
        [ClientRpc]
        private void RpcAddIndicator(bool artifactEnabled)
        {
            if ((artifactEnabled && ArtifactOfPrestige.stackingIndicators.Value) || ArtifactOfPrestige.stackOutsidePrestige.Value)
            {
                ArtifactOfPrestige.shrineBonusStacks++;
            }
            if (TeleporterInteraction.instance?.activationState <= TeleporterInteraction.ActivationState.IdleToCharging && ArtifactOfPrestige.shrineBonusStacks > 1 && ArtifactOfPrestige.stackingIndicators.Value)
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
