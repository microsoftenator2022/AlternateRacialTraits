using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Kingmaker.Blueprints.Classes;

using MicroWrath;
//using MicroWrath.BlueprintInitializationContext;
using MicroWrath.Extensions;
using MicroWrath.Extensions.Components;
using MicroWrath.InitContext;
using MicroWrath.Localization;

using static MicroWrath.Encyclopedia;

namespace AlternateRacialTraits.Features.Human
{
    internal static class UnstoppableMagic
    {
        [LocalizedString]
        public static readonly string DisplayName = "Unstoppable Magic";
        [LocalizedString]
        public static readonly string Description =
            "Humans from civilizations built upon advanced magic are educated in a variety of ways to " +
            $"accomplish their magical goals. They gain a +2 racial {new Link(Page.Bonus, "bonus")} " +
            $"on {new Link(Page.Caster_Level, "caster level")} {new Link(Page.Check, "checks")} " +
            $"against {new Link(Page.Spell_Resistance, "spell resistance")}. This racial trait " +
            $"replaces the bonus feat trait.";

        internal static IInitContext<BlueprintFeature> Create() =>
            InitContext.NewBlueprint<BlueprintFeature>(GeneratedGuid.UnstoppableMagic)
                .Map((BlueprintFeature blueprint) =>
                {
                    blueprint.m_DisplayName = LocalizedStrings.Features_Human_UnstoppableMagic_DisplayName;
                    blueprint.m_Description = LocalizedStrings.Features_Human_UnstoppableMagic_Description;

                    blueprint.Groups = new[] { FeatureGroup.Racial };
                    
                    blueprint.AddSpellPenetrationBonus(c => c.Value = 2);

                    blueprint.SetIcon("15d11c952fdc96b45849f312f3931192", 21300000);

                    return blueprint;
                })
                .AddOnTrigger(GeneratedGuid.UnstoppableMagic, Triggers.BlueprintsCache_Init);
    }
}
