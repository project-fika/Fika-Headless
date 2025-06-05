using EFT.Impostors;
using Fika.Core.Patching;
using System.Reflection;
using UnityEngine;

namespace Fika.Headless.Patches.DestroyGraphics
{
    /// <summary>
    /// This patch mostly deals with deleting the tree renderer.
    /// </summary>
    public class ImpostorsRenderer_OnEnable_Patch : FikaPatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(ImpostorsRenderer).GetMethod(nameof(ImpostorsRenderer.OnEnable)); // This class is pretty sussy
        }

        [PatchPrefix]
        public static bool Prefix(ImpostorsRenderer __instance)
        {
            Object.Destroy(__instance);
            return false;
        }
    }
}
