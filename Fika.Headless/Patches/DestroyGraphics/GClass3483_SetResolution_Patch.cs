using Fika.Core.Patching;
using System.Reflection;

namespace Fika.Headless.Patches.DestroyGraphics
{
    public class GClass3483_SetResolution_Patch : FikaPatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(GClass3483).GetMethod(nameof(GClass3483.SetResolution));
        }

        [PatchPrefix]
        public static bool Prefix()
        {
            return false;
        }
    }
}
