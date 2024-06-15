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
//using MicroWrath.BlueprintInitializationContext;
using MicroWrath.BlueprintsDb;
using MicroWrath.Components;
using MicroWrath.Extensions;
using MicroWrath.Extensions.Components;
using MicroWrath.InitContext;
using MicroWrath.Localization;

using static MicroWrath.Encyclopedia;

namespace AlternateRacialTraits.Features.Human
{
    internal static partial class HumanBonusFeat
    {
        [LocalizedString]
        public static readonly string DisplayName = "Bonus Feat";

        [LocalizedString]
        public static readonly string Description = $"Humans select one extra {new Link(Page.Feat, "feat")} at 1st level.";

        internal static IInitContext<BlueprintFeature> Create() =>
            InitContext.NewBlueprint<BlueprintFeature>(GeneratedGuid.HumanBonusFeat)
                .Combine(BlueprintsDb.Owlcat.BlueprintFeatureSelection.BasicFeatSelection)
                .Map(bps =>
                {
                    var (blueprint, bfs) = bps;

                    blueprint.m_DisplayName = Localized.DisplayName;
                    blueprint.m_Description = Localized.Description;

                    blueprint.Groups = [FeatureGroup.Racial];

                    //blueprint.HideInCharacterSheetAndLevelUp = true;

                    blueprint.m_Icon = bfs.Icon;

                    blueprint.AddComponent(new UnitFactActivateEvent(e =>
                    {
                        Util.AddLevelUpSelection(
                            e.Owner,
                            [
                                BlueprintsDb.Owlcat.BlueprintFeatureSelection.BasicFeatSelection.ToReference<BlueprintFeatureBase,
                                BlueprintFeatureBaseReference>()
                            ],
                            e.Owner.Progression.Race);
                    }));

                    return blueprint;
                })
                .AddBlueprintDeferred(GeneratedGuid.HumanBonusFeat);
                //.AddOnTrigger(GeneratedGuid.HumanBonusFeat, Triggers.BlueprintsCache_Init);
    }
}
