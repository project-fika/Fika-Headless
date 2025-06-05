using Fika.Core.Patching;
using System.Reflection;
using UnityEngine;

namespace Fika.Headless.Patches.DestroyGraphics
{
    public class StaticDeferredDecal_OnEnable_Patch : FikaPatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(StaticDeferredDecal).GetMethod(nameof(StaticDeferredDecal.OnEnable));
        }

        [PatchPrefix]
        public static bool Prefix(StaticDeferredDecal __instance)
        {
            Object.Destroy(__instance.gameObject);
            Object.Destroy(__instance);
            return false;
        }
    }
}
