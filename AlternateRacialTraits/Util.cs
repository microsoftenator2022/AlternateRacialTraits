using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

using AlternateRacialTraits.Features;

using HarmonyLib;

using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Facts;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UI.Common;
using Kingmaker.UI.MVVM._VM.CharGen.Phases;
using Kingmaker.UI.MVVM._VM.CharGen.Phases.FeatureSelector;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Class.LevelUp;
using Kingmaker.UnitLogic.Class.LevelUp.Actions;
using Kingmaker.Utility;

using MicroWrath;
using MicroWrath.Util;
using MicroWrath.Util.Linq;

namespace AlternateRacialTraits;

internal static class Util
{
    internal static void AddLevelUpSelection(UnitEntityData unit, IEnumerable<BlueprintFeatureBaseReference> features, FeatureSource source)
    {
        LevelUpController? controller = Kingmaker.Game.Instance?.LevelUpController;
        if (controller == null) { return; }
        if (controller.State.Mode == LevelUpState.CharBuildMode.Mythic) { return; }
        //if (unit.Descriptor.Progression.CharacterLevel > 1) { return; }

        var featureBps = features.Select(r => r.Get())
            .Where(feature =>
            {
                if (feature is not IFeatureSelection selection)
                    return true;

                if (!selection.Items
                    .Where(item => item is not BlueprintScriptableObject blueprint || blueprint.GetComponent<NoTraitsComponent>() is null)
                    .Any(item => selection.CanSelect(unit, controller.State, new(null, source, selection, 0, 0), item)))
                {
                    MicroLogger.Debug(() => $"Selection {selection} has no available selections. Ignoring.");
                    return false;
                }

                return true;
            })
            .ToArray();

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

//[HarmonyPatch(typeof(PrerequisiteFeaturesFromList))]
//static class PrerequisiteFeaturesFromList_HideEmptyFeatureNames_Patch
//{
//    [HarmonyPatch(nameof(PrerequisiteFeaturesFromList.GetUITextInternal))]
//    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator ilGen)
//    {
//        var getNameIndex = instructions.FindIndex(ci => ci.Calls(AccessTools.PropertyGetter(typeof(BlueprintUnitFact), nameof(BlueprintUnitFact.Name))));

//        var iPlusPlus = instructions.FindInstructionsIndexed(
//        [
//            ci => ci.opcode == OpCodes.Ldloc_2,
//            ci => ci.opcode == OpCodes.Ldc_I4_1,
//            ci => ci.opcode == OpCodes.Add,
//            ci => ci.opcode == OpCodes.Stloc_2
//        ]);

//        if (getNameIndex < 0 || iPlusPlus.Count() != 4)
//            return instructions;

//        var @continue = ilGen.DefineLabel();

//        var pop = iPlusPlus.First().index - 1;

//        var iList = instructions.ToList();

//        iList[pop].labels.Add(@continue);
        
//        var notEmpty = ilGen.DefineLabel();

//        iList.InsertRange(getNameIndex + 1,
//        [
//            new(OpCodes.Dup),
//            new(CodeInstruction.Call((string s) => String.IsNullOrEmpty(s))),
//            new(OpCodes.Brfalse_S, notEmpty),
//            new(OpCodes.Pop),
//            new(OpCodes.Br, @continue),
//            new(OpCodes.Nop) { labels = [notEmpty] }
//        ]);

//        return iList;
//    }
//}
