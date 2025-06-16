using Fika.Core.Patching;
using GPUInstancer;
using System.Reflection;
using UnityEngine;

namespace Fika.Headless.Patches.DestroyGraphics
{
    public class GPUInstancerHiZOcclusionGenerator_Awake_Patch : FikaPatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(GPUInstancerHiZOcclusionGenerator).GetMethod(nameof(GPUInstancerHiZOcclusionGenerator.Awake));
        }

        [PatchPrefix]
        public static bool Prefix(GPUInstancerHiZOcclusionGenerator __instance)
        {
            Object.Destroy(__instance);
            return false;
        }
    }
}
