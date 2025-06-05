using Fika.Core.Patching;
using System.Reflection;
using UnityEngine;

namespace Fika.Headless.Patches.DestroyGraphics
{
    public class AmbientLight_Start_Patch : FikaPatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(AmbientLight).GetMethod(nameof(AmbientLight.Start));
        }

        [PatchPrefix]
        public static bool Prefix(AmbientLight __instance)
        {
            Object.Destroy(__instance);
            return false;
        }
    }
}
