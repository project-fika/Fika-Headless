using Fika.Core.Patching;
using System.Reflection;
using UnityEngine;

namespace Fika.Headless.Patches.PhysicsPatches
{
    /// <summary>
    /// This patch syncs all transforms before a bullet checks if it hits
    /// </summary>
    internal class EftBulletClass_method_14_Patch : FikaPatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(EftBulletClass).GetMethod(nameof(EftBulletClass.method_14));
        }

        [PatchPrefix]
        public static void Prefix()
        {
            Physics.SyncTransforms();
        }
    }
}
