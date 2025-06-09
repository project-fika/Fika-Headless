using Fika.Core.Patching;
using System.Reflection;

namespace Fika.Headless.Patches.Locales
{
    public class LocaleManagerClass_method_4_Patch : FikaPatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(LocaleManagerClass).GetMethod(nameof(LocaleManagerClass.method_4));
        }

        [PatchPrefix]
        public static bool Prefix(ref string __result)
        {
            __result = string.Empty;
            return false;
        }
    }
}
