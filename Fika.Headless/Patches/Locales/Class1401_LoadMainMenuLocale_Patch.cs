using Fika.Core.Patching;
using System.Reflection;
using System.Threading.Tasks;

namespace Fika.Headless.Patches.Locales
{
    public class Class1401_LoadMainMenuLocale_Patch : FikaPatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(Class1401).GetMethod(nameof(Class1401.LoadMainMenuLocale));
        }

        [PatchPrefix]
        public static bool Prefix(ref Task __result)
        {
            LocaleManagerClass.LocaleManagerClass.UpdateApplicationLanguage();
            __result = Task.CompletedTask;
            return false;
        }
    }
}
