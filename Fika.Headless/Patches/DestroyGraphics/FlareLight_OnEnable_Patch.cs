using Fika.Core.Patching;
using MultiFlare;
using System.Reflection;
using UnityEngine;

namespace Fika.Headless.Patches.DestroyGraphics
{
    internal class FlareLight_OnEnable_Patch : FikaPatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(FlareLight).GetMethod(nameof(FlareLight.OnEnable));
        }

        [PatchPrefix]
        public static bool Prefix(FlareLight __instance)
        {
            MonoBehaviour.Destroy(__instance);
            return false;
        }
    }
}
