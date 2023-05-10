using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Enums;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.FactLogic;

using MicroWrath;
using MicroWrath.BlueprintInitializationContext;
using MicroWrath.BlueprintsDb;
using MicroWrath.Extensions;
using MicroWrath.Extensions.Components;
using MicroWrath.Localization;

using static MicroWrath.Encyclopedia;


namespace AlternateRacialTraits.Features
{
    internal static class RacialWeaponFamiliarities
    {
        internal class WeaponFamiliarityComponent : UnitFactComponentDelegate
        {
            public BlueprintRaceReference? Race;

            private BlueprintFeatureReference MartialWeaponProficiency =>
                BlueprintsDb.Owlcat.BlueprintFeature.MartialWeaponProficiency
                    .ToReference<BlueprintFeature, BlueprintFeatureReference>();

            private IEnumerable<WeaponCategory> WeaponCategories
            {
                get
                {
                    if (Race is null) return Enumerable.Empty<WeaponCategory>();

                    return MartialWeaponProficiency.Get().GetComponents<AddProficiencies>()
                        .Where(c => c.RaceRestriction is not null && c.RaceRestriction.RaceId == Race.Get().RaceId)
                        .SelectMany(c => c.WeaponProficiencies);
                }
            }

            public override void OnTurnOn()
            {
                if (!Owner.HasFact(MartialWeaponProficiency.Get())) return;

                foreach (var w in WeaponCategories)
                    Owner.Proficiencies.Add(w);
            }

            public override void OnTurnOff()
            {
                foreach (var w in WeaponCategories)
                {
                    if (Owner.Proficiencies.Contains(w))
                        Owner.Proficiencies.Remove(w);
                }
            }

            public override void OnActivate() => OnTurnOn();

            public override void OnDeactivate()
            {
                if (!Owner.HasFact(MartialWeaponProficiency.Get())) return;
                
                OnTurnOff();
            }
        }

        internal static class Gnome
        {
            [LocalizedString]
            public static readonly string DisplayName = "Gnomish Weapon Familiarity";
            [LocalizedString]
            public static readonly string Description =
                "Gnomes treat any weapon with the word \"Gnome\" in its name as a " +
                $"{new Link(Page.Weapon_Proficiency, "martial weapon")}.";
        }

        internal static class Halfling
        {
            [LocalizedString]
            public static readonly string DisplayName = "Halfling Weapon Familiarity";
            [LocalizedString]
            public static readonly string Description =
                "Halflings treat sling staffs and any weapon with the word \"Halfling\" in its name as a " +
                $"{new Link(Page.Weapon_Proficiency, "martial weapon")}.";
        }

        internal static BlueprintInitializationContext.ContextInitializer<(BlueprintFeature, BlueprintFeature)>
            Create(BlueprintInitializationContext context)
        {


            var gnome = BlueprintsDb.Owlcat.BlueprintRace.GnomeRace;
            var halfling = BlueprintsDb.Owlcat.BlueprintRace.HalflingRace;

            var gnomeWF = context.NewBlueprint<BlueprintFeature>(GeneratedGuid.GnomishWeaponFamiliarity, nameof(GeneratedGuid.GnomishWeaponFamiliarity))
                .Map(blueprint =>
                {
                    blueprint.m_DisplayName = LocalizedStrings.Features_RacialWeaponFamiliarities_Gnome_DisplayName;
                    blueprint.m_Description = LocalizedStrings.Features_RacialWeaponFamiliarities_Gnome_Description;

                    blueprint.SetIcon("ac128c37256e37d408b7149b3edeaa8f", 21300000);

                    return blueprint;
                });

            var halflingWF = context.NewBlueprint<BlueprintFeature>(GeneratedGuid.HalflingWeaponFamiliarity, nameof(GeneratedGuid.HalflingWeaponFamiliarity))
                .Map(blueprint =>
                {
                    blueprint.m_DisplayName = LocalizedStrings.Features_RacialWeaponFamiliarities_Halfling_DisplayName;
                    blueprint.m_Description = LocalizedStrings.Features_RacialWeaponFamiliarities_Halfling_Description;

                    blueprint.SetIcon("2fd0a6cb0f7152941a036ea43f0361cb", 21300000);

                    return blueprint;
                });

            return gnomeWF
                .Combine(halflingWF)
                .Map(bps =>
                {
                    var (gwf, hwf) = bps;

                    var weaponFamiliarity = new[]
                    {
                        (BlueprintsDb.Owlcat.BlueprintRace.ElfRace, BlueprintsDb.Owlcat.BlueprintFeature.ElvenWeaponFamiliarity),
                        (BlueprintsDb.Owlcat.BlueprintRace.HalfOrcRace, BlueprintsDb.Owlcat.BlueprintFeature.OrcWeaponFamiliarity),
                        (BlueprintsDb.Owlcat.BlueprintRace.DwarfRace, BlueprintsDb.Owlcat.BlueprintFeature.DwarvenWeaponFamiliarity)
                    }
                    .Select(wf => (wf.Item1, wf.Item2.GetBlueprint()!))
                    .ToList();

                    weaponFamiliarity.Add((BlueprintsDb.Owlcat.BlueprintRace.GnomeRace, gwf));
                    weaponFamiliarity.Add((BlueprintsDb.Owlcat.BlueprintRace.HalflingRace, hwf));

                    foreach (var (race, feature) in weaponFamiliarity)
                    {
                        feature.AddComponent<WeaponFamiliarityComponent>(c =>
                            c.Race = race.ToReference<BlueprintRace, BlueprintRaceReference>());
                    }

                    return bps;
                });
        }
        

            [Init]
        internal static void Init()
        {
            var initContext = new BlueprintInitializationContext(Triggers.BlueprintsCache_Init);

            Create(initContext).Register();
        }
    }
}
