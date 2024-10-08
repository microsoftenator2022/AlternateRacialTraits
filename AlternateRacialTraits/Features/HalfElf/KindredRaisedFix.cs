﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Kingmaker.EntitySystem.Stats;

using MicroWrath;
using MicroWrath.BlueprintsDb;
using MicroWrath.Components;
using MicroWrath.Extensions;
using MicroWrath.Deferred;

namespace AlternateRacialTraits.Features.HalfElf;

internal static class KindredRaisedFix
{
    [Init]
    internal static void Init()
    {
        _ = Deferred.GetBlueprint(BlueprintsDb.Owlcat.BlueprintFeature.KindredRaisedHalfElf)
            .Map(blueprint =>
            {
                _ = blueprint.AddComponent<PrerequisiteRacialStatBonus>(c => { c.Stat = StatType.Charisma; c.Not = true; });

                return blueprint;
            })
            .AddOnTrigger(BlueprintsDb.Owlcat.BlueprintFeature.KindredRaisedHalfElf.BlueprintGuid, Triggers.BlueprintsCache_Init);
    }
}
