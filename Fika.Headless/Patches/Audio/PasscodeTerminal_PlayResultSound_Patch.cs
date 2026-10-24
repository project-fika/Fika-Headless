using EFT.Passcodes;
using SPTushonka.Reflection.Patching;
using System.Reflection;

namespace Fika.Headless.Patches.Audio;

internal class PasscodeTerminal_PlayResultSound_Patch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(PasscodeTerminal).GetMethod(nameof(PasscodeTerminal.PlayResultSound));
    }

    [PatchPrefix]
    public static bool Prefix()
    {
        return false;
    }
}
