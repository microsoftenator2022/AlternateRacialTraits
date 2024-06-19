using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem.Rules.Abilities;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Parts;

using MicroWrath;
using MicroWrath.BlueprintsDb;
using MicroWrath.Extensions;
using MicroWrath.Extensions.Components;
using MicroWrath.InitContext;
using MicroWrath.Localization;
using MicroWrath.Util;

using static MicroWrath.Encyclopedia;

namespace AlternateRacialTraits.Features.Aasimar;

internal static partial class Heavenborn
{
    [LocalizedString]
    internal const string DisplayName = "Heavenborn";

    [LocalizedString]
    internal static readonly string Description =
        $"Born in the celestial realms, aasimars with this racial {new Link(Page.Trait, "trait")} " +
        $"gain a +2 {new Link(Page.Bonus, "bonus")} on {new Link(Page.Knowledge_Arcana, "Knowledge (Arcana)")} " +
        $"checks and they cast {new Link(Page.Spells, "spells")} with the good or light " +
        $"{new Link(Page.Spell_Descriptor, "descriptor")} at +1 {new Link(Page.Caster_Level, "caster level")}. " +
        "This racial trait replaces the skilled and spell-like ability racial traits.";

    public static readonly List<IMicroBlueprint<BlueprintAbility>> LightSpells =
    [
        BlueprintsDb.Owlcat.BlueprintAbility.DayLight,
        BlueprintsDb.Owlcat.BlueprintAbility.FaerieFire,
        BlueprintsDb.Owlcat.BlueprintAbility.Flare,
        BlueprintsDb.Owlcat.BlueprintAbility.FlareBurst,
        BlueprintsDb.Owlcat.BlueprintAbility.PillarOfLife,
        BlueprintsDb.Owlcat.BlueprintAbility.Sunbeam,
        BlueprintsDb.Owlcat.BlueprintAbility.Sunburst
    ];

    internal static IInitContextBlueprint<BlueprintFeature> Create()
    {
        var feature = InitContext.NewBlueprint<BlueprintFeature>(GeneratedGuid.Get(nameof(Heavenborn)))
            .Combine(AasimarFeatureSelection.SkilledPrerequisiteComponents())
            .Combine(AasimarFeatureSelection.SLAPrerequisiteComponents())
            .Map(bps =>
            {
                var (feature, skilledPrerequisites, slaPrerequisites) = bps.Flatten();

                feature.m_DisplayName = Localized.DisplayName;
                feature.m_Description = Localized.Description;

                feature.SetIcon("869e3bc156f9f3646a6a3eff5e0c5c60", 21300000);

                _ = feature.AddAddStatBonus(c =>
                {
                    c.Value = 2;
                    c.Stat = StatType.SkillKnowledgeArcana;
                    c.Descriptor = ModifierDescriptor.Racial;
                });

                _ = feature.AddComponent<HeavenbornComponent>(c =>
                    c.LightSpellList = LightSpells.Select(mbp => mbp.ToReference()).ToList());

                _ = feature.AddComponents(skilledPrerequisites);
                _ = feature.AddComponents(slaPrerequisites);

                return feature;
            });

        return feature.AddBlueprintDeferred(GeneratedGuid.Heavenborn);
    }

    internal class HeavenbornComponent : UnitFactComponentDelegate, IInitiatorRulebookHandler<RuleCalculateAbilityParams>
    {
        public List<BlueprintAbilityReference> LightSpellList = [];

        public void OnEventAboutToTrigger(RuleCalculateAbilityParams evt)
        {
            if (evt.Spellbook == null)
                return;

            var descriptorComponent = evt.Spell.GetComponent<SpellDescriptorComponent>();
            
            var spellDescriptor = descriptorComponent?.Descriptor.Value ?? SpellDescriptor.None;
            spellDescriptor = UnitPartChangeSpellElementalDamage.ReplaceSpellDescriptorIfCan(base.Owner, spellDescriptor);

            if (spellDescriptor.HasAnyFlag(SpellDescriptor.Good) || LightSpellList.Any(s => s.Get() == evt.Spell))
                evt.AddBonusCasterLevel(1);
        }

        public void OnEventDidTrigger(RuleCalculateAbilityParams evt) { }
    }
}
