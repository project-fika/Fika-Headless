using EFT;
using Fika.Core.Patching;
using System.Reflection;
using System.Threading.Tasks;

namespace Fika.Headless.Patches.GameMode
{
    internal class TarkovApplication_method_53_Patch : FikaPatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(TarkovApplication).GetMethod(nameof(TarkovApplication.method_53));
        }

        [PatchPrefix]
        public static bool Prefix(ref Task __result, TarkovApplication __instance)
        {
            __result = __instance.method_54();
            return false;
        }
    }
}
