using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Kingmaker.EntitySystem.Stats;

using MicroWrath;
using MicroWrath.BlueprintsDb;
using MicroWrath.Components;
using MicroWrath.Extensions;
using MicroWrath.InitContext;

namespace AlternateRacialTraits.Features.HalfElf;

internal static class KindredRaisedFix
{
    [Init]
    static void Init()
    {
        _ = InitContext.GetBlueprint(BlueprintsDb.Owlcat.BlueprintFeature.KindredRaisedHalfElf)
            .Map(blueprint =>
            {
                _ = blueprint.AddComponent<PrerequisiteNoRaceStatBonus>(c => c.Stat = StatType.Charisma);

                return blueprint;
            })
            .AddOnTrigger(BlueprintsDb.Owlcat.BlueprintFeature.KindredRaisedHalfElf.BlueprintGuid, Triggers.BlueprintsCache_Init);
    }
}
