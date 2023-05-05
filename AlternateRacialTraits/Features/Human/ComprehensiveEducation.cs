using System;
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

namespace AlternateRacialTraits.Features.Human
{
    internal static class ComprehensiveEducation
    {
        [LocalizedString]
        public static readonly string DisplayName = "Comprehensive Eduction";

        [LocalizedString]
        public static readonly string Description =
            "Humans raised with skilled teachers draw upon vast swathes of knowledge gained over centuries " +
            "of civilization. They gain all Knowledge and Lore skills as " +
            $"{new Link(Page.Skills, "class skills")}, and they gain a +1 racial " +
            $"{new Link(Page.Bonus, "bonus")} on skill {new Link(Page.Check, "checks")} for " +
            "each Knowledge or Lore skill that they gain as a class skill from their class levels. This " +
            "racial trait replaces the Skilled trait.";

        internal static BlueprintInitializationContext.ContextInitializer<BlueprintFeature> Create(BlueprintInitializationContext initContext) =>
            initContext.NewBlueprint<BlueprintFeature>(GeneratedGuid.ComprehensiveEducation, nameof(ComprehensiveEducation))
                .Map((BlueprintFeature blueprint) =>
                {
                    blueprint.m_DisplayName = LocalizedStrings.Features_Human_ComprehensiveEducation_DisplayName;
                    blueprint.m_Description = LocalizedStrings.Features_Human_ComprehensiveEducation_Description;

                    foreach (var skill in new[]
                        {
                            StatType.SkillKnowledgeArcana,
                            StatType.SkillKnowledgeWorld,
                            StatType.SkillLoreNature,
                            StatType.SkillLoreReligion
                        })
                    {
                        blueprint.AddComponent<AddClassSkill>(acs => acs.Skill = skill);
                        blueprint.AddComponent<AddBackgroundClassSkill>(abcs => abcs.Skill = skill);
                    }

                    blueprint.SetIcon("702d40939a2693b4abb3fa3e9eee30cb", 21300000);
                        
                    blueprint.AddPrerequisiteFeature(BlueprintsDb.Owlcat.BlueprintFeature.HumanSkilled, false, true);

                    return blueprint;
                });
    }

}
