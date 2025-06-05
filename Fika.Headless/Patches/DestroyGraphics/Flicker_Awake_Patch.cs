using EFT.Visual;
using Fika.Core.Patching;
using System.Reflection;
using UnityEngine;

namespace Fika.Headless.Patches.DestroyGraphics
{
    public class Flicker_Awake_Patch : FikaPatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(Flicker).GetMethod(nameof(Flicker.Awake));
        }

        [PatchPrefix]
        public static bool Prefix(Flicker __instance)
        {
            Object.Destroy(__instance);
            return false;
        }
    }
}
