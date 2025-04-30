#if UNITY_EDITOR
using UnityEditor;

[InitializeOnLoad]
public static class SpatialDefineEnabler
{
    static SpatialDefineEnabler()
    {
        string symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
        if (!symbols.Contains("VBO_Spatial"))
        {
            PlayerSettings.SetScriptingDefineSymbolsForGroup(
                EditorUserBuildSettings.selectedBuildTargetGroup,
                symbols + ";VBO_Spatial"
            );
        }
    }
}
#endif