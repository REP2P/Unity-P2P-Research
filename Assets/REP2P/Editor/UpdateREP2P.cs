using System.IO;
using UnityEditor;
using UnityEngine;

public class UpdateREP2P
{
    public const string LOCAL_REP2P_PATH = "Assets/REP2P/";
    public const string REMOTE_REP2P_PATH = "D:\\UnityProjects\\Unity P2P Research\\Core\\REP2P\\REP2P.SteamworksNET\\bin\\Release\\netstandard2.1/";
    const string REP2Pdll = "REP2P.dll";
    const string REP2Pxml = "REP2P.xml";
    const string REP2PSteamworksNETdll = "REP2P.SteamworksNET.dll";
    const string REP2PFacepunchSteamworksdll = "REP2P.Facepunch.Steamworks.dll";
    [MenuItem("Tools/Update/REP2P")]
    public static void PerformUpdate()
    {
        File.Copy(REMOTE_REP2P_PATH + REP2Pdll, LOCAL_REP2P_PATH + REP2Pdll, true);
        File.Copy(REMOTE_REP2P_PATH + REP2Pxml, LOCAL_REP2P_PATH + REP2Pxml, true);
        EditorUtility.RequestScriptReload();
    }
    [MenuItem("Tools/Update/REP2P Extras/Steamworks.NET")]
    public static void PerformSteamworksNETUpdate()
    {
        File.Copy(REMOTE_REP2P_PATH + REP2PSteamworksNETdll, LOCAL_REP2P_PATH + REP2PSteamworksNETdll, true);
        EditorUtility.RequestScriptReload();
    }
    [MenuItem("Tools/Update/REP2P Extras/Facepunch.Steamworks")]
    public static void PerformFacepunchSteamworksUpdate()
    {
        File.Copy(REMOTE_REP2P_PATH + REP2PFacepunchSteamworksdll, LOCAL_REP2P_PATH + REP2PFacepunchSteamworksdll, true);
        EditorUtility.RequestScriptReload();
    }
}
