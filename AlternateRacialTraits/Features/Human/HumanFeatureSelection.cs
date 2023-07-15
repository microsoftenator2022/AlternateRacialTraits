using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Class.LevelUp.Actions;
using Kingmaker.UnitLogic.Class.LevelUp;

using MicroWrath;
using MicroWrath.BlueprintInitializationContext;
using MicroWrath.BlueprintsDb;
using MicroWrath.Components;
using MicroWrath.Constructors;
using MicroWrath.Extensions;
using MicroWrath.Extensions.Components;
using MicroWrath.Localization;
using MicroWrath.Util;
using MicroWrath.Util.Linq;

namespace AlternateRacialTraits.Features.Human
{
    internal static class NoAdditionalTraits
    {
        [LocalizedString]
        public static readonly string DisplayName = "None";
        [LocalizedString]
        public static readonly string Description = "No alternate trait";

        internal static BlueprintInitializationContext.ContextInitializer<BlueprintFeature> Create(BlueprintInitializationContext context) =>
            context.NewBlueprint<BlueprintFeature>(GeneratedGuid.NoAdditionaHumanTraits, nameof(NoAdditionalTraits))
                .Map((BlueprintFeature feat) =>
                {
                    feat.m_DisplayName = LocalizedStrings.Features_Human_NoAdditionalTraits_DisplayName;
                    feat.m_Description = LocalizedStrings.Features_Human_NoAdditionalTraits_Description;
                        
                    feat.HideInUI = true;
                    feat.HideInCharacterSheetAndLevelUp = true;

                    return feat;
                });
    }

    internal static class HumanFeatureSelection
    {
        [LocalizedString]
        public static readonly string DisplayName = "Alternate Racial Traits";
        [LocalizedString]
        public static readonly string Description = "The following alternate traits are available";

        internal static BlueprintInitializationContext.ContextInitializer<BlueprintFeatureSelection> Create(BlueprintInitializationContext context) =>
            context.NewBlueprint<BlueprintFeatureSelection>(GeneratedGuid.HumanFeatureSelection, nameof(HumanFeatureSelection))
                .Map((BlueprintFeatureSelection selection) =>
                {
                    MicroLogger.Debug(() => $"Setting up {nameof(HumanFeatureSelection)}");

                    selection.m_DisplayName = LocalizedStrings.Features_Human_HumanFeatureSelection_DisplayName;
                    selection.m_Description = LocalizedStrings.Features_Human_HumanFeatureSelection_Description;

                    selection.Groups = new[] { FeatureGroup.Racial };

                    selection.AddComponent<OverrideSelectionPriority>(c =>
                        c.Priority = Kingmaker.UI.MVVM._VM.CharGen.Phases.
                            CharGenPhaseBaseVM.ChargenPhasePriority.RaceFeatures);

                    return selection;
                });

        [Init]
        internal static void Init()
        {
            var initContext = new BlueprintInitializationContext(Triggers.BlueprintsCache_Init);

            var bonusFeatDummy = initContext.NewBlueprint<BlueprintFeature>(
                GeneratedGuid.BasicFeatSelectionDummy, nameof(GeneratedGuid.BasicFeatSelectionDummy));

            var selection = HumanFeatureSelection.Create(initContext);

            var humanBonusFeat = HumanBonusFeat.Create(initContext)
                .Combine(bonusFeatDummy)
                .Map(bps =>
                {
                    var (bonusFeat, dummy) = bps;

                    bonusFeat.AddPrerequisiteFeature(dummy.ToMicroBlueprint(), hideInUI: true, removeOnApply: true);

                    dummy.SetIcon(bonusFeat.Icon);

                    dummy.m_DisplayName = bonusFeat.m_DisplayName;
                    dummy.m_Description = bonusFeat.m_Description;

                    return bonusFeat;
                });

            var noMoreSelections = NoAdditionalTraits.Create(initContext)
                .Combine(bonusFeatDummy)
                .Map(bps =>
                {
                    var (noTraits, dummy) = bps;

                    noTraits.AddComponent<PrerequisiteNoFeature>(pnf =>
                    {
                        pnf.m_Feature = dummy.ToReference<BlueprintFeatureReference>();

                        pnf.HideInUI = true;
                    });

                    return noTraits;
                });

            var awareness = AwarenessFeature.Create(initContext)
                .Combine(bonusFeatDummy)
                .Map(bps =>
                {
                    var (awareness, dummy) = bps;

                    awareness.AddPrerequisiteFeature(dummy.ToMicroBlueprint(), hideInUI: true, removeOnApply: true);

                    return awareness;
                });

            var comprehensiveEducation = ComprehensiveEducation.Create(initContext);
            var giantAcestry = GiantAncestry.Create(initContext);
            var historyOfTerrors = HistoryOfTerrorsTrait.Create(initContext);
            var practicedHunter = PracticedHunter.Create(initContext);
            
            var unstoppableMagic = UnstoppableMagic.Create(initContext)
                .Combine(bonusFeatDummy)
                .Map(bps =>
                {
                    var (unstoppableMagic, dummy) = bps;

                    unstoppableMagic.AddPrerequisiteFeature(dummy.ToMicroBlueprint(), hideInUI: true, removeOnApply: true);

                    return unstoppableMagic;
                });

            var focusedStudy = FocusedStudyProgression.Create(initContext)
                .Combine(bonusFeatDummy)
                .Map(bps =>
                {
                    var (focusedStudy, dummy) = bps;

                    focusedStudy.AddPrerequisiteFeature(dummy.ToMicroBlueprint(), hideInUI: true, removeOnApply: true);

                    return focusedStudy as BlueprintFeature;
                });

            var dualTalent = DualTalent.Create(initContext)
                .Combine(bonusFeatDummy)
                .Map(bps =>
                {
                    var (dualTalent, dummy) = bps;

                    dualTalent.AddPrerequisiteFeature(dummy.ToMicroBlueprint(), hideInUI: true, removeOnApply: true);
                    dualTalent.AddPrerequisiteFeature(BlueprintsDb.Owlcat.BlueprintFeature.HumanSkilled, hideInUI: false, removeOnApply: true);

                    return dualTalent;
                });

            var militaryTradition = MilitaryTradition.Create(initContext)
                .Combine(bonusFeatDummy)
                .Map(bps =>
                {
                    var (mtFirst, mtSecond, dummy) = bps.Flatten();

                    mtFirst.AddPrerequisiteFeature(dummy.ToMicroBlueprint(), hideInUI: true, removeOnApply: true);

                    return (mtFirst, mtSecond);
                });

            var adoptiveParentage = AdoptiveParentage.Create(initContext)
                .Combine(bonusFeatDummy)
                .Map(bps =>
                {
                    var (adoptiveParentage, dummy) = bps;

                    adoptiveParentage.AddPrerequisiteFeature(dummy.ToMicroBlueprint(), hideInUI: true, removeOnApply: true);

                    return adoptiveParentage as BlueprintFeature;
                });

            var features = (new[]
            {
                humanBonusFeat,
                noMoreSelections,
                awareness,
                comprehensiveEducation,
                giantAcestry,
                historyOfTerrors,
                practicedHunter,
                unstoppableMagic,
                focusedStudy,
                dualTalent,
                adoptiveParentage
            }).Combine()
            .Combine(militaryTradition)
            .Map(bps =>
            {
                var (list, mtFirst, mtSecond) = bps.Flatten();

                return list.Append(mtFirst).Append(mtSecond);
            });

            selection
                .Combine(bonusFeatDummy)
                .Combine(features)
                .Combine(noMoreSelections)
                .Combine(initContext.GetBlueprint(BlueprintsDb.Owlcat.BlueprintRace.HumanRace))
                .Map(bps =>
                {
                    var (selection, dummy, features, noMoreSelections, humanRace) = bps.Flatten();

                    var raceFeatures = new List<BlueprintFeatureBase> { selection, dummy };

                    raceFeatures.AddRange(humanRace.Features
                        .Where(f => f != BlueprintsDb.Owlcat.BlueprintFeatureSelection.BasicFeatSelection.GetBlueprint()));

                    humanRace.m_Features = raceFeatures
                        .Select(bp => bp.ToReference<BlueprintFeatureBaseReference>())
                        .ToArray();

                    var selectionRef = selection.ToReference<BlueprintFeatureBaseReference>();

                    foreach (var f in features.Where(f => f != noMoreSelections))
                    {
                        f.AddComponent(new UnitFactActivateEvent(e =>
                        {
                            Util.AddLevelUpSelection(e.Owner, new[] { selectionRef }, e.Owner.Progression.Race);
                        }));
                    }

                    selection.AddFeatures(features.Select(MicroBlueprint.ToMicroBlueprint));
                })
                .Register();
        }
    }
}
