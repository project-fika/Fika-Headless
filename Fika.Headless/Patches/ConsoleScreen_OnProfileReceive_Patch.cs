using EFT;
using EFT.UI;
using SPTushonka.Custom.Utils;
using SPTushonka.Reflection.Patching;
using System.Reflection;
using System.Threading.Tasks;

namespace Fika.Headless.Patches;

public class ConsoleScreen_OnProfileReceive_Patch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(TarkovApplication)
            .GetMethod(nameof(TarkovApplication.RunProfile));
    }

    [PatchPrefix]
    public static void Prefix(TarkovApplication __instance)
    {
        var profile = __instance.Session.Profile;
        if (!profile.Nickname.Contains("headless_"))
        {
            if (SystemInfo.operatingSystemFamily == OperatingSystemFamily.Windows)
            {
                MessageBoxHelper.Show(
                    "You are not starting with a headless profile!\nRead the documentation again!\n\nIf you are not planning to run the headless client with this instance of EFT, remove 'Fika.Headless.dll' immediately!",
                    "FIKA ERROR",
                    MessageBoxHelper.MessageBoxType.OK);
            }
            else
            {
                FikaHeadlessPlugin.FikaHeadlessLogger.LogError("Not running a headless client profile! Read the documentation again! Exiting...");
            }
            Application.Quit();
        }
        else
        {
            FikaHeadlessPlugin.FikaHeadlessLogger.LogInfo("Profile verified");
            FikaHeadlessPlugin.Instance.CanHost = true;
            FikaHeadlessPlugin.Instance.OnReady();

            _ = FikaHeadlessPlugin.Instance.RunPluginValidation();
        }
    }
}
