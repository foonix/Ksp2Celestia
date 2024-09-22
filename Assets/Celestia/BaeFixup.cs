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
            var baePicker = app.transform.parent.GetComponentInChildren<PartInfoOverlay>(true);
            app.PartInfoOverlay = baePicker;

            orig(app);
        }
    }
}
