using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;

using MicroWrath;

using MicroWrath.InitContext;

using Newtonsoft.Json.Linq;

namespace AlternateRacialTraits;

internal static class OldBlueprints
{
    [Init]
    internal static void Init()
    {
        _ = InitContext.NewBlueprint<BlueprintFeature>(GeneratedGuid.Removed1)
            .AddOnTrigger(GeneratedGuid.Removed1, Triggers.BlueprintsCache_Init);

        _ = InitContext.NewBlueprint<BlueprintFeature>(GeneratedGuid.Removed2)
            .AddOnTrigger(GeneratedGuid.Removed2, Triggers.BlueprintsCache_Init);
        
        _ = InitContext.NewBlueprint<BlueprintFeatureSelection>(GeneratedGuid.Get("RemovedHumanBonusFeatSelection"))
            .AddOnTrigger(GeneratedGuid.RemovedHumanBonusFeatSelection, Triggers.BlueprintsCache_Init);
    }
}
