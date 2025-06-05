using Fika.Core.Patching;
using System.Reflection;

namespace Fika.Headless.Patches
{
    internal class LocaleManagerClass_String_0_Patch : FikaPatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(LocaleManagerClass).GetProperty(nameof(LocaleManagerClass.String_0)).GetSetMethod();
        }

        [PatchPrefix]
        public static bool Prefix(ref string ___String_2)
        {
            Logger.LogInfo("Forcing 'en' language");
            ___String_2 = "en";
            return false;
        }
    }
}
