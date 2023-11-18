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
using MicroWrath.Components;
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

        internal static BlueprintInitializationContext.ContextInitializer<BlueprintFeature> Create(BlueprintInitializationContext initContext) =>
            initContext.NewBlueprint<BlueprintFeature>(GeneratedGuid.HumanBonusFeat, nameof(HumanBonusFeat))
                .GetBlueprint(BlueprintsDb.Owlcat.BlueprintFeatureSelection.BasicFeatSelection)
                .Map(bps =>
                {
                    var (blueprint, bfs) = bps;

                    blueprint.m_DisplayName = LocalizedStrings.Features_Human_HumanBonusFeat_DisplayName;
                    blueprint.m_Description = LocalizedStrings.Features_Human_HumanBonusFeat_Description;

                    blueprint.Groups = new[] { FeatureGroup.Racial };

                    //blueprint.HideInCharacterSheetAndLevelUp = true;

                    blueprint.m_Icon = bfs.Icon;

                    blueprint.AddComponent(new UnitFactActivateEvent(e =>
                    {
                        Util.AddLevelUpSelection(
                            e.Owner,
                            new[] { BlueprintsDb.Owlcat.BlueprintFeatureSelection
                                .BasicFeatSelection.ToReference<BlueprintFeatureBase, BlueprintFeatureBaseReference>() },
                            e.Owner.Progression.Race);
                    }));

                    return blueprint;
                });
    }

}
