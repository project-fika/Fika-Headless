using Fika.Core.Patching;
using System.Reflection;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace Fika.Headless.Patches.TextureValidateFormat
{
    /// <summary>
    /// <see href="https://github.com/Unity-Technologies/UnityCsReference/blob/77b37cd9f002e27b45be07d6e3667ee53985ec82/Runtime/Export/Graphics/Texture.cs#L730"/>
    /// </summary>
    public class ValidateFormatPatch3 : FikaPatch
    {
        protected override MethodBase GetTargetMethod()
        {
            MethodInfo[] methods = typeof(Texture).GetMethods(BindingFlags.NonPublic | BindingFlags.Instance);

            foreach (MethodInfo method in methods)
            {
                if (method.Name == "ValidateFormat" && method.GetParameters().Length == 2 && method.GetParameters()[0].ParameterType == typeof(GraphicsFormat))
                {
                    return method;
                }
            }

            return null;
        }

        [PatchPostfix]
        public static void Postfix(ref bool __result)
        {
            __result = true;
        }
    }
}