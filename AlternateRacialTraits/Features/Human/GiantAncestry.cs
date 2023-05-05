using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Kingmaker.Blueprints.Classes;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.UnitLogic.FactLogic;

using MicroWrath;
using MicroWrath.BlueprintInitializationContext;
using MicroWrath.BlueprintsDb;
using MicroWrath.Extensions;
using MicroWrath.Extensions.Components;
using MicroWrath.Localization;

using static MicroWrath.Encyclopedia;

namespace AlternateRacialTraits.Features.Human
{
    internal static class GiantAncestry
    {
        [LocalizedString]
        public static readonly string DisplayName = "Giant Ancestry";

        [LocalizedString]
        public static readonly string Description =
            "Humans with ogre or troll ancestry end up having hulking builds and asymmetrical features. Such " +
            $"humans gain a +1 {new Link(Page.Bonus, "bonus")} on " +
            $"{new Link(Page.Combat_Maneuvers, "combat maneuver")} checks and to " +
            $"{new Link(Page.CMD, "CMD")}, but a –2 {new Link(Page.Penalty, "penalty")} on " +
            "Stealth checks. This racial trait replaces Skilled.";

        internal static BlueprintInitializationContext.ContextInitializer<BlueprintFeature> Create(BlueprintInitializationContext initContext) =>
            initContext.NewBlueprint<BlueprintFeature>(GeneratedGuid.GiantAncestry, nameof(GiantAncestry))
                .Map((BlueprintFeature blueprint) =>
                {
                    blueprint.m_DisplayName = LocalizedStrings.Features_Human_GiantAncestry_DisplayName;
                    blueprint.m_Description = LocalizedStrings.Features_Human_GiantAncestry_Description;

                    blueprint.SetIcon("bd9b4ce9708652f44a072434b4560aca", 21300000);

                    blueprint.Groups = new[] { FeatureGroup.Racial };

                    blueprint.AddCMBBonus(c =>
                    {
                        c.Value = 1;
                        c.Descriptor = ModifierDescriptor.Racial;
                    });

                    blueprint.AddCMDBonus(c =>
                    {
                        c.Value = 1;
                        c.Descriptor = ModifierDescriptor.Racial;
                    });

                    blueprint.AddAddStatBonus(c =>
                    {
                        c.Value = -2;
                        c.Stat = StatType.SkillStealth;
                        c.Descriptor = ModifierDescriptor.Racial;
                    });

                    blueprint.AddPrerequisiteFeature(BlueprintsDb.Owlcat.BlueprintFeature.HumanSkilled, false, true);

                    return blueprint;
                });
    }
}
