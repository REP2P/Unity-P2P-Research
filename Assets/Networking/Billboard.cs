using UnityEngine;

namespace Networking
{
    public class Billboard : MonoBehaviour
    {
        void Update()
        {
            if (Networker.Instance) {
                if (Networker.Instance.localPlayerTransform) {
                    transform.LookAt(Networker.Instance.localPlayerTransform);
                }
            }
        }
    }
}
