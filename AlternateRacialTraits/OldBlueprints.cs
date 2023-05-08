using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Kingmaker.Blueprints.Classes;

using MicroWrath;
using MicroWrath.BlueprintInitializationContext;

namespace AlternateRacialTraits
{
    internal static class OldBlueprints
    {
        [Init]
        internal static void Init()
        {
            var initContext = new BlueprintInitializationContext(Triggers.BlueprintsCache_Init);

            initContext.NewBlueprint<BlueprintFeature>(GeneratedGuid.Removed1, nameof(GeneratedGuid.Removed1))
                .Combine(initContext.NewBlueprint<BlueprintFeature>(GeneratedGuid.Removed2, nameof(GeneratedGuid.Removed2)))
                .Register();
        }
    }
}
