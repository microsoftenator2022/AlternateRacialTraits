using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Items.Weapons;

using MicroWrath;
using MicroWrath.BlueprintsDb;
using MicroWrath.Extensions;
using MicroWrath.Extensions.Components;
using MicroWrath.InitContext;
using MicroWrath.Localization;
using MicroWrath.Util;

using static MicroWrath.Encyclopedia;

namespace AlternateRacialTraits.Features.Tiefling;

internal static partial class MawOrClaw
{
    [LocalizedString]
    internal const string DisplayName = "Maw or Claw";

    [LocalizedString]
    internal const string MawDisplayName = DisplayName + " (Bite)";

    [LocalizedString]
    internal const string ClawDisplayName = DisplayName + " (Claws)";

    [LocalizedString]
    internal static readonly string Description =
        "Some tieflings take on the more bestial aspects of their fiendish ancestors. These tieflings exhibit " +
        "either powerful, toothy maws or dangerous claws. The tiefling can choose " +
        $"a bite {new Link(Page.Attack, "attack")} that deals " +
        $"1d6 points of {new Link(Page.Damage, "damage")} or two claws that each deal 1d4 points of damage. " +
        $"These attacks are {new Link(Page.NaturalAttack, "primary natural attacks")}. " +
        $"This racial {new Link(Page.Trait, "trait")} replaces the spell-like ability racial trait.";

    internal static IInitContextBlueprint<BlueprintFeature> Create()
    {
        var mawWeapon = InitContext.CloneBlueprint(
            BlueprintsDb.Owlcat.BlueprintItemWeapon.Bite1d6,
            GeneratedGuid.Get("MawOrClawMawWeapon"))
            .Map(blueprint =>
            {
                blueprint.m_AlwaysPrimary = true;

                return blueprint;
            })
            .AddBlueprintDeferred(GeneratedGuid.MawOrClawMawWeapon);

        var clawWeapon = InitContext.CloneBlueprint(
            BlueprintsDb.Owlcat.BlueprintItemWeapon.Claw1d4,
            GeneratedGuid.Get("MawOrClawClawWeapon"))
            .Map(blueprint =>
            {
                blueprint.m_AlwaysPrimary = true;

                return blueprint;
            })
            .AddBlueprintDeferred(GeneratedGuid.MawOrClawClawWeapon);

        var mawFeature = InitContext.NewBlueprint<BlueprintFeature>(GeneratedGuid.Get("MawOrClawMaw"))
            .Combine(mawWeapon)
            .Map(bps =>
            {
                var (feature, weapon) = bps;

                feature.m_DisplayName = Localized.MawDisplayName;
                feature.m_Description = Localized.Description;

                feature.Groups = [FeatureGroup.Racial];

                _ = feature.AddAddAdditionalLimb(c => c.m_Weapon = weapon.ToReference());

                return feature;
            })
            .AddBlueprintDeferred(GeneratedGuid.MawOrClawMaw);

        var clawFeature = InitContext.NewBlueprint<BlueprintFeature>(GeneratedGuid.Get("MawOrClawClaw"))
            .Combine(clawWeapon)
            .Map(bps =>
            {
                var (feature, weapon) = bps;

                feature.m_DisplayName = Localized.ClawDisplayName;
                feature.m_Description = Localized.Description;

                feature.Groups = [FeatureGroup.Racial];

                _ = feature.AddEmptyHandWeaponOverride(c =>
                {
                    c.m_Weapon = weapon.ToReference();
                    c.IsPermanent = true;
                });

                return feature;
            })
            .AddBlueprintDeferred(GeneratedGuid.MawOrClawClaw);

        var feature = InitContext.NewBlueprint<BlueprintFeatureSelection>(GeneratedGuid.Get(nameof(MawOrClaw)))
            .Combine(TieflingFeatureSelection.SLAPrerequisiteComponents())
            .Combine(mawFeature)
            .Combine(clawFeature)
            .Map(things =>
            {
                var (selection, slaPrerequisites, maw, claw) = things.Expand();

                selection.m_DisplayName = Localized.DisplayName;
                selection.m_Description = Localized.Description;

                selection.Groups = [FeatureGroup.Racial];

                _ = selection.AddComponents(slaPrerequisites);

                selection.AddFeatures(maw, claw);

                return selection;
            });

        return feature.AddBlueprintDeferred(GeneratedGuid.MawOrClaw);
    }
}