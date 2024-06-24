using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

using HarmonyLib;

using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Facts;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.CasterCheckers;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.UnitLogic.Mechanics.Conditions;
using Kingmaker.UnitLogic.Mechanics.Properties;
using Kingmaker.UnitLogic.Parts;

using MicroWrath;
using MicroWrath.BlueprintsDb;
using MicroWrath.Components;
using MicroWrath.Extensions;
using MicroWrath.Extensions.Components;
using MicroWrath.InitContext;
using MicroWrath.Localization;
using MicroWrath.Util;

using static MicroWrath.Encyclopedia;

namespace AlternateRacialTraits.Features.Aasimar;

[AllowedOn(typeof(BlueprintUnitFact))]
[HarmonyPatch]
internal class ContextSpellResistance : UnitFactComponentDelegate<ContextSpellResistance.Data>
{
    public ConditionsChecker Conditions = new();
    public ContextValue Value = 0;

    public bool CanApply(MechanicsContext context)
    {
        var result = Conditions.Check();

        MicroLogger.Debug(() => "Condition check");
        MicroLogger.Debug(() => $"Caster: {context.MaybeCaster}");
        MicroLogger.Debug(() => $"Owner: {context.MaybeOwner}");

        MicroLogger.Debug(() => $"Check passed? {result}");

        return result;
    }
    
    public override void OnTurnOn()
    {
        var value = this.Value.Calculate(base.Fact.MaybeContext);

        base.Data.Id = base.Owner.Ensure<UnitPartSpellResistance>()
            .AddResistance(value, base.Fact.UniqueId, null, null, null);
    }

    public override void OnTurnOff()
    {
        if (base.Data.Id is { } id)
        {
            base.Owner.Get<UnitPartSpellResistance>()?.Remove(id);

            base.Data.Id = null;
        }
    }

    new public class Data
    {
        public int? Id;
    }

    static bool CanApply(
        bool result,
        UnitPartSpellResistance instance,
        MechanicsContext? context,
        //BlueprintAbility? ability,
        UnitEntityData? caster)
    {
#if DEBUG
        var timer = System.Diagnostics.Stopwatch.StartNew();
#endif
        if (result)
        {
            foreach (var sr in instance.SRs)
            {
                if (instance.Owner.Facts.FindById(sr.FactId) is { } fact &&
                    fact.GetComponent<ContextSpellResistance>() is { } csr)
                {
                    MicroLogger.Debug(() => $"Caster: {((caster ?? context?.MaybeCaster)?.ToString() ?? "NULL")}");

                    context = new MechanicsContext(caster ?? context?.MaybeCaster, instance.Owner, csr.OwnerBlueprint, context);

                    using (context.GetDataScope(instance.Owner.Unit))
                    {
                        result = csr.CanApply(context);
                    }

                    break;
                }
            }
        }
#if DEBUG
        timer.Stop();
        MicroLogger.Debug(() => $"{nameof(ContextSpellResistance)}.{nameof(CanApply)} took {timer.ElapsedMilliseconds}ms");
#endif
        return result;
    }

    static bool CanApply(
        bool result,
        UnitPartSpellResistance instance,
        MechanicsContext? context) =>
        CanApply(result, instance, context, caster: null);

    static bool CanApply(
        bool result,
        UnitPartSpellResistance instance,
        UnitEntityData? caster = null) =>
        CanApply(result, instance, context: null, caster);

    [HarmonyPatch(typeof(UnitPartSpellResistance), nameof(UnitPartSpellResistance.GetValue), [typeof(MechanicsContext)])]
    [HarmonyTranspiler]
    static IEnumerable<CodeInstruction> GetValue_MechanicsContext_Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        foreach (var i in instructions)
        {
            yield return i;

            if (i.Calls(AccessTools.Method(
                typeof(UnitPartSpellResistance),
                nameof(UnitPartSpellResistance.CanApply),
                [
                    typeof(UnitPartSpellResistance.SpellResistanceValue),
                    typeof(MechanicsContext)
                ])))
            {
                yield return new(OpCodes.Ldarg_0);
                yield return new(OpCodes.Ldarg_1);
                yield return CodeInstruction.Call(
                    (bool result,
                    UnitPartSpellResistance instance,
                    MechanicsContext context) =>
                    CanApply(result, instance, context));
            }
        }
    }

    [HarmonyPatch(
        typeof(UnitPartSpellResistance),
        nameof(UnitPartSpellResistance.GetValue),
        [typeof(BlueprintAbility), typeof(UnitEntityData)])]
    [HarmonyTranspiler]
    static IEnumerable<CodeInstruction> GetValue_BlueprintAbility_UnitEntityData_Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        foreach (var i in instructions)
        {
            yield return i;

            if (i.Calls(AccessTools.Method(
                typeof(UnitPartSpellResistance),
                nameof(UnitPartSpellResistance.CanApply),
                [
                    typeof(UnitPartSpellResistance.SpellResistanceValue),
                    typeof(BlueprintAbility),
                    typeof(UnitEntityData),
                    typeof(SpellDescriptor?),
                    typeof(SpellSchool?),
                    typeof(AbilityData),
                    typeof(MechanicsContext)
                ])))
            {
                yield return new(OpCodes.Ldarg_0);
                yield return new(OpCodes.Ldarg_2);
                yield return CodeInstruction.Call(
                    (bool result,
                    UnitPartSpellResistance instance,
                    UnitEntityData caster) =>
                    CanApply(result, instance, caster));
            }
        }
    }
}

internal static partial class ExaltedResistance
{
    [LocalizedString]
    internal const string DisplayName = "Exalted Resistance";

    [LocalizedString]
    internal static readonly string Description =
        $"An aasimar with this racial {new Link(Page.Trait, "trait")} gains " +
        $"{new Link(Page.Spell_Resistance, "spell resistance")} equal to 5 + her " +
        $"{new Link(Page.Character_Level, "level")} against {new Link(Page.Spell, "spells")} and " +
        $"spell-like abilities with the {new Link(Page.Spell_Descriptor, "evil descriptor")}, " +
        "as well as any spells and spell-like abilities cast by evil outsiders. " +
        "This racial trait replaces celestial resistance.";

    internal static IInitContextBlueprint<BlueprintFeature> Create()
    {
        var srVsEvilDescriptorFeature =
            InitContext.NewBlueprint<BlueprintFeature>(GeneratedGuid.Get("ExaltedResistanceVsEvilDescriptor"))
                .Map(feature =>
                {
                    feature.m_DisplayName = Localized.DisplayName;

                    feature.HideInCharacterSheetAndLevelUp = true;
                    //feature.HideInUI = true;

                    _ = feature.AddContextRankConfig(c =>
                    {
                        c.m_BaseValueType = ContextRankBaseValueType.CharacterLevel;
                        c.m_StepLevel = 5;
                        c.m_Type = AbilityRankType.Default;
                        c.m_Progression = ContextRankProgression.BonusValue;
                    });

                    _ = feature.AddSpellResistanceAgainstSpellDescriptor(c =>
                    {
                        c.SpellDescriptor = SpellDescriptor.Evil;

                        c.Value.ValueType = ContextValueType.Rank;
                        c.Value.ValueRank = AbilityRankType.Default;

                    });

                    return feature;
                })
                .AddBlueprintDeferred(GeneratedGuid.ExaltedResistanceVsEvilDescriptor);

        var srVsEvilOutsiderFeature =
            InitContext.NewBlueprint<BlueprintFeature>(GeneratedGuid.Get("ExaltedResistanceVsEvilOutsider"))
                .Combine(BlueprintsDb.Owlcat.BlueprintFeature.OutsiderType)
                .Combine(BlueprintsDb.Owlcat.BlueprintFeature.SubtypeEvil)
                .Map(bps =>
                {
                    var (feature, outsider, evil) = bps.Flatten();

                    feature.m_DisplayName = Localized.DisplayName;

                    feature.HideInCharacterSheetAndLevelUp = true;
                    //feature.HideInUI = true;

                    _ = feature.AddContextRankConfig(c =>
                    {
                        c.m_BaseValueType = ContextRankBaseValueType.CharacterLevel;
                        c.m_StepLevel = 5;
                        c.m_Type = AbilityRankType.Default;
                        c.m_Progression = ContextRankProgression.BonusValue;
                    });

                    _ = feature.AddComponent<ContextSpellResistance>(c =>
                    {
                        c.Value.ValueType = ContextValueType.Rank;
                        c.Value.ValueRank = AbilityRankType.Default;

                        c.Conditions.Operation = Operation.And;
                        _ = c.Conditions.Add(
                            new ContextConditionCasterHasFact
                            {
                                m_Fact = outsider.ToReference<BlueprintUnitFactReference>()
                            },
                            new ContextConditionCasterHasFact
                            {
                                m_Fact = evil.ToReference<BlueprintUnitFactReference>()
                            });
                    });

                    return feature;
                })
                .AddBlueprintDeferred(GeneratedGuid.ExaltedResistanceVsEvilOutsider);

        var feature =
            InitContext.NewBlueprint<BlueprintFeature>(GeneratedGuid.Get("ExaltedResistance"))
                .Combine(srVsEvilDescriptorFeature)
                .Combine(srVsEvilOutsiderFeature)
                .Map(bps =>
                {
                    var (feature, vsEvilSpell, vsEvilOutsider) = bps.Flatten();

                    feature.m_DisplayName = Localized.DisplayName;
                    feature.m_Description = Localized.Description;

                    feature.SetIcon("5895704fc09cae446b64117e3c52e06b", 21300000);
                   
                    feature.Groups = [FeatureGroup.Racial];

                    _ = feature.AddAddFacts(c =>
                    {
                        c.m_Facts =
                        [
                            vsEvilSpell.ToReference<BlueprintUnitFactReference>(),
                            vsEvilOutsider.ToReference<BlueprintUnitFactReference>()
                        ];
                    });

                    _ = feature.AddPrerequisiteFeature(BlueprintsDb.Owlcat.BlueprintFeature.CelestialResistance, removeOnApply: true);

                    return feature;
                });
        
        return feature.AddBlueprintDeferred(GeneratedGuid.ExaltedResistance);
    }
}
