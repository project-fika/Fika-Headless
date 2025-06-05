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
            return typeof(Class438).GetMethod(nameof(Class438.Run));
        }

        [PatchPrefix]
        public static bool Prefix(Class438 __instance, ref Task __result, ref Class438.Interface3 ___Interface3_0)
        {
            ___Interface3_0 = new Class438.Class448(__instance);
            __result = Task.CompletedTask;
            return false;
        }
    }
}
