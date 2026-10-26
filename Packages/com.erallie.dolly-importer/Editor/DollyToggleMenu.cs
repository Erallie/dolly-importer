using UnityEditor;
using UnityEngine;

public static class DollyToggleMenu
{
    // Change this to the prefab's project-relative asset path.
    private const string DollyTogglePrefabPath =
        "Packages/com.erallie.dolly-importer/Prefabs/Dolly Toggle.prefab";

    [MenuItem("Tools/Erallie/Dolly Importer/Add Dolly Toggle to Scene", false, 1)]
    public static void AddDollyToggleToScene()
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(
            DollyTogglePrefabPath
        );

        if (prefab == null || !PrefabUtility.IsPartOfPrefabAsset(prefab))
        {
            EditorUtility.DisplayDialog(
                "Dolly Importer",
                "Could not find a prefab at:\n" + DollyTogglePrefabPath +
                "\n\nUpdate DollyTogglePrefabPath in DollyToggleMenu.cs.",
                "OK"
            );
            return;
        }

        GameObject instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
        if (instance == null)
        {
            return;
        }

        Undo.RegisterCreatedObjectUndo(instance, "Add Dolly Toggle to Scene");
        Selection.activeGameObject = instance;
        EditorGUIUtility.PingObject(instance);
    }
}