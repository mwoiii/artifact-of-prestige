using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using RiskOfOptions.OptionConfigs;
using RiskOfOptions.Options;
using RiskOfOptions;
using UnityEngine;
using ArtifactOfPrestige;

namespace ArtifactOfPrestige
{
    public static class RiskOfOptionsCompatibility
    {
        private static bool? _enabled;

        public static bool enabled
        {
            get
            {
                if (_enabled == null)
                {
                    _enabled = BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.rune580.riskofoptions");
                }
                return (bool)_enabled;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static void OptionsInit()
        {
            ModSettingsManager.AddOption(new CheckBoxOption(ArtifactOfPrestige.stackingIndicators));
            ModSettingsManager.AddOption(new CheckBoxOption(ArtifactOfPrestige.colouredIndicators));
            ModSettingsManager.AddOption(new ColorOption(ArtifactOfPrestige.indicatorColor, new ColorOptionConfig()));
            ModSettingsManager.AddOption(new CheckBoxOption(ArtifactOfPrestige.stackOutsidePrestige));
            ModSettingsManager.AddOption(new CheckBoxOption(ArtifactOfPrestige.colouredOutsidePrestige));

            ModSettingsManager.SetModDescription("Allows customisation of how the artifact affects Shrine of the Mountain visuals, or to enable the visuals even without the artifact because it's kinda cool I think.");

            Sprite icon = Assets.mainAssetBundle.LoadAsset<Sprite>("icon.png");
            ModSettingsManager.SetModIcon(icon);
        }
    }
}