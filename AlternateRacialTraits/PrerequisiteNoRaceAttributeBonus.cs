using System;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Class.LevelUp;

using MicroWrath.Localization;

namespace AlternateRacialTraits;

[AllowedOn(typeof(BlueprintFeature))]
internal partial class PrerequisiteNoRaceStatBonus : Prerequisite, IParamPrerequisite
{
    public StatType Stat;
    public bool Parametrized;

    bool Check(UnitDescriptor unit, StatType stat)
    {
        var statValue = unit.Stats.Attributes.First(s => s.Type == stat);

        return !statValue.Modifiers.Any(m =>
            m.ModDescriptor == ModifierDescriptor.Racial &&
            m.Source.Blueprint == unit.Progression.Race &&
            m.ModValue > 0);
    }

    public bool CanBeSelected(BlueprintParametrizedFeature parametrizedFeature, UnitDescriptor unit, FeatureParam param)
    {
        if (Parametrized && parametrizedFeature.ParameterType is FeatureParameterType.Skill)
        {
            return Check(unit, param.StatType!.Value);
        }

        return true;
    }

    public override bool CheckInternal(
        FeatureSelectionState selectionState,
        UnitDescriptor unit,
        LevelUpState state)
    {
        if (Parametrized)
            return true;

        return Check(unit, Stat);
    }

    [LocalizedString]
    public const string NoRacialAttributeText = "Doesn't have the following racial bonus";

    public override string GetUITextInternal(UnitDescriptor unit) =>
        $"{Localized.NoRacialAttributeText}: {LocalizedTexts.Instance.Stats.GetText(Stat)}";
}
