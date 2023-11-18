using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Facts;

using MicroWrath;
using MicroWrath.BlueprintInitializationContext;
using MicroWrath.BlueprintsDb;
using MicroWrath.Extensions;
using MicroWrath.Extensions.Components;
using MicroWrath.Localization;
using MicroWrath.Util;

namespace AlternateRacialTraits.Features.HalfElf
{
    internal static class WeaponFamiliarity
    {
        [LocalizedString]
        internal const string DisplayName = "Weapon Familiarity";

        [LocalizedString]
        internal const string Description = "Half-elves raised among elves often feel pitied and mistrusted by " +
            "their longer-lived kin, and yet they receive training in elf weapons. They gain the elf ‘s weapon " +
            "familiarity trait. This racial trait replaces adaptability.";

        [Init]
        internal static void Init()
        {
            var initContext = new BlueprintInitializationContext(Triggers.BlueprintsCache_Init);

            initContext.NewBlueprint<BlueprintFeature>(GeneratedGuid.Get("HalfElfWeaponFamiliarity"), nameof(GeneratedGuid.HalfElfWeaponFamiliarity))
                .GetBlueprint(BlueprintsDb.Owlcat.BlueprintFeatureSelection.HalfElfHeritageSelection)
                .GetBlueprint(BlueprintsDb.Owlcat.BlueprintFeature.ElvenWeaponFamiliarity)
                .Map(bps =>
                {
                    var (blueprint, halfElfHeritageSelection, elvenWeaponFamiliarity) = bps.Expand();

                    blueprint.m_DisplayName = LocalizedStrings.Features_HalfElf_WeaponFamiliarity_DisplayName;
                    blueprint.m_Description = LocalizedStrings.Features_HalfElf_WeaponFamiliarity_Description;

                    blueprint.Groups = new[] { FeatureGroup.Racial };

                    blueprint.m_Icon = elvenWeaponFamiliarity.Icon;

                    blueprint.AddAddFacts(c => c.m_Facts =
                        new[] { elvenWeaponFamiliarity.ToReference<BlueprintUnitFactReference>() });

                    // TODO: rework to allow for multiple selections (if TTT doesn't already)
                    //blueprint.AddPrerequisiteFeature(BlueprintsDb.Owlcat.BlueprintFeatureSelection.Adaptability, false, true)
                    //    .Group = Prerequisite.GroupType.Any;

                    //blueprint.AddPrerequisiteFeature(BlueprintsDb.Owlcat.BlueprintFeatureSelection.BaseRaseHalfElfSelection, false, true)
                    //    .Group = Prerequisite.GroupType.Any;

                    blueprint.AddComponent<PrerequisiteNoFeature>(
                        c =>
                        {
                            c.m_Feature = BlueprintsDb.Owlcat.BlueprintFeatureSelection.Adaptability
                                .ToReference<BlueprintFeature, BlueprintFeatureReference>();

                            c.Group = Prerequisite.GroupType.Any;
                        });

                    //BlueprintsDb.Owlcat.BlueprintFeatureSelection.HalfElfHeritageSelection.GetBlueprint()!.AddFeatures(blueprint.ToMicroBlueprint());

                })
                .Register();
        }
    }
}
