using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class P2PScene : MonoBehaviour
{
    public static P2PScene instance { get; private set; }
    public P2PIdentity[] identities;

    private void Start()
    {
        instance = this;
        P2PNetworkManager.ResetScene();
    }
}
