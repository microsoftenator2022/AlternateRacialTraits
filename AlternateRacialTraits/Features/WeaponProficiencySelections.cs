using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading.Tasks;

using HarmonyLib;

using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Enums;
using Kingmaker.UI.MVVM._VM.ServiceWindows.CharacterInfo.Sections.Martial.WeaponProficiency;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.FactLogic;

using MicroWrath;
using MicroWrath.BlueprintsDb;
using MicroWrath.Extensions;
using MicroWrath.Extensions.Components;
using MicroWrath.InitContext;
using MicroWrath.Localization;
using MicroWrath.Util;
using MicroWrath.Util.Linq;

namespace AlternateRacialTraits.Features;

[AllowedOn(typeof(BlueprintParametrizedFeature))]
public class AddWeaponProficiencyParametrized : AddProficiencies
{
    public AddWeaponProficiencyParametrized()
    {
        this.WeaponProficiencies ??= [];
    }

    public override void OnTurnOn()
    {
        if (this.Param.WeaponCategory is WeaponCategory weapon && !this.WeaponProficiencies.Contains(weapon))
            this.WeaponProficiencies = this.WeaponProficiencies.Append(weapon);

        base.OnTurnOn();
    }

    public override void OnTurnOff()
    {
        if (this.Param.WeaponCategory is WeaponCategory weapon && !this.WeaponProficiencies.Contains(weapon))
            this.WeaponProficiencies = this.WeaponProficiencies.Append(weapon);

        base.OnTurnOff();
    }
}

internal static class WeaponProficiencySelections
{
    private static IInitContext<BlueprintParametrizedFeature> ExoticWeaponProficiencyParametrized() =>
        InitContext
            .NewBlueprint<BlueprintParametrizedFeature>(GeneratedGuid.ExoticWeaponProficiencyParametrized)
            .Combine(BlueprintsDb.Owlcat.BlueprintFeatureSelection.ExoticWeaponProficiencySelection)
            .Map(bps =>
            {
                var (blueprint, eps) = bps;

                blueprint.m_DisplayName = eps.m_DisplayName;
                blueprint.m_Description = eps.m_Description;
                blueprint.m_Icon = eps.Icon;

                blueprint.ParameterType = FeatureParameterType.WeaponCategory;
                blueprint.WeaponSubCategory = WeaponSubCategory.Exotic;

                _ = blueprint.AddComponent<AddWeaponProficiencyParametrized>();

                blueprint.Groups = [FeatureGroup.CombatFeat];

                return blueprint;
            });

    [LocalizedString]
    internal const string MartialWeaponProficiencyDisplayName = "Martial Weapon Proficiency";

    [LocalizedString]
    internal const string MartialWeaponProficiencyDescription = "You become proficient with a single martial weapon";

    private static IInitContext<BlueprintParametrizedFeature>
        MartialWeaponProficiencyParametrized() =>
            InitContext.NewBlueprint<BlueprintParametrizedFeature>(GeneratedGuid.MartialWeaponProficiencyParametrized)
            .Map((BlueprintParametrizedFeature blueprint) =>
            {
                blueprint.m_DisplayName = LocalizedStrings.Features_WeaponProficiencySelections_MartialWeaponProficiencyDisplayName;
                blueprint.m_Description = LocalizedStrings.Features_WeaponProficiencySelections_MartialWeaponProficiencyDescription;
                blueprint.SetIcon("4f7a69c8c0e877149ac5f0ee124343e8", 21300000);

                blueprint.ParameterType = FeatureParameterType.WeaponCategory;
                blueprint.WeaponSubCategory = WeaponSubCategory.Martial;

                _ = blueprint.AddComponent<AddWeaponProficiencyParametrized>();

                _ = blueprint.AddComponent<PrerequisiteNoFeature>(c =>
                    c.m_Feature = BlueprintsDb.Owlcat.BlueprintFeature
                        .MartialWeaponProficiency.ToReference<BlueprintFeature, BlueprintFeatureReference>());

                blueprint.Groups = [FeatureGroup.CombatFeat];

                return blueprint;
            });

    [Init]
    internal static void Init()
    {
        _ = ExoticWeaponProficiencyParametrized().AddOnTrigger(GeneratedGuid.ExoticWeaponProficiencyParametrized, Triggers.BlueprintsCache_Init);
        _ = MartialWeaponProficiencyParametrized().AddOnTrigger(GeneratedGuid.MartialWeaponProficiencyParametrized, Triggers.BlueprintsCache_Init);
    }
}

[HarmonyPatch(typeof(CharInfoWeaponProficiencyVM))]
public static class CharInfoWeaponProficiencyVM_Patch
{
    [HarmonyPatch(nameof(CharInfoWeaponProficiencyVM.GetProficiency))]
    [HarmonyTranspiler]
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        MicroLogger.Debug(() => $"{nameof(CharInfoWeaponProficiencyVM_Patch)}.{nameof(Transpiler)}");

        var t = typeof(CharInfoWeaponProficiencyEntryVM);

        var toMatch = new[]
        {
            new CodeInstruction(OpCodes.Ldloc_2),
            new CodeInstruction(OpCodes.Callvirt, typeof(UnitFact<BlueprintFeature>).GetMethod("get_Blueprint", AccessTools.allDeclared)),
            new CodeInstruction(OpCodes.Callvirt, typeof(BlueprintUnitFact).GetMethod("get_Name", AccessTools.allDeclared)),
            CodeInstruction.StoreField(t, "DisplayName")
        };

        var replaceWith = new[]
        {
            new CodeInstruction(OpCodes.Ldloc_2),
            new CodeInstruction(OpCodes.Callvirt, typeof(Feature).GetMethod("get_Name", AccessTools.allDeclared)),
            CodeInstruction.StoreField(t, "DisplayName")
        };

        return TranspilerUtil.ReplaceInstructions(instructions, toMatch, replaceWith);
    }
}
