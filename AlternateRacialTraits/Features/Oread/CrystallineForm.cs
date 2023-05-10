using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Enums;

using MicroWrath;
using MicroWrath.BlueprintInitializationContext;
using MicroWrath.BlueprintsDb;
using MicroWrath.Extensions;
using MicroWrath.Extensions.Components;
using MicroWrath.Localization;

using static MicroWrath.Encyclopedia;

namespace AlternateRacialTraits.Features.Oread
{
    using InitFeature = BlueprintInitializationContext.ContextInitializer<BlueprintFeature>;

    internal static class CrystallineForm
    {
        [LocalizedString]
        public const string DisplayName = "Crystalline Form";
        [LocalizedString]
        public static readonly string Description =
            $"Oreads with this trait gain a +2 racial {new Link(Page.Bonus, "bonus")} to " +
            $"{new Link(Page.Armor_Class, "AC")} against rays thanks to their reflective crystalline " +
            "skin. In addition, once per day, they can deflect a single ray attack targeted at them as if they " +
            "were using the Deflect Arrows feat. This racial trait replaces earth affinity.";

        internal static class Resource
        {

        }

        internal static InitFeature Create(BlueprintInitializationContext context)
        {
            var feature = context.NewBlueprint<BlueprintFeature>(GeneratedGuid.Get("CrystallineForm"), nameof(CrystallineForm))
                .Map((BlueprintFeature feature) =>
                {
                    feature.m_DisplayName = LocalizedStrings.Features_Oread_CrystallineForm_DisplayName;
                    feature.m_Description = LocalizedStrings.Features_Oread_CrystallineForm_Description;

                    feature.SetIcon("b3355206cc666b543addf8d60df20299", 21300000);

                    feature.Groups = new[] { FeatureGroup.Racial };

                    feature.AddACBonusAgainstWeaponCategory(c =>
                    {
                        c.ArmorClassBonus = 2;
                        c.Category = WeaponCategory.Ray;

                        c.Descriptor = ModifierDescriptor.Racial;
                    });

                    feature.AddPrerequisiteFeature(BlueprintsDb.Owlcat.BlueprintFeature.AcidAffinityOread, true);

                    return feature;
                });

            return feature;
        }
    }
}
