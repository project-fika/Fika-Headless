using SPTushonka.Reflection.Patching;
using System.Reflection;

namespace Fika.Headless.Patches.PlayerPatches;

internal class Player_PlayPrewarmSound_Transpiler : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(AudioSourcePrewarmer)
            .GetMethod(nameof(AudioSourcePrewarmer.PlayPrewarmSound), [typeof(BetterSource)]);
    }

    [PatchPrefix]
    public static bool Prefix()
    {
        return false;
    }
}
