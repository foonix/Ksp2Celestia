using BepInEx;
using MonoMod.RuntimeDetour;
using System.Collections.Generic;

namespace Celestia
{
    [BepInPlugin("Celestia", "Celestia", "0.0.1")]
    public class CelestiaPlugin : BaseUnityPlugin
    {
        bool baeFixupsApplied = false;

        private List<IDetour> hooks = new();

        private void Start()
        {
            hooks.AddRange(BaeFixup.MakeHooks());
        }
    }
}