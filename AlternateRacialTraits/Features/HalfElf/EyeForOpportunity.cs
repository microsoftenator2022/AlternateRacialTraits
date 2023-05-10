using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Enums;

using MicroWrath;
using MicroWrath.BlueprintInitializationContext;
using MicroWrath.BlueprintsDb;
using MicroWrath.Extensions;
using MicroWrath.Extensions.Components;
using MicroWrath.Localization;

using static MicroWrath.Encyclopedia;

namespace AlternateRacialTraits.Features.HalfElf
{
    internal static class EyeForOpportunity
    {
        [LocalizedString]
        public const string DisplayName = "Eye for Opportunity";

        [LocalizedString]
        public static readonly string Description =
            "Constantly facing the rough edges of two societies, some half-elves develop a knack for finding " +
            $"overlooked opportunities. They gain a +1 racial {new Link(Page.Bonus, "bonus")} on " +
            $"{new Link(Page.Attack_Of_Opportunity, "attacks of opportunity")}. This racial " +
            "trait replaces adaptability and keen senses.";

        [Init]
        internal static void Init()
        {
            var initContext = new BlueprintInitializationContext(Triggers.BlueprintsCache_Init);

            initContext.NewBlueprint<BlueprintFeature>(GeneratedGuid.Get("EyeForOpportunity"), nameof(GeneratedGuid.EyeForOpportunity))
                .Map(blueprint =>
                {
                    blueprint.m_DisplayName = LocalizedStrings.Features_HalfElf_EyeForOpportunity_DisplayName;
                    blueprint.m_Description = LocalizedStrings.Features_HalfElf_EyeForOpportunity_Description;

                    blueprint.Groups = new[] { FeatureGroup.Racial };

                    blueprint.SetIcon("f1caa0aa81d2e7d469e00c793f284b07", 21300000);

                    blueprint.AddAttackOfOpportunityAttackBonus(c =>
                    {
                        c.Bonus = 1;

                        c.Descriptor = ModifierDescriptor.Racial;
                    });

                    blueprint.AddPrerequisiteFeature(BlueprintsDb.Owlcat.BlueprintFeature.KeenSenses, hideInUI: false, removeOnApply: true)
                        .Group = Prerequisite.GroupType.All;

                    blueprint.AddComponent<PrerequisiteNoFeature>(c =>
                    {
                        c.m_Feature = BlueprintsDb.Owlcat.BlueprintFeatureSelection.Adaptability
                            .ToReference<BlueprintFeature, BlueprintFeatureReference>();

                        c.Group = Prerequisite.GroupType.All;
                    });

                    BlueprintsDb.Owlcat.BlueprintFeatureSelection.HalfElfHeritageSelection.GetBlueprint()!
                        .AddFeatures(blueprint.ToMicroBlueprint());
                })
                .Register();
        }
    }
}
