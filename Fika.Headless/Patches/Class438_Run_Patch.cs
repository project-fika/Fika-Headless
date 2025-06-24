using Fika.Core.Patching;
using System.Reflection;
using System.Threading.Tasks;

namespace Fika.Headless.Patches
{
    /// <summary>
    /// This prevents the season controller from running due to no graphics being used
    /// </summary>
    public class Class438_Run_Patch : FikaPatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(Class445).GetMethod(nameof(Class445.Run));
        }

        [PatchPrefix]
        public static bool Prefix(Class445 __instance, ref Task __result, ref Class445.Interface3 ___Interface3_0)
        {
            ___Interface3_0 = new Class445.Class448(__instance);
            __result = Task.CompletedTask;
            return false;
        }
    }
}
