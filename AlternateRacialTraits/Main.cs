using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;

using MicroWrath;
using MicroWrath.Constructors;

using UniRx;

using UnityEngine;

using UnityModManagerNet;

namespace AlternateRacialTraits;
partial class Main
{
    internal bool AHTCompat = false;
    private IDisposable? updateSubscription;
    void OnGUI(UnityModManager.ModEntry modEntry)
    {
        GUILayout.BeginVertical();
        {
            GUILayout.Label("Enable this option if a save created with Alternate Human Traits fails to load, then load the save and respec the character.");
            if (this.AHTCompat != GUILayout.Toggle(this.AHTCompat, "Upgrade from Alternate Human Traits"))
            {
                var useSelection = this.AHTCompat = !this.AHTCompat;

                MicroLogger.Debug(() => $"AHT Compat toggled to {useSelection}");

                updateSubscription?.Dispose();

                updateSubscription =
                    Triggers.BlueprintLoad_Prefix
                        .Where(guid => guid == GeneratedGuid.RemovedHumanBonusFeatSelection.Guid)
                        .Take(1)
                        .Subscribe(guid =>
                        {
                            if (ResourcesLibrary.TryGetBlueprint(guid) is not null)
                                ResourcesLibrary.BlueprintsCache.RemoveCachedBlueprint(guid);

                            MicroLogger.Debug(() => $"Adding blueprint for AHT compat = {useSelection}");

                            SimpleBlueprint newBlueprint = useSelection ?
                                Construct.New.Blueprint<BlueprintFeatureSelection>(GeneratedGuid.RemovedHumanBonusFeatSelection) :
                                Construct.New.Blueprint<BlueprintFeature>(GeneratedGuid.RemovedHumanBonusFeatSelection);

                            _ = ResourcesLibrary.BlueprintsCache.AddCachedBlueprint(guid, newBlueprint);
                        });
            }
        }
        GUILayout.EndVertical();
    }

    [Init]
    static void Init()
    {
        if (Main.Instance.ModEntry is null)
            return;

        Main.Instance.ModEntry.OnGUI = ((Main)Main.Instance).OnGUI;
    }
}
