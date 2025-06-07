using Fika.Core.Patching;
using System.Reflection;
using UnityEngine;

namespace Fika.Headless.Patches.DestroyGraphics
{
    internal class SingleFlareController_OnEnable_Patch : FikaPatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(SingleFlareController).GetMethod(nameof(SingleFlareController.OnEnable));
        }

        [PatchPrefix]
        public static bool Prefix(SingleFlareController __instance)
        {
            MonoBehaviour.Destroy(__instance);
            return false;
        }
    }
}