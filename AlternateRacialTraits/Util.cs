using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Kingmaker.UnitLogic.Class.LevelUp.Actions;
using Kingmaker.UnitLogic.Class.LevelUp;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Blueprints.Classes;
using Kingmaker.UnitLogic;
using Kingmaker.Blueprints;
using HarmonyLib;
using Kingmaker.UI.MVVM._VM.CharGen.Phases.FeatureSelector;
using Kingmaker.UI.Common;
using Kingmaker.UI.MVVM._VM.CharGen.Phases;
using Kingmaker.Blueprints.Classes.Selection;

namespace AlternateRacialTraits
{
    internal static class Util
    {
        internal static void AddLevelUpSelection(UnitEntityData unit, IEnumerable<BlueprintFeatureBaseReference> features, FeatureSource source)
        {
            LevelUpController? controller = Kingmaker.Game.Instance?.LevelUpController;
            if (controller == null) { return; }
            if (controller.State.Mode == LevelUpState.CharBuildMode.Mythic) { return; }
            //if (unit.Descriptor.Progression.CharacterLevel > 1) { return; }

            var featureBps = features.Select(r => r.Get()).ToArray();
            
            LevelUpHelper.AddFeaturesFromProgression(
                controller.State,
                unit,
                featureBps,
                source,
                0);
        }

    }

    [HarmonyPatch]
    [AllowedOn(typeof(BlueprintFeatureSelection))]
    [AllowedOn(typeof(BlueprintParametrizedFeature))]
    internal class SelectFeaturePriority : UnitFactComponentDelegate
    {
        public LevelUpActionPriority Priority = LevelUpActionPriority.Features;

        [HarmonyPatch(typeof(SelectFeature), nameof(SelectFeature.CalculatePriority))]
        [HarmonyPostfix]
        static LevelUpActionPriority CalculatePriority_Postfix(LevelUpActionPriority __result, IFeatureSelection selection)
        {
            if (selection is BlueprintScriptableObject blueprint &&
                blueprint.GetComponent<SelectFeaturePriority>() is { } component)
                return component.Priority;

            return __result;
        }
    }

    [HarmonyPatch(
        typeof(CharGenFeatureSelectorPhaseVM),
        nameof(CharGenFeatureSelectorPhaseVM.OrderPriority),
        MethodType.Getter)]
    static class Background_OrderPriority_Patch
    {
        static int Postfix(int __result, CharGenFeatureSelectorPhaseVM __instance)
        {
            FeatureGroup featureGroup = UIUtilityUnit.GetFeatureGroup(__instance.FeatureSelectorStateVM?.Feature);
            if (featureGroup == FeatureGroup.BackgroundSelection)
            {
                if (__result < (((int)CharGenPhaseBaseVM.ChargenPhasePriority.AbilityScores * 1000) - 100))
                    return __result + 100;
            }

            return __result;
        }
    }
}
