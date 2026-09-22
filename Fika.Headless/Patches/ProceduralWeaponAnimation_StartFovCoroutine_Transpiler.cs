using EFT;
using EFT.Animations;
using SPTushonka.Reflection.Patching;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace Fika.Headless.Patches;

/// <summary>
/// This patch aims to decrease the amount of CPU cycles spent updating data the headless does not see
/// </summary>
public class ProceduralWeaponAnimation_StartFovCoroutine_Transpiler : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(ProceduralWeaponAnimation).GetMethod(nameof(ProceduralWeaponAnimation.StartFovCoroutine), [typeof(Player)]);
    }

    [PatchPrefix]
    public static bool Prefix()
    {
        return false;
    }
}
