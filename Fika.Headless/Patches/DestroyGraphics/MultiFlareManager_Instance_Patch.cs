using SPT.Reflection.Patching;
using System.Reflection;

namespace Fika.Headless.Patches.DestroyGraphics
{
    public class MultiFlareManager_Instance_Patch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(MultiFlareManager).GetMethod(nameof(MultiFlareManager.smethod_0));
        }

        [PatchPrefix]
        public static bool Prefix(ref MultiFlareManager __result)
        {
            __result = null;
            return false;
        }
    }
}
