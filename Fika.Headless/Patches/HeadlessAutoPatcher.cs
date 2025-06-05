/*using Fika.Core.Patching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Fika.Headless.Patches
{
    public class HeadlessAutoPatcher
    {
        public static void EnableDestroyGraphicsPatches()
        {
            IEnumerable<Type> query = Assembly.GetExecutingAssembly().GetTypes().Where(t => t.BaseType == typeof(FikaPatch) && t.Namespace == "Fika.Headless.Patches.DestroyGraphics");

            FikaHeadlessPlugin.FikaHeadlessLogger.LogInfo("Autoloading patches in the DestroyGraphics namespace");
            int i = 0;

            foreach (Type patch in query)
            {
                if (patch.Name == "LoadScenePatch")
                {
                    if (!FikaHeadlessPlugin.DestroyRenderersOnSceneLoad.Value)
                    {
                        FikaHeadlessPlugin.FikaHeadlessLogger.LogInfo("DestroyRenderersOnSceneLoad is disabled! skipping LoadScenePatch");

                        continue;
                    }
                }

                ((FikaPatch)Activator.CreateInstance(patch)).Enable();
                i++;
            }

            FikaHeadlessPlugin.FikaHeadlessLogger.LogInfo($"{i} Patches enabled");
        }

        public static void EnableDisableAudioPatches()
        {
            IEnumerable<Type> query = Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(t => t.BaseType == typeof(FikaPatch) && t.Namespace == "Fika.Headless.Patches.Audio");

            FikaHeadlessPlugin.FikaHeadlessLogger.LogInfo("Autoloading patches in the Audio namespace");
            int i = 0;

            foreach (Type patch in query)
            {
                ((FikaPatch)Activator.CreateInstance(patch)).Enable();
                i++;
            }

            FikaHeadlessPlugin.FikaHeadlessLogger.LogInfo($"{i} Audio patches enabled");
        }
    }
}
*/