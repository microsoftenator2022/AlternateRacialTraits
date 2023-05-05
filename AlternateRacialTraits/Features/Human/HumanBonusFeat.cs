using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Classes.Selection;

using MicroWrath;
using MicroWrath.BlueprintInitializationContext;
using MicroWrath.BlueprintsDb;
using MicroWrath.Extensions;
using MicroWrath.Extensions.Components;
using MicroWrath.Localization;

using static MicroWrath.Encyclopedia;

namespace AlternateRacialTraits.Features.Human
{
    internal static class HumanBonusFeat
    {
        [LocalizedString]
        public static readonly string DisplayName = "Bonus Feat";

        [LocalizedString]
        public static readonly string Description = $"Humans select one extra {new Link(Page.Feat, "feat")} at 1st level.";

        internal static BlueprintInitializationContext.ContextInitializer<BlueprintFeatureSelection> Create(BlueprintInitializationContext initContext) =>
            initContext.NewBlueprint<BlueprintFeatureSelection>(GeneratedGuid.HumanBonusFeat, nameof(HumanBonusFeat))
                .Map((BlueprintFeatureSelection blueprint) =>
                {
                    blueprint.m_DisplayName = LocalizedStrings.Features_Human_HumanBonusFeat_DisplayName;
                    blueprint.m_Description = LocalizedStrings.Features_Human_HumanBonusFeat_Description;

                    blueprint.Group = FeatureGroup.Feat;
                    blueprint.Groups = new[] { FeatureGroup.Racial };

                    blueprint.m_Icon = BlueprintsDb.Owlcat.BlueprintFeatureSelection.BasicFeatSelection.GetBlueprint()?.Icon;

                    blueprint.AddFeatures(BlueprintsDb.Owlcat.BlueprintFeatureSelection.BasicFeatSelection);

                    return blueprint;
                });
    }

}
