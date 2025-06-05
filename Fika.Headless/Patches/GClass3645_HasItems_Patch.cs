using Fika.Core.Patching;
using System.Reflection;
using static EFT.UI.ScavengerInventoryScreen;

namespace Fika.Headless.Patches
{
    public class GClass3645_HasItems_Patch : FikaPatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(GClass3645).GetProperty(nameof(GClass3645.HasItems)).GetGetMethod();
        }

        [PatchPrefix]
        public static bool Prefix(ref bool __result)
        {
            __result = false;
            return false;
        }
    }
}
