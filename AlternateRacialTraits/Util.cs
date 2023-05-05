using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Kingmaker.UnitLogic.Class.LevelUp.Actions;
using Kingmaker.UnitLogic.Class.LevelUp;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Blueprints.Classes;

namespace AlternateRacialTraits
{
    internal static class Util
    {
        internal static void AddRacialSelection(UnitEntityData unit, IList<BlueprintFeatureBase> features)
        {
            LevelUpController? controller = Kingmaker.Game.Instance?.LevelUpController;
            if (controller == null) { return; }
            if (controller.State.Mode == LevelUpState.CharBuildMode.Mythic) { return; }
            if (unit.Descriptor.Progression.CharacterLevel > 1) { return; }
            LevelUpHelper.AddFeaturesFromProgression(
                controller.State,
                unit,
                features,
                unit.Progression.Race,
                0);
        }
    }
}
