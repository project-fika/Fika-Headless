using EFT;
using SPTushonka.Reflection.Patching;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace Fika.Headless.Patches.PlayerPatches;

internal class Player_PlayGearSound_Transpiler : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(Player)
            .GetMethod(nameof(Player.PlayGearSound), [typeof(SoundBank), typeof(float)]);
    }

    [PatchPrefix]
    public static bool Prefix()
    {
        return false;
    }
}
