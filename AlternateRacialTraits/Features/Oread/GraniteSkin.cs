﻿using System;
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

namespace AlternateRacialTraits.Features.Oread
{
    internal static class GraniteSkin
    {
        [LocalizedString]
        public const string DisplayName = "Granite Skin";
        [LocalizedString]
        public static readonly string Description = "Rocky growths cover the skin of oreads with this racial trait. " +
            $"They gain a +1 racial {new Link(Page.Bonus, "bonus")} to natural " +
            $"{new Link(Page.Armor_Class, "armor")}. This racial trait replaces Oread energy resistance.";

        internal static BlueprintInitializationContext.ContextInitializer<BlueprintFeature> Create(BlueprintInitializationContext context) =>
            context.NewBlueprint<BlueprintFeature>(GeneratedGuid.Get(nameof(GraniteSkin)), nameof(GraniteSkin))
                .Map(blueprint =>
                {
                    blueprint.m_DisplayName = LocalizedStrings.Features_Oread_GraniteSkin_DisplayName;
                    blueprint.m_Description = LocalizedStrings.Features_Oread_GraniteSkin_Description;

                    blueprint.Groups = new[] { FeatureGroup.Racial };

                    blueprint.AddComponent<AddStatBonus>(c =>
                    {
                        c.Stat = StatType.AC;
                        c.Value = 1;
                        c.Descriptor = ModifierDescriptor.Racial;
                    });

                    blueprint.AddPrerequisiteFeature(BlueprintsDb.Owlcat.BlueprintFeature.EnergyResistanceOread, true);

                    return blueprint;
                });
    }
}