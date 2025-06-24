using Fika.Core.Patching;
using System.Reflection;

namespace Fika.Headless.Patches
{
    /// <summary>
    /// Target the IsReflexAvailable() method with no parameters
    /// </summary>
    public class IsReflexAvailablePatch : FikaPatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(GClass3488).GetMethod(nameof(GClass3488.IsReflexAvailable),
                BindingFlags.Public | BindingFlags.Static,
                null, [], null);
        }

        [PatchPrefix]
        public static bool Prefix(ref bool __result)
        {
            __result = false;
            return false;
        }
    }
}
