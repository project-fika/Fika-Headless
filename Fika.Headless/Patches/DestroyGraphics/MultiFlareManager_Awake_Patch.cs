using SPT.Reflection.Patching;
using System.Reflection;
using UnityEngine;

namespace Fika.Headless.Patches.DestroyGraphics
{
    public class MultiFlareManager_Awake_Patch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(MultiFlareManager).GetMethod(nameof(MultiFlareManager.Awake));
        }

        [PatchPrefix]
        public static bool Prefix(MultiFlareManager __instance)
        {
            GameObject.Destroy(__instance);
            return false;
        }
    }
}
