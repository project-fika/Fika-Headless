using SPT.Core.Utils;
using SPT.Reflection.Patching;
using System.Reflection;

namespace Fika.Headless.Patches;

internal class LocaleFixPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(ValidationUtil)
            .GetMethod(nameof(ValidationUtil.Validate));
    }

    [PatchPrefix]
    public static bool Prefix(bool ____hasRun)
    {
        ____hasRun = true;
        ValidationUtil._crashHandler = "0";
        return false;
    }
}
