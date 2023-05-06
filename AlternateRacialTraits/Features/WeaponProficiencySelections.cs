using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading.Tasks;

using HarmonyLib;

using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Enums;
using Kingmaker.UI.MVVM._VM.ServiceWindows.CharacterInfo.Sections.Martial.WeaponProficiency;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.FactLogic;

using MicroWrath;
using MicroWrath.BlueprintInitializationContext;
using MicroWrath.BlueprintsDb;
using MicroWrath.Extensions;
using MicroWrath.Extensions.Components;
using MicroWrath.Localization;
using MicroWrath.Util.Linq;

namespace AlternateRacialTraits.Features
{
    [AllowedOn(typeof(BlueprintParametrizedFeature))]
    public class AddWeaponProficiencyParametrized : AddProficiencies
    {
        public AddWeaponProficiencyParametrized()
        {
            this.WeaponProficiencies ??= new WeaponCategory[0];
        }

        public override void OnTurnOn()
        {
            if (Param.WeaponCategory is WeaponCategory weapon && !this.WeaponProficiencies.Contains(weapon))
                this.WeaponProficiencies = this.WeaponProficiencies.Append(weapon);

            base.OnTurnOn();
        }

        public override void OnTurnOff()
        {
            if (Param.WeaponCategory is WeaponCategory weapon && !this.WeaponProficiencies.Contains(weapon))
                this.WeaponProficiencies = this.WeaponProficiencies.Append(weapon);

            base.OnTurnOff();
        }
    }

    internal static class WeaponProficiencySelections
    {
        private static BlueprintInitializationContext.ContextInitializer<BlueprintParametrizedFeature>
            ExoticWeaponProficiencyParametrized(BlueprintInitializationContext context) =>
            context.NewBlueprint<BlueprintParametrizedFeature>(
                GeneratedGuid.ExoticWeaponProficiencyParametrized,
                nameof(ExoticWeaponProficiencyParametrized))
                .Map((BlueprintParametrizedFeature blueprint) =>
                {
                    var eps = BlueprintsDb.Owlcat.BlueprintFeatureSelection.ExoticWeaponProficiencySelection.GetBlueprint()!;

                    blueprint.m_DisplayName = eps.m_DisplayName;
                    blueprint.m_Description = eps.m_Description;
                    blueprint.m_Icon = eps.Icon;

                    blueprint.ParameterType = FeatureParameterType.WeaponCategory;
                    blueprint.WeaponSubCategory = WeaponSubCategory.Exotic;

                    blueprint.AddComponent<AddWeaponProficiencyParametrized>();

                    blueprint.Groups = new[] { FeatureGroup.CombatFeat };

                    return blueprint;
                });

        [LocalizedString]
        internal const string MartialWeaponProficiencyDisplayName = "Martial Weapon Proficiency";

        [LocalizedString]
        internal const string MartialWeaponProficiencyDescription = "You become proficient with a single martial weapon";

        private static BlueprintInitializationContext.ContextInitializer<BlueprintParametrizedFeature>
            MartialWeaponProficiencyParametrized(BlueprintInitializationContext context) =>
            context.NewBlueprint<BlueprintParametrizedFeature>(
                GeneratedGuid.MartialWeaponProficiencyParametrized,
                nameof(MartialWeaponProficiencyParametrized))
                .Map((BlueprintParametrizedFeature blueprint) =>
                {
                    blueprint.m_DisplayName = LocalizedStrings.Features_WeaponProficiencySelections_MartialWeaponProficiencyDisplayName;
                    blueprint.m_Description = LocalizedStrings.Features_WeaponProficiencySelections_MartialWeaponProficiencyDescription;
                    blueprint.SetIcon("4f7a69c8c0e877149ac5f0ee124343e8", 21300000);

                    blueprint.ParameterType = FeatureParameterType.WeaponCategory;
                    blueprint.WeaponSubCategory = WeaponSubCategory.Martial;

                    blueprint.AddComponent<AddWeaponProficiencyParametrized>();

                    blueprint.AddComponent<PrerequisiteNoFeature>(c =>
                        c.m_Feature = BlueprintsDb.Owlcat.BlueprintFeature
                            .MartialWeaponProficiency.ToReference<BlueprintFeature, BlueprintFeatureReference>());

                    blueprint.Groups = new[] { FeatureGroup.CombatFeat };

                    return blueprint;
                });

        [Init]
        internal static void Init()
        {
            var initContext = new BlueprintInitializationContext(Triggers.BlueprintsCache_Init);

            ExoticWeaponProficiencyParametrized(initContext).Register();
            MartialWeaponProficiencyParametrized(initContext).Register();
        }
    }

    [HarmonyPatch(typeof(CharInfoWeaponProficiencyVM))]
    public static class CharInfoWeaponProficiencyVM_Patch
    {
        [HarmonyPatch(nameof(CharInfoWeaponProficiencyVM.GetProficiency))]
        [HarmonyPostfix]
        static List<CharInfoWeaponProficiencyEntryVM> GetProficiency_Postfix(
            List<CharInfoWeaponProficiencyEntryVM> __result,
            CharInfoWeaponProficiencyVM __instance,
            UnitDescriptor unit)
        {
            __result = unit.Progression.Features.Visible
                .Where(f =>
                {
                    var ac = f.GetComponent<AddProficiencies>();
                    return ac != default && ac.WeaponProficiencies.Length > 0;
                })
                .Select(f =>
                {
                    return __instance.AddDisposableAndReturn<CharInfoWeaponProficiencyEntryVM>(new()
                    {
                        DisplayName = f.Name,
                        Description = f.Description
                    });
                })
                .ToList();

            return __result;
        }
    }
}
