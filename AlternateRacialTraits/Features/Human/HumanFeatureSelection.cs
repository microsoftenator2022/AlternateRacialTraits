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
//using MicroWrath.BlueprintInitializationContext;
using MicroWrath.BlueprintsDb;
using MicroWrath.Components;
using MicroWrath.Constructors;
using MicroWrath.Extensions;
using MicroWrath.Extensions.Components;
using MicroWrath.InitContext;
using MicroWrath.Localization;
using MicroWrath.Util;
using MicroWrath.Util.Linq;
using HarmonyLib;
using System.Threading;

namespace AlternateRacialTraits.Features.Human
{
    internal static partial class NoAdditionalTraits
    {
        [LocalizedString]
        public static readonly string DisplayName = "None";
        [LocalizedString]
        public static readonly string Description = "No alternate trait";

        internal static IInitContext<BlueprintFeature> Create() =>
            InitContext.NewBlueprint<BlueprintFeature>(GeneratedGuid.NoAdditionaHumanTraits, nameof(NoAdditionalTraits))
                .Map((BlueprintFeature feat) =>
                {
                    feat.m_DisplayName = Localized.DisplayName;
                    feat.m_Description = Localized.Description;
                        
                    feat.HideInUI = true;
                    feat.HideInCharacterSheetAndLevelUp = true;

                    return feat;
                })
                .AddBlueprintDeferred(GeneratedGuid.NoAdditionaHumanTraits);
    }

    internal static partial class HumanFeatureSelection
    {
        [LocalizedString]
        public static readonly string DisplayName = "Alternate Racial Traits";
        [LocalizedString]
        public static readonly string Description = "The following alternate traits are available";

        internal static IInitContext<BlueprintFeatureSelection> Create()
        {
            var bonusFeatDummy = InitContext.NewBlueprint<BlueprintFeature>(GeneratedGuid.BasicFeatSelectionDummy)
                .AddBlueprintDeferred(GeneratedGuid.BasicFeatSelectionDummy);

            var selection =
                InitContext.NewBlueprint<BlueprintFeatureSelection>(GeneratedGuid.HumanFeatureSelection)
                    .Map((BlueprintFeatureSelection selection) =>
                    {
                        MicroLogger.Debug(() => $"Setting up {nameof(HumanFeatureSelection)}");

                        selection.m_DisplayName = Localized.DisplayName;
                        selection.m_Description = Localized.Description;

                        selection.Groups = [FeatureGroup.Racial];

                        //selection.AddComponent<OverrideSelectionPriority>(c =>
                        //    c.Priority = Kingmaker.UI.MVVM._VM.CharGen.Phases.
                        //        CharGenPhaseBaseVM.ChargenPhasePriority.RaceFeatures);
                        
                        //selection.AddComponent<SelectFeaturePriority>(c => c.Priority = LevelUpActionPriority.Heritage);

                        selection.AddComponent<SelectionPriority>(c =>
                        {
                            c.PhasePriority = Kingmaker.UI.MVVM._VM.CharGen.Phases.
                                CharGenPhaseBaseVM.ChargenPhasePriority.RaceFeatures;

                            c.ActionPriority = LevelUpActionPriority.Heritage;
                        });

                        return selection;
                    });

            var humanBonusFeat = HumanBonusFeat.Create()
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

            var noMoreSelections = NoAdditionalTraits.Create()
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

            var awareness = AwarenessFeature.Create()
                .Combine(bonusFeatDummy)
                .Map(bps =>
                {
                    var (awareness, dummy) = bps;

                    awareness.AddPrerequisiteFeature(dummy.ToMicroBlueprint(), hideInUI: true, removeOnApply: true);

                    return awareness;
                });

            var eyeForTalent = EyeForTalent.Create()
                .Combine(bonusFeatDummy)
                .Map(bps => 
                {
                    var (blueprint, dummy) = bps;

                    blueprint.AddPrerequisiteFeature(dummy.ToMicroBlueprint(), hideInUI: true, removeOnApply: true);

                    return blueprint;
                });

            var comprehensiveEducation = ComprehensiveEducation.Create();
            var giantAncestry = GiantAncestry.Create();
            var historyOfTerrors = HistoryOfTerrorsTrait.Create();
            var practicedHunter = PracticedHunter.Create();
            
            var unstoppableMagic = UnstoppableMagic.Create()
                .Combine(bonusFeatDummy)
                .Map(bps =>
                {
                    var (unstoppableMagic, dummy) = bps;

                    unstoppableMagic.AddPrerequisiteFeature(dummy.ToMicroBlueprint(), hideInUI: true, removeOnApply: true);

                    return unstoppableMagic;
                });

            var focusedStudy = FocusedStudyProgression.Create()
                .Combine(bonusFeatDummy)
                .Map(bps =>
                {
                    var (focusedStudy, dummy) = bps;

                    focusedStudy.AddPrerequisiteFeature(dummy.ToMicroBlueprint(), hideInUI: true, removeOnApply: true);

                    return focusedStudy as BlueprintFeature;
                });

            var dualTalent = DualTalent.Create()
                .Combine(bonusFeatDummy)
                .Map(bps =>
                {
                    var (dualTalent, dummy) = bps;

                    dualTalent.AddPrerequisiteFeature(dummy.ToMicroBlueprint(), hideInUI: true, removeOnApply: true);
                    dualTalent.AddPrerequisiteFeature(BlueprintsDb.Owlcat.BlueprintFeature.HumanSkilled, hideInUI: false, removeOnApply: true);

                    return dualTalent;
                });

            var (militaryTraditionFirst, militaryTraditionSecond) = MilitaryTradition.Create();

            var militaryTradition =
                militaryTraditionFirst
                .Combine(militaryTraditionSecond)
                .Combine(bonusFeatDummy)
                .Map(bps =>
                {
                    var (mtFirst, mtSecond, dummy) = bps.Flatten();

                    mtFirst.AddPrerequisiteFeature(dummy.ToMicroBlueprint(), hideInUI: true, removeOnApply: true);

                    return (mtFirst, mtSecond);
                });

            var adoptiveParentage = AdoptiveParentage.Create()
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
                giantAncestry,
                historyOfTerrors,
                practicedHunter,
                unstoppableMagic,
                focusedStudy,
                dualTalent,
                adoptiveParentage,
                eyeForTalent
            }).Collect()
            .Combine(militaryTradition)
            .Map(bps =>
            {
                var (list, mtFirst, mtSecond) = bps.Flatten();

                return list.Append(mtFirst).Append(mtSecond);
            });

            //InitContext.GetBlueprint(BlueprintsDb.Owlcat.BlueprintFeature.HumanSkilled)
            //    .Map(skilled =>
            //    {
            //    });

            return selection
                .Combine(bonusFeatDummy)
                .Combine(features)
                .Combine(noMoreSelections)
                .Combine(BlueprintsDb.Owlcat.BlueprintRace.HumanRace)
                .Combine(BlueprintsDb.Owlcat.BlueprintFeatureSelection.BasicFeatSelection)
                .Map(bps =>
                {
                    var (selection, dummy, features, noMoreSelections, humanRace, bfs) = bps.Expand();

                    var raceFeatures = new List<BlueprintFeatureBase> { selection, dummy };

                    raceFeatures.AddRange(humanRace.Features
                        .Where(f => f != bfs));

                    humanRace.m_Features = raceFeatures
                        .Select(bp => bp.ToReference<BlueprintFeatureBaseReference>())
                        .ToArray();

                    var selectionRef = selection.ToReference<BlueprintFeatureBaseReference>();

                    foreach (var f in features.Where(f => f != noMoreSelections))
                    {
                        f.AddComponent(new UnitFactActivateEvent(e =>
                        {
                            Util.AddLevelUpSelection(e.Owner, [selectionRef], e.Owner.Progression.Race);
                        }));
                    }

                    selection.AddFeatures(features.Select(MicroBlueprint.ToMicroBlueprint));

                    return selection;
                });
        }

        [Init]
        internal static void Init()
        {
            HumanFeatureSelection.Create()
                .AddOnTrigger(GeneratedGuid.HumanFeatureSelection, Triggers.BlueprintsCache_Init);
        }
    }
}
