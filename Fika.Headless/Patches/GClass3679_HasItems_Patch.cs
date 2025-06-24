using Fika.Core.Patching;
using System.Reflection;
using static EFT.UI.ScavengerInventoryScreen;

namespace Fika.Headless.Patches
{
    public class GClass3679_HasItems_Patch : FikaPatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(GClass3679)
                .GetProperty(nameof(GClass3679.HasItems))
                .GetGetMethod();
        }

        [PatchPrefix]
        public static bool Prefix(ref bool __result)
        {
            __result = false;
            return false;
        }
    }
}
