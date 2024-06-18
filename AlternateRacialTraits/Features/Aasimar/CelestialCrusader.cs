using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.ElementsSystem;
using Kingmaker.Enums;
using Kingmaker.UnitLogic.Mechanics.Conditions;

using MicroWrath;
using MicroWrath.BlueprintsDb;
using MicroWrath.Extensions;
using MicroWrath.Extensions.Components;
using MicroWrath.InitContext;
using MicroWrath.Localization;
using MicroWrath.Util;

using static MicroWrath.Encyclopedia;

namespace AlternateRacialTraits.Features.Aasimar
{
    internal static partial class CelestialCrusader
    {
        [LocalizedString]
        internal const string DisplayName = "Celestial Crusader";

        [LocalizedString]
        internal static readonly string Description =
            "Some aasimars follow their destiny to war against the powers of ultimate evil. These individuals gain a " +
            $"+1 insight {new Link(Page.Bonus, "bonus")} on {new Link(Page.Attack, "attack")} rolls " +
            $"and to {new Link(Page.Armor_Class, "AC")} against evil outsiders. " +
            //"and a +2 racial bonus on Knowledge (planes) and Spellcraft checks to identify evil outsiders or items " +
            //"or effects created by evil outsiders;" +
            $"This racial {new Link(Page.Trait, "trait")} replaces celestial resistance and heritage skill bonuses.";

        internal static IInitContextBlueprint<BlueprintFeature> Create()
        {
            var feature = InitContext.NewBlueprint<BlueprintFeature>(GeneratedGuid.Get(nameof(CelestialCrusader)))
                .Combine(BlueprintsDb.Owlcat.BlueprintFeature.SubtypeEvil)
                .Combine(BlueprintsDb.Owlcat.BlueprintFeature.OutsiderType)
                .Combine(AasimarFeatureSelection.SkilledPrerequisite())
                .Map(bps =>
                {
                    var (feature, evil, outsider, skilledPrerequisites) = bps.Flatten();

                    feature.m_DisplayName = Localized.DisplayName;
                    feature.m_Description = Localized.Description;

                    feature.SetIcon("ab2e9bc2629773d4f9080c596d6feb4f", 21300000);

                    feature.AddACBonusAgainstFactOwnerMultiple(c =>
                    {
                        c.m_Facts =
                        [
                            evil.ToReference<BlueprintUnitFactReference>(),
                            outsider.ToReference<BlueprintUnitFactReference>()
                        ];
                        c.Bonus = 1;
                        c.Descriptor = ModifierDescriptor.Insight;
                    });

                    feature.AddComponent<AttackBonusConditional>(c =>
                    {
                        c.Bonus = 1;
                        c.Descriptor = ModifierDescriptor.Insight;

                        c.Conditions.Operation = Operation.And;

                        c.Conditions.Add(
                            [
                                new ContextConditionHasFact
                                {
                                    m_Fact = evil.ToReference<BlueprintUnitFactReference>()
                                },
                                new ContextConditionHasFact
                                {
                                    m_Fact = outsider.ToReference<BlueprintUnitFactReference>()
                                }
                            ]);
                    });

                    feature.AddPrerequisiteFeature(BlueprintsDb.Owlcat.BlueprintFeature.CelestialResistance, removeOnApply: true)
                        .Group = Prerequisite.GroupType.All;

                    //foreach (var c in skilledPrerequisites)
                    //    feature.AddComponent(c);
                    feature.AddComponents(skilledPrerequisites);

                    return feature;
                });

            return feature.AddBlueprintDeferred(GeneratedGuid.CelestialCrusader);
        }
    }
}
