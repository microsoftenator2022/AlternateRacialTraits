using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Components;

using MicroWrath;
using MicroWrath.BlueprintInitializationContext;
using MicroWrath.Extensions;
using MicroWrath.Extensions.Components;
using MicroWrath.Localization;

using static MicroWrath.Encyclopedia;

namespace AlternateRacialTraits.Features.Oread
{
    //internal class HPRestoredTrigger : UnitFactComponentDelegate, IInitiatorRulebookHandler<RuleHealDamage>
    //{
    //    public BlueprintUnitFactReference? SourceFact = null;
    //    public ActionList Actions = Default.ActionList;
    //    public AbilitySharedValue SharedValue;

    //    private void RunAction(RuleHealDamage evt, UnitEntityData Target)
    //    {
    //        if (this.SourceFact is not null &&
    //            (evt.SourceFact?.Blueprint is null ||
    //            evt.SourceFact.Blueprint is not BlueprintUnitFact fact ||
    //            fact != this.SourceFact.Get()))
    //            return;

    //        if (this.Actions.HasActions && evt.Reason.Context != null)
    //        {
    //            using var dataScope = evt.Reason.Context.GetDataScope(Target);

    //            dataScope.Context[this.SharedValue] = evt.Value;

    //            this.Actions.Run();
    //        }
    //    }

    //    public void OnEventAboutToTrigger(RuleHealDamage evt) { }

    //    public void OnEventDidTrigger(RuleHealDamage evt)
    //    {
    //        if (evt.Value < 1)
    //            return;

    //        this.RunAction(evt, evt.Target);
    //    }
    //}

    //internal static class StoneInTheBlood
    //{
    //    [LocalizedString]
    //    public const string DisplayName = "Stone in the Blood";
    //    [LocalizedString]
    //    public static readonly string Description =
    //        "Oreads with this racial trait mimic the healing abilities of the mephits, gaining fast healing 2 for " +
    //        "1 round anytime they are subject to acid damage (the acid damage does not need to overcome the oread’s " +
    //        $"{new Link(Page.Energy_Resistance, "resistances")} or " +
    //        $"{new Link(Page.Energy_Immunity, "immunities")} to activate this ability). The oread can heal up " +
    //        $"to 2 {new Link(Page.HP, "hit points")} per level per day with this ability, after which it " +
    //        "ceases to function. This racial trait replaces earth affinity.";

    //    //internal static class Buff
    //    //{
    //    //    internal static BuffInit Create(BlueprintInitializationContext context) =>
    //    //        context.NewBlueprint<BlueprintBuff>(
    //    //            GeneratedGuid.Get("StoneInTheBloodBuff"), nameof(GeneratedGuid.StoneInTheBloodBuff))
    //    //            .Map(buff =>
    //    //            {
    //    //                buff.m_DisplayName = LocalizedStrings.Features_Oread_StoneInTheBlood_DisplayName;
    //    //                buff.m_Description = LocalizedStrings.Features_Oread_StoneInTheBlood_Description;

    //    //                buff.AddComponent<AddFactContextActions>(c =>
    //    //                {

    //    //                });

    //    //                return buff;
    //    //            });
    //    //}

    //    internal static BlueprintInitializationContext.ContextInitializer<BlueprintFeature> Create(BlueprintInitializationContext context)
    //    {



    //        var feature = context.NewBlueprint<BlueprintFeature>(GeneratedGuid.Get("StoneInTheBloodFeature"));



    //        return feature;
    //    }
    //}
}
