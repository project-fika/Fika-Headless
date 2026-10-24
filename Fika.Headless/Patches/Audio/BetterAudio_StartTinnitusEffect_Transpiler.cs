using Audio.Effects;
using SPTushonka.Reflection.Patching;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace Fika.Headless.Patches.Audio;

internal class BetterAudio_StartTinnitusEffect_Transpiler : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(TinnitusEffectController).GetMethod(nameof(TinnitusEffectController.StartTinnitusEffect));
    }

    [PatchPrefix]
    public static bool Prefix()
    {
        return false;
    }
}
