using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Kingmaker.Blueprints.Classes;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;

using MicroWrath;
using MicroWrath.Extensions;
using MicroWrath.Extensions.Components;
using MicroWrath.InitContext;
using MicroWrath.Localization;

using static MicroWrath.Encyclopedia;

namespace AlternateRacialTraits.Features.Tiefling;

internal static partial class BeguilingLiar
{
    [LocalizedString]
    internal const string DisplayName = "Beguiling Liar";

    [LocalizedString]
    internal static readonly string Description =
        "Many tieflings find that the best way to get along in the world is to tell others what they want to " +
        "hear. These tieflings’ practice of telling habitual falsehoods grants them a +4 racial " +
        $"{new Link(Page.Bonus, "bonus")} on Bluff checks. " +
        //"to convince an opponent that what they are saying is true when they tell a lie. " +
        $"This racial {new Link(Page.Trait, "trait")} replaces skilled.";

    internal static IInitContextBlueprint<BlueprintFeature> Create()
    {
        var feature = InitContext.NewBlueprint<BlueprintFeature>(GeneratedGuid.Get(nameof(BeguilingLiar)))
            .Combine(TieflingFeatureSelection.SkilledPrerequisiteComponents())
            .Map(things =>
            {
                var (feature, prerequisiteComponents) = things;

                feature.m_DisplayName = Localized.DisplayName;
                feature.m_Description = Localized.Description;

                feature.SetIcon("494cc3f31fcb2a24cb7e69ec5df0055c", 21300000);

                feature.Groups = [FeatureGroup.Racial];

                _ = feature.AddAddStatBonus(c =>
                {
                    c.Value = 4;
                    c.Stat = StatType.CheckBluff;
                    c.Descriptor = ModifierDescriptor.Racial;
                });

                _ = feature.AddComponents(prerequisiteComponents);

                return feature;
            });

        return feature.AddBlueprintDeferred(GeneratedGuid.BeguilingLiar);
    }
}