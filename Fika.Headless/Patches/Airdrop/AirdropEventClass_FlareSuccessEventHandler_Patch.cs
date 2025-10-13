using Comfort.Common;
using EFT;
using SPT.Reflection.Patching;
using System.Reflection;

namespace Fika.Headless.Patches.Airdrop;

/// <summary>
/// Prevents a nullref on headless due to having no player
/// </summary>
public class AirdropEventClass_FlareSuccessEventHandler_Patch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(AirdropEventClass)
            .GetMethod(nameof(AirdropEventClass.FlareSuccessEventHandler));
    }

    [PatchPrefix]
    public static bool Prefix(AirdropEventClass __instance, string profileId, Vector3 position, string lootTemplateId)
    {
        if (__instance.TimerClass.Time + (float)__instance.Int32_2 > __instance.Float_2 && __instance.List_2.Count == 0)
        {
            __instance.Float_2 = __instance.TimerClass.Time + (float)__instance.Int32_2;
        }
        __instance.List_2.Add(position);
        __instance.String_0 = lootTemplateId;
        GInterface279 ginterface279_ = Singleton<GameWorld>.Instance.SynchronizableObjectLogicProcessor.Ginterface279_0;
        if (ginterface279_ != null)
        {
            ginterface279_.SendFlareSuccessEvent(profileId, __instance.TimerClass.Time + (float)__instance.Int32_2 >= __instance.Float_2);
        }

        return false;
    }
}
