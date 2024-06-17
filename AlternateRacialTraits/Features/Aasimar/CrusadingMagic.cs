using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Facts;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;

using MicroWrath;
using MicroWrath.Extensions;
using MicroWrath.Extensions.Components;
using MicroWrath.InitContext;
using MicroWrath.Localization;
using MicroWrath.Util;

using static MicroWrath.Encyclopedia;

namespace AlternateRacialTraits.Features.Aasimar
{
    internal static partial class CrusadingMagic
    {
        [LocalizedString]
        internal const string DisplayName = "Crusading Magic";

        [LocalizedString]
        internal static readonly string Description =
            "Many aasimars feel obligated to train to defend the world against fiends. These aasimars gain a " +
            $"+2 racial {new Link(Page.Bonus, "bonus")} on " +
            $"{new Link(Page.Caster_Level, "caster level")} checks to overcome " +
            $"{new Link(Page.Spell_Resistance, "spell resistance")} and on " +
            $"{new Link(Page.Knowledge_Arcana, "Knowledge (Arcana)")} checks. " +
            $"This racial {new Link(Page.Trait, "trait")} replaces the skilled and spell-like ability racial traits.";

        internal static IInitContextBlueprint<BlueprintFeature> Create()
        {
            var feature = InitContext.NewBlueprint<BlueprintFeature>(GeneratedGuid.Get(nameof(CrusadingMagic)))
                .Combine(AasimarFeatureSelection.SLAFeatures.Value)
                .Combine(AasimarFeatureSelection.SkilledFeatures.Value)
                .Map(bps =>
                {
                    var (feature, slaFeatures, skilledFeatures) = bps.Flatten();

                    feature.m_DisplayName = Localized.DisplayName;
                    feature.m_Description = Localized.Description;

                    feature.SetIcon("869e3bc156f9f3646a6a3eff5e0c5c60", 21300000);

                    feature.AddSpellPenetrationBonus(c =>
                    {
                        c.Value = 2;
                        c.Descriptor = ModifierDescriptor.Racial;
                    });

                    feature.AddAddStatBonus(c =>
                    {
                        c.Value = 2;
                        c.Descriptor = ModifierDescriptor.Racial;
                        c.Stat = StatType.SkillKnowledgeArcana;
                    });

                    feature.AddComponent<PrerequisiteFeaturesFromList>(c =>
                    {
                        c.Amount = 1;
                        c.Group = Prerequisite.GroupType.All;
                        c.m_Features = skilledFeatures.Select(f => f.ToReference()).ToArray();
                    });

                    feature.AddComponent<PrerequisiteFeaturesFromList>(c =>
                    {
                        c.Amount = 1;
                        c.Group = Prerequisite.GroupType.All;
                        c.m_Features = slaFeatures.Select(f => f.feature.ToReference()).ToArray();
                    });

                    foreach (var f in slaFeatures.SelectMany(sla => sla.facts.Append(sla.feature as BlueprintUnitFact)).Concat(skilledFeatures))
                    {
                        feature.AddRemoveFeatureOnApply(c => c.m_Feature = f.ToReference<BlueprintUnitFactReference>());
                    }

                    return feature;
                });

            return feature.AddBlueprintDeferred(GeneratedGuid.CrusadingMagic);
        }
    }
}
