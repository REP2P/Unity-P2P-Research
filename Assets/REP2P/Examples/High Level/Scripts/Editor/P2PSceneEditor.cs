using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(P2PScene))]
public class P2PSceneEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (GUILayout.Button("Bake"))
        {
            EditorUtility.SetDirty(target);
            P2PScene scene = (P2PScene)target;
            scene.identities = FindObjectsByType<P2PIdentity>(FindObjectsInactive.Include, FindObjectsSortMode.InstanceID);
            AssetDatabase.SaveAssetIfDirty(scene);
        }
    }
}
