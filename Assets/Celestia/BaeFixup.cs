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

        private static void InjectBaeDepsOnStart(Action<AssemblyPartsPicker> orig, AssemblyPartsPicker baeApp)
        {
            // Fix missing link to PartInfoOverlay, which is a sibling in the OAB prefab.
            var partInfoOverlay = baeApp.transform.parent.GetComponentInChildren<PartInfoOverlay>(true);
            baeApp.PartInfoOverlay = partInfoOverlay;

            // Fix missing reference to size (S, M, L, XL, etc) color mapping by grabbing it from the OAB version.
            var oabPicker = baeApp.transform.parent.GetComponentsInChildren<AssemblyPartsPicker>(true)
                .Where(child => child.name == "widget_PartsPicker")
                .First();
            baeApp.filterColors = oabPicker.filterColors;

            // Fix orbital OAB environment lights being on layer 8 (OAB.Scenery) and not 9 (OAB.Parts).
            // Without some lights on layer 9, the parts are completely unlit.
            foreach (var light in baeApp.transform.root.GetComponentsInChildren<Light>())
            {
                light.cullingMask |= LayerMask.NameToLayer("OAB.Parts");
            }

            orig(baeApp);
        }
    }
}
