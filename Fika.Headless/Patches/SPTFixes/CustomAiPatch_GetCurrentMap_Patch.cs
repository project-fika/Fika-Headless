using Comfort.Common;
using Fika.Core.Coop.GameMode;
using Fika.Core.Patching;
using SPT.Custom.Patches;
using System.Reflection;

namespace Fika.Headless.Patches.SPTFixes
{
    internal class CustomAiPatch_GetCurrentMap_Patch : FikaPatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(CustomAiPatch).GetMethod("GetCurrentMap", BindingFlags.NonPublic | BindingFlags.Static);
        }

        [PatchPrefix]
        public static bool Prefix(ref string __result)
        {
            __result = Singleton<IFikaGame>.Instance.GameController.Location.Id;
            return false;
        }
    }
}
