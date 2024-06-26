using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Alignments;

using MicroWrath;
using MicroWrath.BlueprintsDb;
using MicroWrath.Extensions;
using MicroWrath.Extensions.Components;
using MicroWrath.Deferred;
using MicroWrath.Localization;
using MicroWrath.Util;

using static MicroWrath.Encyclopedia;

namespace AlternateRacialTraits.Features.Tiefling;

internal static partial class SmiteGood
{
    [LocalizedString]
    internal const string DisplayName = "Smite Good";

    [LocalizedString]
    internal static readonly string Description =
        $"Once per day, a tiefling with this racial {new Link(Page.Trait, "trait")} can smite a " +
        $"{new Link(Page.Alignment, "good-aligned")} creature. As a " +
        $"{new Link(Page.Swift_Action, "swift action")}, the tiefling chooses one target within sight to " +
        $"smite. If this target is good, the tiefling adds her {new Link(Page.Charisma, "Charisma")} bonus " +
        $"(if any) to {new Link(Page.Attack, "attack")} rolls against the target and gains a bonus on " +
        $"{new Link(Page.Damage, "damage")} rolls against the target equal to her number of " +
        $"{new Link (Page.Hit_Dice, "Hit Dice")}. This effect lasts until the first time the tiefling successfully " +
        "hits her designated target. " +
        "This racial trait replaces fiendish sorcery and the tiefling’s spell-like ability.";

    internal static IDeferredBlueprint<BlueprintFeature> Create()
    {
        var resource = Deferred.NewBlueprint<BlueprintAbilityResource>(GeneratedGuid.Get("TieflingSmiteGoodResource"))
            .Map(resource =>
            {
                resource.m_MaxAmount = new() { BaseValue = 1 };

                return resource;
            })
            .AddBlueprintDeferred(GeneratedGuid.TieflingSmiteGoodResource);

        var ability = Deferred.CloneBlueprint(BlueprintsDb.Owlcat.BlueprintAbility.FiendishSmiteGoodAbility, GeneratedGuid.Get("TieflingSmiteGoodAbility"))
            .Combine(resource)
            .Map(bps =>
            {
                var (ability, resource) = bps;

                _ = ability.AddComponent<AbilityResourceLogic>(c =>
                {
                    c.m_RequiredResource = resource.ToReference();
                    c.Amount = 1;
                    c.m_IsSpendResource = true;
                });

                //_ = ability.AddAbilityCasterAlignment(c => c.Alignment = AlignmentMaskType.Evil);

                return ability;
            })
            .AddBlueprintDeferred(GeneratedGuid.TieflingSmiteGoodAbility);

        var feature = Deferred.NewBlueprint<BlueprintFeature>(GeneratedGuid.Get("TieflingSmiteGoodFeature"))
            .Combine(ability)
            .Combine(FiendishSorcery.FiendishSorceryFeature.Value)
            .Combine(TieflingFeatureSelection.SLAPrerequisiteComponents())
            .Map(things =>
            {
                var (feature, ability, fiendishSorcery, slaPrerequisites) = things.Expand();

                feature.m_DisplayName = Localized.DisplayName;
                feature.m_Description = Localized.Description;
                
                feature.m_Icon = ability.m_Icon;

                feature.Groups = [FeatureGroup.Racial];

                _ = feature.AddAddFacts(c => c.m_Facts = [ability.ToReference<BlueprintUnitFactReference>()]);
                //_ = feature.AddComponent<PrerequisiteAlignment>(c => c.Alignment = AlignmentMaskType.Evil);
                _ = feature.AddPrerequisiteFeature(fiendishSorcery.ToMicroBlueprint(), removeOnApply: true);
                _ = feature.AddComponents(slaPrerequisites);

                return feature;
            });

        return feature.AddBlueprintDeferred(GeneratedGuid.TieflingSmiteGoodFeature);
    }
}