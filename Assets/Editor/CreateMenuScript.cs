using UnityEditor;

public static class CreateMenuScript
{
    [MenuItem("Assets/Create Script/NetworkBehaviour", priority = 0)]
    public static void CreateNetworkBehaviour()
    {
        string templatepath = "Assets/Editor/Templates/NetworkBehaviourTemplate.txt";
        ProjectWindowUtil.CreateScriptAssetFromTemplateFile(templatepath, "NewNetworkBehaviour.cs");
    }

    [MenuItem("Assets/Create Script/Interface", priority = 0)]
    public static void CreateInterface()
    {
        string templatepath = "Assets/Editor/Templates/Interface.txt";
        ProjectWindowUtil.CreateScriptAssetFromTemplateFile(templatepath, "IInterfaceable.cs");
    }
}