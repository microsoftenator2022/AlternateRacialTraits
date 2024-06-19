using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

using HarmonyLib;

using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UI.Common;
using Kingmaker.UI.MVVM._VM.CharGen.Phases;
using Kingmaker.UI.MVVM._VM.CharGen.Phases.FeatureSelector;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Class.LevelUp;
using Kingmaker.UnitLogic.Class.LevelUp.Actions;
using Kingmaker.Utility;

using MicroWrath;

namespace AlternateRacialTraits;

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

// Don't yell at me about missing feature param when I haven't even finished level up
[HarmonyPatch]
static class ShutUpShutUpShutUpShutUp
{
    static bool IsCurrentlyLevelingUp()
    {
        if (Game.Instance.LevelUpController is not { } controller)
            return false;

        return !controller.Committed;
    }

    [HarmonyPatch(typeof(Feature), nameof(Feature.OnActivate))]
    [HarmonyTranspiler]
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var index = instructions.FindIndex(ci => ci.Calls(AccessTools.Method(typeof(FeatureParam), "op_Equality")));

        if (index < 0)
            throw new Exception("Could not find target instruction");

        var iList = instructions.ToList();

        var ifFalse = iList[index + 1];
        
        iList.InsertRange(index + 2,
        [
            CodeInstruction.Call(() => IsCurrentlyLevelingUp()),
            new(OpCodes.Brtrue_S, ifFalse.operand)
        ]);

        return iList;
    }
}
