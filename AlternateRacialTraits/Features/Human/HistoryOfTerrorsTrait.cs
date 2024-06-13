using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;

using MicroWrath;
//using MicroWrath.BlueprintInitializationContext;
using MicroWrath.BlueprintsDb;
using MicroWrath.Extensions;
using MicroWrath.Extensions.Components;
using MicroWrath.InitContext;

namespace AlternateRacialTraits.Features.Human
{
    internal static class HistoryOfTerrorsTrait
    {
        public static IInitContext<BlueprintFeature> Create() =>
            InitContext.NewBlueprint<BlueprintFeature>(GeneratedGuid.HistoryOfTerrorsTrait)
                .Combine(BlueprintsDb.Owlcat.BlueprintFeature.HistoryOfTerrors)
                .Map(bps =>
                {
                    var (blueprint, feat) = bps;

                    blueprint.m_DisplayName = feat.m_DisplayName;
                    blueprint.m_Description = feat.m_Description;

                    blueprint.m_Icon = feat.Icon;

                    blueprint.Groups = new[] { FeatureGroup.Racial };

                    blueprint.HideInCharacterSheetAndLevelUp = true;
                    
                    blueprint.AddAddFacts(c =>
                    {
                        c.m_Facts = [feat.ToReference<BlueprintUnitFactReference>()];
                    });

                    blueprint.AddPrerequisiteFeature(BlueprintsDb.Owlcat.BlueprintFeature.HumanSkilled, hideInUI: false, removeOnApply: true);

                    return blueprint;
                })
                .AddOnTrigger(GeneratedGuid.HistoryOfTerrorsTrait, Triggers.BlueprintsCache_Init);
    }

}
