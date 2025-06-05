using EFT.UI.Settings;
using Fika.Core.Patching;
using System.Reflection;

namespace Fika.Headless.Patches.VRAM
{
    // Token: 0x0200000C RID: 12
    public class VRAMPatch4 : FikaPatch
    {
        // Token: 0x06000023 RID: 35 RVA: 0x00002500 File Offset: 0x00000700
        protected override MethodBase GetTargetMethod()
        {
            return typeof(GraphicsSettingsTab).GetMethod("UpdateRecommendedVRAMLabel");
        }

        // Token: 0x06000024 RID: 36 RVA: 0x00002528 File Offset: 0x00000728
        [PatchPrefix]
        public static bool Prefix()
        {
            return false;
        }
    }
}
