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
using MicroWrath.Deferred;
using MicroWrath.Localization;
using MicroWrath.Util;

using static MicroWrath.Encyclopedia;

namespace AlternateRacialTraits.Features.Aasimar;

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

    internal static IDeferredBlueprint<BlueprintFeature> Create()
    {
        var feature = Deferred.NewBlueprint<BlueprintFeature>(GeneratedGuid.Get(nameof(CrusadingMagic)))
            .Combine(AasimarFeatureSelection.SLAPrerequisiteComponents())
            .Combine(AasimarFeatureSelection.SkilledPrerequisiteComponents())
            .Map(bps =>
            {
                var (feature, slaPrerequisites, skilledPrerequisites) = bps.Flatten();

                feature.m_DisplayName = Localized.DisplayName;
                feature.m_Description = Localized.Description;

                feature.SetIcon("ecb2815de750b0d4ba865e22c84b06c8", 21300000);

                feature.Groups = [FeatureGroup.Racial];

                _ = feature.AddSpellPenetrationBonus(c =>
                {
                    c.Value = 2;
                    c.Descriptor = ModifierDescriptor.Racial;
                });

                _ = feature.AddAddStatBonus(c =>
                {
                    c.Value = 2;
                    c.Descriptor = ModifierDescriptor.Racial;
                    c.Stat = StatType.SkillKnowledgeArcana;
                });

                //foreach (var c in skilledPrerequisites)
                //    feature.AddComponent(c);
                //foreach (var c in slaPrerequisites)
                //    feature.AddComponent(c);
                _ = feature.AddComponents(slaPrerequisites);
                _ = feature.AddComponents(skilledPrerequisites);
                
                return feature;
            });

        return feature.AddBlueprintDeferred(GeneratedGuid.CrusadingMagic);
    }
}
