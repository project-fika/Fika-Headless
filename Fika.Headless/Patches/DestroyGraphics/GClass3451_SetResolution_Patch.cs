using SPT.Reflection.Patching;
using System.Reflection;

namespace Fika.Headless.Patches.DestroyGraphics
{
    public class GClass3451_SetResolution_Patch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(GClass3451).GetMethod(nameof(GClass3451.SetResolution));
        }

        [PatchPrefix]
        public static bool Prefix()
        {
            return false;
        }
    }
}
