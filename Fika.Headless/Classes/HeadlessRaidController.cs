using UnityEngine;

namespace Fika.Headless.Classes
{
    public class HeadlessRaidController : MonoBehaviour
    {
        protected void Update()
        {
            Physics.SyncTransforms();
        }
    }
}
