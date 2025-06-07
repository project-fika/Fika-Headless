using Fika.Core.Patching;
using MultiFlare;
using System.Reflection;
using UnityEngine;

namespace Fika.Headless.Patches.DestroyGraphics
{
    internal class FlareSceneSettings_Awake_Patch : FikaPatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(FlareSceneSettings).GetMethod(nameof(FlareSceneSettings.Awake));
        }

        [PatchPrefix]
        public static bool Prefix(FlareSceneSettings __instance)
        {
            MonoBehaviour.Destroy(__instance);
            return false;
        }
    }
}
