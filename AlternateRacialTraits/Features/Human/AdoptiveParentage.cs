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
using MicroWrath.BlueprintInitializationContext;
using MicroWrath.BlueprintsDb;
using MicroWrath.Extensions;
using MicroWrath.Extensions.Components;
using MicroWrath.Localization;
using MicroWrath.Util;

using static MicroWrath.Encyclopedia;

namespace AlternateRacialTraits.Features.Human
{
    internal static class AdoptiveParentage
    {
        [LocalizedString]
        public static readonly string DisplayName = "Adoptive Parentage";

        [LocalizedString]
        public static readonly string Description =
            "Humans are sometimes orphaned and adopted by other races. Choose one humanoid " +
            $"{new Link(Page.Race, "race")} without the human subtype. You gain that race’s " +
            "weapon familiarity racial trait. If the race does not have weapon familiarity, you gain " +
            $"Skill Focus or Weapon Focus as a bonus {new Link(Page.Feat, "feat")} that " +
            "is appropriate for that race instead. This racial trait replaces the bonus feat trait.";

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
                new[] { BlueprintsDb.Owlcat.BlueprintFeature.DwarvenWeaponFamiliarity }),

            new(GeneratedGuid.AdoptiveParentageElf,
                nameof(GeneratedGuid.AdoptiveParentageElf),
                BlueprintsDb.Owlcat.BlueprintRace.ElfRace,
                new[] { BlueprintsDb.Owlcat.BlueprintFeature.ElvenWeaponFamiliarity }),

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
                new[] { BlueprintsDb.Owlcat.BlueprintFeature.OrcWeaponFamiliarity }),

            new(GeneratedGuid.AdoptiveParentageAasimar,
                nameof(GeneratedGuid.AdoptiveParentageAasimar),
                BlueprintsDb.Owlcat.BlueprintRace.AasimarRace,
                new[] { BlueprintsDb.Owlcat.BlueprintFeature.SkillFocusDiplomacy,
                    BlueprintsDb.Owlcat.BlueprintFeature.SkillFocusPerception }),

            new(GeneratedGuid.AdoptiveParentageDhampir,
                nameof(GeneratedGuid.AdoptiveParentageDhampir),
                BlueprintsDb.Owlcat.BlueprintRace.DhampirRace,
                new[] { BlueprintsDb.Owlcat.BlueprintFeature.SkillFocusDiplomacy,
                    BlueprintsDb.Owlcat.BlueprintFeature.SkillFocusPerception }),

            new(GeneratedGuid.AdoptiveParentageKitsune,
                nameof(GeneratedGuid.AdoptiveParentageKitsune),
                BlueprintsDb.Owlcat.BlueprintRace.KitsuneRace,
                new[] { BlueprintsDb.Owlcat.BlueprintFeature.WeaponFocusBite,
                    BlueprintsDb.Owlcat.BlueprintFeature.SkillFocusAcrobatics }),

            new(GeneratedGuid.AdoptiveParentageOread,
                nameof(GeneratedGuid.AdoptiveParentageOread),
                BlueprintsDb.Owlcat.BlueprintRace.OreadRace,
                new[] { BlueprintsDb.Owlcat.BlueprintFeature.WeaponFocusLongbow,
                    BlueprintsDb.Owlcat.BlueprintFeature.SkillFocusDiplomacy}),

            new(GeneratedGuid.AdoptiveParentageTiefling,
                nameof(GeneratedGuid.AdoptiveParentageTiefling),
                BlueprintsDb.Owlcat.BlueprintRace.TieflingRace,
                new[] { BlueprintsDb.Owlcat.BlueprintFeature.SkillFocusDiplomacy,
                    BlueprintsDb.Owlcat.BlueprintFeature.SkillFocusStealth })
        };

        internal static BlueprintInitializationContext.ContextInitializer<BlueprintFeatureSelection> Create(BlueprintInitializationContext context)
        {
            var apFeatures = AdoptingRaces
                .Select(ar =>
                {
                    if (ar.Features.Count() == 1)
                    {
                        return context.NewBlueprint<BlueprintFeature>(ar.Guid, ar.BlueprintName)
                            .Map((BlueprintFeature bp) =>
                            {
                                bp.AddComponent<AddFacts>(c =>
                                    c.m_Facts = new[] { ar.Features.First().ToReference<BlueprintUnitFact, BlueprintUnitFactReference>() });

                                return (ar, bp);
                            });
                    }
                    else
                    {
                        return context.NewBlueprint<BlueprintFeatureSelection>(ar.Guid, ar.BlueprintName)
                            .Map((BlueprintFeatureSelection selection) =>
                            {
                                selection.AddFeatures(ar.Features);

                                return (ar, selection as BlueprintFeature);
                            });
                    }
                })
                .Combine()
                .Map(bps =>
                {
                    foreach (var (ar, bp) in bps)
                    {
                        bp.m_DisplayName = ar.Race.GetBlueprint()!.m_DisplayName;
                        bp.m_Description = LocalizedStrings.Features_Human_AdoptiveParentage_Description;

                        bp.m_Icon = ar.Features.FirstOrDefault()?.GetBlueprint()?.Icon;

                        bp.Groups = new[] { FeatureGroup.Racial };
                    }

                    return bps.Select(x => x.Item2);
                });

            var selection = context.NewBlueprint<BlueprintFeatureSelection>(
                GeneratedGuid.AdoptiveParentageSelection, nameof(AdoptiveParentage))
                .Map(selection =>
                {
                    selection.m_DisplayName = LocalizedStrings.Features_Human_AdoptiveParentage_DisplayName;
                    selection.m_Description = LocalizedStrings.Features_Human_AdoptiveParentage_Description;

                    selection.Groups = new[] { FeatureGroup.Racial };

                    return selection;
                })
                .Combine(apFeatures)
                .Map(bps =>
                {
                    var (selection, apFeatures) = bps;

                    selection.AddFeatures(apFeatures.Select(MicroBlueprint.ToMicroBlueprint));

                    return selection;
                });

            return selection;
        }
    }
}
