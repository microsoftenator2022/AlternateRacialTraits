using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Facts;
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

namespace AlternateRacialTraits.Features.Human
{
    internal static partial class AdoptiveParentage
    {
        [LocalizedString]
        public static readonly string DisplayName = "Adoptive Parentage";

        [LocalizedString]
        public static readonly string Description =
            "Humans are sometimes orphaned and adopted by other races. Choose one humanoid " +
            $"{new Link(Page.Race, "race")} without the human subtype. You gain that race’s " +
            $"weapon familiarity racial {new Link(Page.Trait, "trait")}. If the race does not have weapon " +
            $"familiarity, you gain Skill Focus or Weapon Focus as a bonus {new Link(Page.Feat, "feat")} that " +
            $"is appropriate for that race instead. This racial trait replaces the bonus {new Link(Page.Feat, "feat")} trait.";

        private record class AdoptingRace(
            GeneratedGuid Guid,
            string BlueprintName,
            IMicroBlueprint<BlueprintRace> Race,
            IEnumerable<IMicroBlueprint<BlueprintFeature>> Features);

        private static readonly AdoptingRace[] AdoptingRaces = new AdoptingRace[]
        {
            new(GeneratedGuid.AdoptiveParentageDwarf,
                nameof(GeneratedGuid.AdoptiveParentageDwarf),
                BlueprintsDb.Owlcat.BlueprintRace.DwarfRace,
                new IMicroBlueprint<BlueprintFeature>[] { BlueprintsDb.Owlcat.BlueprintFeature.DwarvenWeaponFamiliarity }),

            new(GeneratedGuid.AdoptiveParentageElf,
                nameof(GeneratedGuid.AdoptiveParentageElf),
                BlueprintsDb.Owlcat.BlueprintRace.ElfRace,
                new IMicroBlueprint<BlueprintFeature>[] { BlueprintsDb.Owlcat.BlueprintFeature.ElvenWeaponFamiliarity }),

            new(GeneratedGuid.AdoptiveParentageGnome,
                nameof(GeneratedGuid.AdoptiveParentageGnome),
                BlueprintsDb.Owlcat.BlueprintRace.GnomeRace,
                new IMicroBlueprint<BlueprintFeature>[] { new MicroBlueprint<BlueprintFeature>(GeneratedGuid.GnomishWeaponFamiliarity) }),

            new(GeneratedGuid.AdoptiveParentageHalfling,
                nameof(GeneratedGuid.AdoptiveParentageHalfling),
                BlueprintsDb.Owlcat.BlueprintRace.HalflingRace,
                new IMicroBlueprint<BlueprintFeature>[] { new MicroBlueprint<BlueprintFeature>(GeneratedGuid.HalflingWeaponFamiliarity) }),

            new(GeneratedGuid.AdoptiveParentageOrc,
                nameof(GeneratedGuid.AdoptiveParentageOrc),
                BlueprintsDb.Owlcat.BlueprintRace.HalfOrcRace,
                new IMicroBlueprint<BlueprintFeature>[] { BlueprintsDb.Owlcat.BlueprintFeature.OrcWeaponFamiliarity }),

            new(GeneratedGuid.AdoptiveParentageAasimar,
                nameof(GeneratedGuid.AdoptiveParentageAasimar),
                BlueprintsDb.Owlcat.BlueprintRace.AasimarRace,
                new IMicroBlueprint<BlueprintFeature>[] { BlueprintsDb.Owlcat.BlueprintFeature.SkillFocusDiplomacy,
                    BlueprintsDb.Owlcat.BlueprintFeature.SkillFocusPerception }),

            new(GeneratedGuid.AdoptiveParentageDhampir,
                nameof(GeneratedGuid.AdoptiveParentageDhampir),
                BlueprintsDb.Owlcat.BlueprintRace.DhampirRace,
                new IMicroBlueprint<BlueprintFeature>[] { BlueprintsDb.Owlcat.BlueprintFeature.SkillFocusDiplomacy,
                    BlueprintsDb.Owlcat.BlueprintFeature.SkillFocusPerception }),

            new(GeneratedGuid.AdoptiveParentageKitsune,
                nameof(GeneratedGuid.AdoptiveParentageKitsune),
                BlueprintsDb.Owlcat.BlueprintRace.KitsuneRace,
                new IMicroBlueprint<BlueprintFeature>[] { BlueprintsDb.Owlcat.BlueprintFeature.WeaponFocusBite,
                    BlueprintsDb.Owlcat.BlueprintFeature.SkillFocusAcrobatics }),

            new(GeneratedGuid.AdoptiveParentageOread,
                nameof(GeneratedGuid.AdoptiveParentageOread),
                BlueprintsDb.Owlcat.BlueprintRace.OreadRace,
                new IMicroBlueprint<BlueprintFeature>[] { BlueprintsDb.Owlcat.BlueprintFeature.WeaponFocusLongbow,
                    BlueprintsDb.Owlcat.BlueprintFeature.SkillFocusDiplomacy }),

            new(GeneratedGuid.AdoptiveParentageTiefling,
                nameof(GeneratedGuid.AdoptiveParentageTiefling),
                BlueprintsDb.Owlcat.BlueprintRace.TieflingRace,
                new IMicroBlueprint<BlueprintFeature>[] { BlueprintsDb.Owlcat.BlueprintFeature.SkillFocusDiplomacy,
                    BlueprintsDb.Owlcat.BlueprintFeature.SkillFocusStealth })
        };

        internal static IInitContext<BlueprintFeatureSelection> Create()
        {
            var apFeatures = AdoptingRaces
                .Select(ar =>
                {
                    if (ar.Features.Count() == 1)
                    {
                        return (ar, InitContext.NewBlueprint<BlueprintFeature>(ar.Guid, ar.BlueprintName)
                            .Map((BlueprintFeature bp) =>
                            {
                                bp.AddComponent<AddFacts>(c =>
                                    c.m_Facts = [ar.Features.First().ToReference<BlueprintUnitFact, BlueprintUnitFactReference>()]);

                                return bp;
                            }));
                    }
                    else
                    {
                        return (ar, InitContext.NewBlueprint<BlueprintFeatureSelection>(ar.Guid, ar.BlueprintName)
                            .Map((BlueprintFeatureSelection selection) =>
                            {
                                selection.AddFeatures(ar.Features);

                                return selection as BlueprintFeature;
                            }));
                    }
                })
                .Select(pair =>
                {
                    var (ar, context) = pair;
                    
                    return context
                        .Map(bp =>
                        {
                            bp.m_DisplayName = ar.Race.GetBlueprint()!.m_DisplayName;
                            bp.m_Description = Localized.Description;

                            bp.m_Icon = ar.Features.FirstOrDefault()?.GetBlueprint()?.Icon;

                            bp.Groups = [FeatureGroup.Racial];

                            return bp;
                        })
                        .AddBlueprintDeferred(ar.Guid);
                });

            var selection = InitContext.NewBlueprint<BlueprintFeatureSelection>(
                GeneratedGuid.AdoptiveParentageSelection,
                nameof(AdoptiveParentage))
                .Map(selection =>
                {
                    selection.m_DisplayName = Localized.DisplayName;
                    selection.m_Description = Localized.Description;

                    selection.Groups = [FeatureGroup.Racial];

                    return selection;
                })
                .Combine(apFeatures.Collect())
                .Map(bps =>
                {
                    var (selection, apFeatures) = bps;

                    selection.AddFeatures(apFeatures.Select(MicroBlueprint.ToMicroBlueprint));

                    return selection;
                })
                .AddBlueprintDeferred(GeneratedGuid.AdoptiveParentageSelection);

            return selection;
        }
    }
}
