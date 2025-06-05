using Fika.Core.Patching;
using System.Reflection;
using UnityEngine;

namespace Fika.Headless.Patches.DestroyGraphics
{
    public class StaticDeferredDecalRenderer_Awake_Patch : FikaPatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(StaticDeferredDecalRenderer).GetMethod(nameof(StaticDeferredDecalRenderer.Awake));
        }

        [PatchPrefix]
        public static bool Prefix(StaticDeferredDecalRenderer __instance)
        {
            Object.Destroy(__instance);
            return false;
        }
    }
}
