using Comfort.Common;
using EFT;
using EFT.Airdrop;
using SPT.Reflection.Patching;
using System.Reflection;

namespace Fika.Headless.Patches.Airdrop;

/// <summary>
/// Prevents a nullref on headless due to having no player
/// </summary>
public class ServerAirdropManager_FlareSuccessEventHandler_Patch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(ServerAirdropManager)
            .GetMethod(nameof(ServerAirdropManager.FlareSuccessEventHandler));
    }

    [PatchPrefix]
    public static bool Prefix(ServerAirdropManager __instance, string profileId, Vector3 position, string lootTemplateId)
    {
        if (__instance._spawnAirdropTimer.Time + (float)__instance.PlaneAirdropFlareWait > __instance._nextAirdropTimeByFlare && __instance._projectileSuccessPositions.Count == 0)
        {
            __instance._nextAirdropTimeByFlare = __instance._spawnAirdropTimer.Time + (float)__instance.PlaneAirdropFlareWait;
        }
        __instance._projectileSuccessPositions.Add(position);
        __instance._overrideLootTemplateId = lootTemplateId;
        IAirdropDataSender airdropDataSender = Singleton<GameWorld>.Instance.SynchronizableObjectLogicProcessor.AirdropDataSender;
        if (airdropDataSender != null)
        {
            airdropDataSender.SendFlareSuccessEvent(profileId, __instance._spawnAirdropTimer.Time + (float)__instance.PlaneAirdropFlareWait >= __instance._nextAirdropTimeByFlare);
        }

        return false;
    }
}
