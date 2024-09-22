using KSP.OAB;
using KSP.UI;
using MonoMod.RuntimeDetour;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Celestia
{
    public class BaeFixup
    {

        internal static List<IDetour> MakeHooks()
        {
            return new()
            {
                new Hook(
                    typeof(AssemblyPartsPicker).GetMethod("Start", BindingFlags.NonPublic | BindingFlags.Instance),
                    (Action<Action<AssemblyPartsPicker>, AssemblyPartsPicker>)InjectBaeDepsOnStart
                ),
            };
        }

        private static void InjectBaeDepsOnStart(Action<AssemblyPartsPicker> orig, AssemblyPartsPicker app)
        {
            // Fix missing link to PartInfoOverlay, which is a sibling in the OAB prefab.
            var partInfoOverlay = app.transform.parent.GetComponentInChildren<PartInfoOverlay>(true);
            app.PartInfoOverlay = partInfoOverlay;

            // Fix missing reference to size (S, M, L, XL, etc) color mapping by grabbing it from the OAB version.
            var oabPicker = app.transform.parent.GetComponentsInChildren<AssemblyPartsPicker>(true)
                .Where(child => child.name == "widget_PartsPicker")
                .First();
            app.filterColors = oabPicker.filterColors;

            orig(app);
        }
    }
}
