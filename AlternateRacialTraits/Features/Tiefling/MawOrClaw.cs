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
using MicroWrath.Deferred;
using MicroWrath.Localization;
using MicroWrath.Util;

using static MicroWrath.Encyclopedia;
using UnityEngine;
using Kingmaker.Blueprints.JsonSystem.Converters;

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

    internal static readonly Lazy<IDeferredBlueprint<BlueprintFeature>> MawFeature = new(() =>
    {
        var mawWeapon = Deferred.CloneBlueprint(
            BlueprintsDb.Owlcat.BlueprintItemWeapon.Bite1d6,
            GeneratedGuid.Get("MawOrClawMawWeapon"))
            .Map(blueprint =>
            {
                blueprint.m_AlwaysPrimary = true;

                return blueprint;
            })
            .AddBlueprintDeferred(GeneratedGuid.MawOrClawMawWeapon);

        return Deferred.NewBlueprint<BlueprintFeature>(GeneratedGuid.Get("MawOrClawMaw"))
            .Combine(mawWeapon)
            .Map(bps =>
            {
                var (feature, weapon) = bps;

                feature.m_DisplayName = Localized.MawDisplayName;
                feature.m_Description = Localized.Description;

                feature.m_Icon = weapon.m_Icon;

                feature.Groups = [FeatureGroup.Racial];

                _ = feature.AddAddAdditionalLimb(c => c.m_Weapon = weapon.ToReference());

                return feature;
            })
            .AddBlueprintDeferred(GeneratedGuid.MawOrClawMaw);
    });

    internal static readonly Lazy<IDeferredBlueprint<BlueprintFeature>> ClawFeature = new(() =>
    {
        var clawWeapon = Deferred.CloneBlueprint(
            BlueprintsDb.Owlcat.BlueprintItemWeapon.Claw1d4,
            GeneratedGuid.Get("MawOrClawClawWeapon"))
            .Map(blueprint =>
            {
                blueprint.m_AlwaysPrimary = true;

                blueprint.m_Icon = (Sprite)UnityObjectConverter.AssetList.Get("9100e5f2e80631d47822697ae833a4b6", 21300000);

                return blueprint;
            })
            .AddBlueprintDeferred(GeneratedGuid.MawOrClawClawWeapon);

        return Deferred.NewBlueprint<BlueprintFeature>(GeneratedGuid.Get("MawOrClawClaw"))
            .Combine(clawWeapon)
            .Map(bps =>
            {
                var (feature, weapon) = bps;

                feature.m_DisplayName = Localized.ClawDisplayName;
                feature.m_Description = Localized.Description;

                feature.m_Icon = weapon.m_Icon;

                feature.Groups = [FeatureGroup.Racial];

                _ = feature.AddEmptyHandWeaponOverride(c =>
                {
                    c.m_Weapon = weapon.ToReference();
                    c.IsPermanent = true;
                });

                return feature;
            })
            .AddBlueprintDeferred(GeneratedGuid.MawOrClawClaw);
    });

    internal static IDeferredBlueprint<BlueprintFeature> Create()
    {
        var feature = Deferred.NewBlueprint<BlueprintFeatureSelection>(GeneratedGuid.Get(nameof(MawOrClaw)))
            .Combine(TieflingFeatureSelection.SLAPrerequisiteComponents())
            .Combine(MawFeature.Value)
            .Combine(ClawFeature.Value)
            .Map(things =>
            {
                var (selection, slaPrerequisites, maw, claw) = things.Expand();

                selection.m_DisplayName = Localized.DisplayName;
                selection.m_Description = Localized.Description;

                var texture = AssetUtils.DiagonalCutBlend(maw.m_Icon.texture, claw.m_Icon.texture, invertX: true);

                selection.m_Icon = Sprite.Create(texture, new Rect { width = texture.width, height = texture.height}, new(0.5f, 0.5f));

                selection.Groups = [FeatureGroup.Racial];

                _ = selection.AddComponents(slaPrerequisites);

                selection.AddFeatures(maw, claw);

                return selection;
            });

        return feature.AddBlueprintDeferred(GeneratedGuid.MawOrClaw);
    }
}