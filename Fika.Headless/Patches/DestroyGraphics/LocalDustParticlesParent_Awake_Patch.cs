using Fika.Core.Patching;
using System.Reflection;
using UnityEngine;

namespace Fika.Headless.Patches.DestroyGraphics
{
    public class LocalDustParticlesParent_Awake_Patch : FikaPatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(LocalDustParticlesParent).GetMethod(nameof(LocalDustParticlesParent.Awake));
        }

        [PatchPrefix]
        public static bool Prefix(LocalDustParticlesParent __instance)
        {
            GameObject.Destroy(__instance);
            return false;
        }
    }
}
