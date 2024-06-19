using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Kingmaker.Blueprints.Classes.Selection;

using Kingmaker.Blueprints;

using Kingmaker.EntitySystem.Stats;

namespace AlternateRacialTraits;


[HarmonyPatch]
[AllowedOn(typeof(BlueprintParametrizedFeature))]
internal class ParamStats : BlueprintComponent
{
    public StatType[] Stats = [];

    static readonly Type ExtractSkillsEnumerator =
        typeof(BlueprintParametrizedFeature)
            .GetNestedTypes(AccessTools.all)
            .Single(t => t.GetFields(AccessTools.all).Any(f => f.FieldType.Equals(typeof(StatType[]))));

    static readonly FieldInfo BlueprintField =
        ExtractSkillsEnumerator.GetFields().Single(f => f.FieldType.Equals(typeof(BlueprintParametrizedFeature)));

    [HarmonyTargetMethod]
    static MethodInfo TargetMethod() =>
        ExtractSkillsEnumerator
            .GetInterfaceMap(typeof(System.Collections.IEnumerator))
            .TargetMethods
            .Single(m => m.Name == nameof(System.Collections.IEnumerator.MoveNext));

    static StatType[] GetStats(object enumerator)
    {
        var blueprint = BlueprintField.GetValue(enumerator) as BlueprintParametrizedFeature;

        if (blueprint?.GetComponent<ParamStats>() is { } component)
        {
            return component.Stats;
        }

        return StatTypeHelper.Skills;
    }

    [HarmonyTranspiler]
    static IEnumerable<CodeInstruction> ExtractSkills_Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        foreach (var i in instructions)
        {
            if (i.opcode == OpCodes.Ldsfld && (FieldInfo)i.operand == AccessTools.Field(typeof(StatTypeHelper), nameof(StatTypeHelper.Skills)))
            {
                yield return new(OpCodes.Ldarg_0);
                yield return CodeInstruction.Call((object enumerator) => GetStats(enumerator));
            }
            else
                yield return i;
        }
    }
}
