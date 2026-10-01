using System.IO;
using UnityEditor;
using UnityEngine;

public class UpdateREP2P
{
    public const string LOCAL_REP2P_PATH = "Assets/REP2P/";
    public const string REMOTE_REP2P_PATH = "D:\\UnityProjects\\Unity P2P Research\\Core\\REP2P\\REP2P.SteamworksNET\\bin\\Release\\netstandard2.1/";
    [MenuItem("Tools/Update/REP2P")]
    public static void PerformUpdate()
    {
        File.Copy(REMOTE_REP2P_PATH + "REP2P.SteamworksNET.dll", LOCAL_REP2P_PATH + "REP2P.SteamworksNET.dll", true);
        File.Copy(REMOTE_REP2P_PATH + "REP2P.dll", LOCAL_REP2P_PATH + "REP2P.dll", true);
        File.Copy(REMOTE_REP2P_PATH + "REP2P.xml", LOCAL_REP2P_PATH + "REP2P.xml", true);
        EditorUtility.RequestScriptReload();
    }
}
