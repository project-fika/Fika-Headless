using EFT.GameTriggers;
using EFT.InventoryLogic;
using SPTushonka.Reflection.Patching;
using System.Reflection;
using Il2CppSystems.Effects;

namespace Fika.Headless.Patches.Audio;

internal class WeaponSoundPlayer_FireBullet_Patch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(WeaponSoundPlayer).GetMethod(nameof(WeaponSoundPlayer.FireBullet),
            [typeof(Ammo), typeof(Vector3), typeof(Vector3), typeof(bool)]);
    }

    [PatchPrefix]
    public static bool Prefix()
    {
        return false;
    }
}

internal class Effect_PlaySound_Patch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(Effects.Effect).GetMethod(nameof(Effects.Effect.PlaySound),
            [typeof(Vector3), typeof(float), typeof(float), typeof(bool), typeof(bool), typeof(bool)]);
    }

    [PatchPrefix]
    public static bool Prefix()
    {
        return false;
    }
}

internal class HandlerPlaySoundAdvanced_PlaySound_Patch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(HandlerPlaySoundAdvanced).GetMethod(nameof(HandlerPlaySoundAdvanced.PlaySound), [typeof(TriggerEvent)]);
    }

    [PatchPrefix]
    public static bool Prefix()
    {
        return false;
    }
}
