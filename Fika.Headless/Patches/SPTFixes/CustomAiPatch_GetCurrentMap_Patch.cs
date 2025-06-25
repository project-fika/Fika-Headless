using Comfort.Common;
using EFT;
using Fika.Core.Patching;
using SPT.Custom.Patches;
using System.Reflection;

namespace Fika.Headless.Patches.SPTFixes
{
    internal class CustomAiPatch_GetCurrentMap_Patch : FikaPatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(CustomAiPatch)
                .GetMethod("GetCurrentMap", BindingFlags.NonPublic | BindingFlags.Static);
        }

        [PatchPrefix]
        public static bool Prefix(ref string __result)
        {
            __result = Singleton<GameWorld>.Instance.LocationId;
            return false;
        }
    }
}
