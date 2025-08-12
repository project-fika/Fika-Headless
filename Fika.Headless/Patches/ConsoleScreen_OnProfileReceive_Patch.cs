using EFT;
using EFT.UI;
using Fika.Core.Patching;
using SPT.Core.Utils;
using SPT.Custom.Utils;
using System.Reflection;
using System.Threading.Tasks;

namespace Fika.Headless.Patches;

public class ConsoleScreen_OnProfileReceive_Patch : FikaPatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(ConsoleScreen).GetMethod(nameof(ConsoleScreen.OnProfileReceive));
    }

    [PatchPostfix]
    public static void Prefix(Profile profile)
    {
        if (!profile.Nickname.Contains("headless_"))
        {
            if (SystemInfo.operatingSystemFamily == OperatingSystemFamily.Windows)
            {
                MessageBoxHelper.Show(
                    $"You are not starting with a headless profile!\nRead the documentation again!\n\nIf you are not planning to run the headless client with this instance of EFT, remove 'Fika.Headless.dll' immediately!",
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
            ValidationUtil._crashHandler = "FikaHeadless";
            FikaHeadlessPlugin.FikaHeadlessLogger.LogInfo("Profile verified");
            FikaHeadlessPlugin.Instance.CanHost = true;

            _ = Task.Run(FikaHeadlessPlugin.Instance.RunPluginValidation);
        }
    }
}
