using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.Enums.Damage;
using Kingmaker.UnitLogic.Class.LevelUp.Actions;
using Kingmaker.UnitLogic.FactLogic;

using MicroWrath;
using MicroWrath.BlueprintsDb;
using MicroWrath.Components;
using MicroWrath.Extensions;
using MicroWrath.Extensions.Components;
using MicroWrath.InitContext;
using MicroWrath.Localization;

using static MicroWrath.Encyclopedia;

namespace AlternateRacialTraits.Features.Tiefling;

internal static partial class ScaledSkin
{
    [LocalizedString]
    internal const string DisplayName = "Scaled Skin";

    [LocalizedString]
    internal static readonly string Description =
        "The skin of these tieflings provides some energy resistance, but is also as hard as armor. " +
        $"Choose one of the following {new Link(Page.Energy_Damage, "energy")} types: " +
        $"cold, electricity, or fire. A tiefling with this {new Link(Page.Trait, "trait")} gains " +
        $"{new Link(Page.Energy_Resistance, "resistance")} 5 in the chosen energy type and also gains a " +
        $"+1 natural armor {new Link(Page.Bonus, "bonus")} to {new Link(Page.Armor_Class, "AC")}. " +
        "This racial trait replaces fiendish resistance.";

    static IInitContextBlueprint<BlueprintFeature>[] CreateResistanceFeatures()
    {
        var cold = InitContext.NewBlueprint<BlueprintFeature>(GeneratedGuid.Get("ScaledSkinCold"))
            .Map(feature =>
             {
                 _ = feature.AddAddDamageResistanceEnergy(c =>
                 {
                     c.Type = DamageEnergyType.Cold;
                     c.Value = 5;
                 });

                 return feature;
             });

        var electricity = InitContext.NewBlueprint<BlueprintFeature>(GeneratedGuid.Get("ScaledSkinElectricity"))
            .Map(feature =>
            {
                _ = feature.AddAddDamageResistanceEnergy(c =>
                {
                    c.Type = DamageEnergyType.Electricity;
                    c.Value = 5;
                });

                return feature;
            });

        var fire = InitContext.NewBlueprint<BlueprintFeature>(GeneratedGuid.Get("ScaledSkinFire"))
            .Map(feature =>
            {
                _ = feature.AddAddDamageResistanceEnergy(c =>
                {
                    c.Type = DamageEnergyType.Fire;
                    c.Value = 5;
                });

                return feature;
            });

        return new [] { (cold, GeneratedGuid.ScaledSkinCold), (electricity, GeneratedGuid.ScaledSkinElectricity), (fire, GeneratedGuid.ScaledSkinFire) }
            .Select(pair =>
                pair.Item1.Map(f =>
                {
                    var displayNameKey = $"{Localized.DisplayName.Key}.{f.GetComponent<AddDamageResistanceEnergy>().Type}";

                    var energyText = LocalizedTexts.Instance.DamageEnergy.Entries
                        .First(e => e.Value == f.GetComponent<AddDamageResistanceEnergy>().Type)
                        .Text;

                    LocalizedStrings.DefaultStringEntries.Add(displayNameKey, $"{DisplayName} - {energyText}");

                    f.m_DisplayName = new() { Key = displayNameKey };

                    f.m_Description = Localized.Description;

                    f.Groups = [FeatureGroup.Racial];

                    return f;
                })
                .AddBlueprintDeferred(pair.Item2))
            .ToArray();
    }

    internal static IInitContextBlueprint<BlueprintFeatureSelection> CreateResistanceSelection()
    {
        var selection = InitContext.NewBlueprint<BlueprintFeatureSelection>(GeneratedGuid.Get("ScaledSkinResistanceSelection"))
            .Combine(CreateResistanceFeatures().Collect())
            .Map(bps =>
            {
                var (selection, features) = bps;

                selection.m_DisplayName = Localized.DisplayName;
                selection.m_Description = Localized.Description;

                selection.AddFeatures(features);

                selection.Groups = [FeatureGroup.Racial];

                _ = selection.AddAddStatBonus(c =>
                {
                    c.Value = 1;
                    c.Stat = StatType.AC;
                    c.Descriptor = ModifierDescriptor.NaturalArmor;
                });

                _ = selection.AddComponent<SelectionPriority>(c =>
                {
                    c.PhasePriority = Kingmaker.UI.MVVM._VM.CharGen.Phases.
                        CharGenPhaseBaseVM.ChargenPhasePriority.RaceFeatures;

                    c.ActionPriority = LevelUpActionPriority.Heritage;
                });

                _= selection.AddPrerequisiteFeature(BlueprintsDb.Owlcat.BlueprintFeature.FiendishResistance, removeOnApply: true);

                return selection;
            });

        return selection.AddBlueprintDeferred(GeneratedGuid.ScaledSkinResistanceSelection);
    }
}