using Fika.Core.Patching;
using System.Reflection;
using System.Threading.Tasks;

namespace Fika.Headless.Patches.Locales
{
    public class Class1417_LoadMainMenuLocale_Patch : FikaPatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(Class1417).GetMethod(nameof(Class1417.LoadMainMenuLocale));
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
