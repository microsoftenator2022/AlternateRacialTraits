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
//using MicroWrath.BlueprintInitializationContext;
using MicroWrath.BlueprintsDb;
using MicroWrath.Extensions;
using MicroWrath.Extensions.Components;
using MicroWrath.InitContext;
using MicroWrath.Localization;
using MicroWrath.Util;

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
        
        internal static IEnumerable<(IInitContext<BlueprintFeature>, BlueprintGuid)> Create()
        {
            //var gnome = BlueprintsDb.Owlcat.BlueprintRace.GnomeRace;
            //var halfling = BlueprintsDb.Owlcat.BlueprintRace.HalflingRace;

            var gnomeWF = InitContext.NewBlueprint<BlueprintFeature>(GeneratedGuid.GnomishWeaponFamiliarity)
                .Map(blueprint =>
                {
                    blueprint.m_DisplayName = LocalizedStrings.Features_RacialWeaponFamiliarities_Gnome_DisplayName;
                    blueprint.m_Description = LocalizedStrings.Features_RacialWeaponFamiliarities_Gnome_Description;

                    blueprint.SetIcon("ac128c37256e37d408b7149b3edeaa8f", 21300000);

                    return blueprint;
                })
                .RegisterBlueprint(GeneratedGuid.GnomishWeaponFamiliarity, Triggers.BlueprintsCache_Init);

            var halflingWF = InitContext.NewBlueprint<BlueprintFeature>(GeneratedGuid.HalflingWeaponFamiliarity)
                .Map(blueprint =>
                {
                    blueprint.m_DisplayName = LocalizedStrings.Features_RacialWeaponFamiliarities_Halfling_DisplayName;
                    blueprint.m_Description = LocalizedStrings.Features_RacialWeaponFamiliarities_Halfling_Description;

                    blueprint.SetIcon("2fd0a6cb0f7152941a036ea43f0361cb", 21300000);

                    return blueprint;
                })
                .RegisterBlueprint(GeneratedGuid.HalflingWeaponFamiliarity, Triggers.BlueprintsCache_Init);

            return
                new (OwlcatBlueprint<BlueprintRace> race, OwlcatBlueprint<BlueprintFeature> wf)[]
                {
                    (BlueprintsDb.Owlcat.BlueprintRace.ElfRace, BlueprintsDb.Owlcat.BlueprintFeature.ElvenWeaponFamiliarity),
                    (BlueprintsDb.Owlcat.BlueprintRace.HalfOrcRace, BlueprintsDb.Owlcat.BlueprintFeature.OrcWeaponFamiliarity),
                    (BlueprintsDb.Owlcat.BlueprintRace.DwarfRace, BlueprintsDb.Owlcat.BlueprintFeature.DwarvenWeaponFamiliarity)
                }
                .Select(pair => (InitContext.GetBlueprint(pair.race).Combine(pair.wf), pair.wf.BlueprintGuid))
                .Append((InitContext.GetBlueprint(BlueprintsDb.Owlcat.BlueprintRace.GnomeRace).Combine(gnomeWF), GeneratedGuid.GnomishWeaponFamiliarity))
                .Append((InitContext.GetBlueprint(BlueprintsDb.Owlcat.BlueprintRace.HalflingRace).Combine(halflingWF), GeneratedGuid.HalfElfWeaponFamiliarity))
                .Select(pair =>
                {
                    var (context, guid) = pair;

                    return (
                        context.Map(pair =>
                        {
                            var (race, wf) = pair;

                            wf.AddComponent<WeaponFamiliarityComponent>(c => c.Race = race.ToReference());

                            return wf;
                        }),
                        guid);
                });

            //InitContext.GetBlueprint(BlueprintsDb.Owlcat.BlueprintRace.ElfRace)
            //    .Combine(BlueprintsDb.Owlcat.BlueprintFeature.ElvenWeaponFamiliarity)
            //    .Combine(BlueprintsDb.Owlcat.BlueprintRace.HalfOrcRace)
            //    .Combine(BlueprintsDb.Owlcat.BlueprintFeature.OrcWeaponFamiliarity)
            //    .Combine(BlueprintsDb.Owlcat.BlueprintRace.DwarfRace)
            //    .Combine(BlueprintsDb.Owlcat.BlueprintFeature.DwarvenWeaponFamiliarity)
            //    .Map(bps =>
            //    {
            //        var (elf, elfWf, halfOrc, orcWf, dwarf, dwarfWf) = bps.Expand();

            //        return new[] {
            //            (elf, elfWf),
            //            (halfOrc, orcWf),
            //            (dwarf, dwarfWf)
            //        };
            //    });

            //gnomeWF
            //    .Combine(halflingWF)
            //    .Combine(wfs)
            //    .Combine(BlueprintsDb.Owlcat.BlueprintRace.GnomeRace)
            //    .Combine(BlueprintsDb.Owlcat.BlueprintRace.HalflingRace)
            //    .Map(bps =>
            //    {
            //        var (gwf, hwf, wfs, gnomeRace, halflingRace) = bps.Expand();

            //        var weaponFamiliarity = wfs.ToList();

            //        weaponFamiliarity.Add((gnomeRace, gwf));
            //        weaponFamiliarity.Add((halflingRace, hwf));

            //        foreach (var (race, feature) in weaponFamiliarity)
            //        {
            //            feature.AddComponent<WeaponFamiliarityComponent>(c => c.Race = race.ToReference());
            //        }

            //        return (gwf, hwf);
            //    });
        }
        
        [Init]
        internal static void Init()
        {
            foreach (var (context, guid) in Create())
            {
                context.RegisterBlueprint(guid, Triggers.BlueprintsCache_Init);
            }

            //var initContext = new BlueprintInitializationContext(Triggers.BlueprintsCache_Init);

            //Create(initContext).Register();
        }
    }
}
