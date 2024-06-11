using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Kingmaker.Blueprints.Classes;

using MicroWrath;
//using MicroWrath.BlueprintInitializationContext;
using MicroWrath.InitContext;

namespace AlternateRacialTraits
{
    internal static class OldBlueprints
    {
        [Init]
        internal static void Init()
        {
            //var initContext = new BlueprintInitializationContext(Triggers.BlueprintsCache_Init);

            InitContext.NewBlueprint<BlueprintFeature>(GeneratedGuid.Removed1).RegisterBlueprint(GeneratedGuid.Removed1, Triggers.BlueprintsCache_Init);
            InitContext.NewBlueprint<BlueprintFeature>(GeneratedGuid.Removed2).RegisterBlueprint(GeneratedGuid.Removed2, Triggers.BlueprintsCache_Init);
        }
    }
}
