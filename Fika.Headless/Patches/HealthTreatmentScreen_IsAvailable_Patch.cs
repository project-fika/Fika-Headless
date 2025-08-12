using EFT.UI.SessionEnd;
using Fika.Core.Patching;
using System.Reflection;

namespace Fika.Headless.Patches;

public class HealthTreatmentScreen_IsAvailable_Patch : FikaPatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(HealthTreatmentScreen).GetMethod(nameof(HealthTreatmentScreen.IsAvailable), BindingFlags.Public | BindingFlags.Static);
    }

    [PatchPrefix]
    public static bool PatchPrefix(ref bool __result)
    {
        __result = false;
        return false;
    }
}
