using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class DollyImporter : EditorWindow
{
    private const string PackageRoot = "Packages/com.erallie.dolly-importer";
    private const string GitHubUrl = "https://github.com/Erallie/dolly-importer";

    private GameObject targetDolly;
    private string jsonPath = "";

    [MenuItem("Tools/Erallie/Dolly Importer")]
    public static void ShowWindow()
    {
        GetWindow<DollyImporter>("Dolly Importer");
    }

    private void OnGUI()
    {
        GUILayout.Label("VRChat Dolly JSON Importer", EditorStyles.boldLabel);

        EditorGUILayout.Space();

        EditorGUILayout.HelpBox(
            "FIRST TIME IMPORTING A DOLLY:\n" +
            "Leave Target Dolly blank. The importer will automatically add the " +
            "starting Dolly prefab to your scene and import the JSON into it.",
            MessageType.Info
        );

        EditorGUILayout.Space();

        EditorGUILayout.HelpBox(
            "REIMPORTING / UPDATING A DOLLY:\n" +
            "Select your existing Dolly as the Target Dolly.\n\n" +
            "Do not remove the first Path, the first Point inside that Path, or the " +
            "VRChat Dolly Path/Point scripts attached to them. The importer uses " +
            "these as templates when rebuilding the Dolly.",
            MessageType.Warning
        );

        EditorGUILayout.Space();

        targetDolly = (GameObject)EditorGUILayout.ObjectField(
            "Target Dolly",
            targetDolly,
            typeof(GameObject),
            true
        );

        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();

        EditorGUILayout.TextField("JSON File", jsonPath);

        if (GUILayout.Button("Browse...", GUILayout.Width(90)))
        {
            string selectedPath = EditorUtility.OpenFilePanel(
                "Select VRChat Dolly JSON",
                "",
                "json"
            );

            if (!string.IsNullOrEmpty(selectedPath))
            {
                jsonPath = selectedPath;
            }
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        EditorGUI.BeginDisabledGroup(
            string.IsNullOrEmpty(jsonPath) ||
            !File.Exists(jsonPath)
        );

        if (GUILayout.Button("Import / Replace Dolly", GUILayout.Height(35)))
        {
            ImportDolly();
        }

        EditorGUI.EndDisabledGroup();

        EditorGUILayout.Space();

        EditorGUILayout.HelpBox(
            "AFTER IMPORTING:\n" +
            "Click \"Collect Paths & Points\" on the Camera Dolly Animation component. " +
            "Then edit the Dolly's animation settings however you like, such as its " +
            "path type, loop behavior, speed settings, focus settings, and other " +
            "Camera Dolly Animation options.",
            MessageType.Info
        );

        EditorGUILayout.Space();

        GUILayout.FlexibleSpace();

        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();

        if (EditorGUILayout.LinkButton("View on GitHub"))
        {
            Application.OpenURL("https://github.com/Erallie/dolly-importer");
        }

        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();
    }

    private void ImportDolly()
    {
        GameObject createdDolly = null;
        bool firstImport = targetDolly == null;

        try
        {
            // ============================================================
            // FIRST IMPORT - LOAD STARTING PREFAB
            // ============================================================

            if (firstImport)
            {
                GameObject startingPrefab = FindStartingPrefab();

                if (startingPrefab == null)
                {
                    ShowModifiedPrefabError();
                    return;
                }

                if (!ValidateDollyHierarchy(startingPrefab))
                {
                    ShowModifiedPrefabError();
                    return;
                }

                createdDolly = PrefabUtility.InstantiatePrefab(
                    startingPrefab
                ) as GameObject;

                if (createdDolly == null)
                {
                    ShowModifiedPrefabError();
                    return;
                }

                Undo.RegisterCreatedObjectUndo(
                    createdDolly,
                    "Create Camera Dolly"
                );

                targetDolly = createdDolly;

                Selection.activeGameObject = targetDolly;
            }


            // ============================================================
            // VALIDATE TARGET
            // ============================================================

            if (!ValidateDollyHierarchy(targetDolly))
            {
                if (firstImport)
                {
                    if (createdDolly != null)
                    {
                        Undo.DestroyObjectImmediate(createdDolly);
                        targetDolly = null;
                    }

                    ShowModifiedPrefabError();
                }
                else
                {
                    ShowExistingDollyError();
                }

                return;
            }


            // ============================================================
            // GET TEMPLATE HIERARCHY
            // ============================================================

            Transform originalPathTemplate =
                targetDolly.transform.GetChild(0);

            Transform originalPointTemplate =
                originalPathTemplate.GetChild(0);


            // ============================================================
            // READ JSON
            // ============================================================

            string json = File.ReadAllText(jsonPath);

            // VRChat exports the Dolly as a raw JSON array.
            // JsonUtility requires an object at the root, so wrap it.
            string wrappedJson =
                "{\"Points\":" + json + "}";

            DollyJsonWrapper wrapper =
                JsonUtility.FromJson<DollyJsonWrapper>(
                    wrappedJson
                );

            if (
                wrapper == null ||
                wrapper.Points == null ||
                wrapper.Points.Length == 0
            )
            {
                if (firstImport && createdDolly != null)
                {
                    Undo.DestroyObjectImmediate(createdDolly);
                    targetDolly = null;
                }

                EditorUtility.DisplayDialog(
                    "Dolly Importer",
                    "No Dolly points were found in the selected JSON file.",
                    "OK"
                );

                return;
            }


            // ============================================================
            // GROUP POINTS BY PATH
            // ============================================================

            SortedDictionary<int, List<DollyPointJson>> paths =
                new SortedDictionary<int, List<DollyPointJson>>();

            foreach (DollyPointJson point in wrapper.Points)
            {
                if (!paths.ContainsKey(point.PathIndex))
                {
                    paths.Add(
                        point.PathIndex,
                        new List<DollyPointJson>()
                    );
                }

                paths[point.PathIndex].Add(point);
            }

            foreach (
                KeyValuePair<int, List<DollyPointJson>> pair in paths
            )
            {
                pair.Value.Sort(
                    (a, b) => a.Index.CompareTo(b.Index)
                );
            }


            // ============================================================
            // START UNDO GROUP
            // ============================================================

            int undoGroup = Undo.GetCurrentGroup();

            Undo.SetCurrentGroupName(
                "Import VRChat Dolly JSON"
            );


            // ============================================================
            // CREATE TEMPORARY TEMPLATES
            // ============================================================

            GameObject pathTemplate = Instantiate(
                originalPathTemplate.gameObject
            );

            pathTemplate.name = "__DollyPathTemplate";
            pathTemplate.hideFlags = HideFlags.HideAndDontSave;

            GameObject pointTemplate = Instantiate(
                originalPointTemplate.gameObject
            );

            pointTemplate.name = "__DollyPointTemplate";
            pointTemplate.hideFlags = HideFlags.HideAndDontSave;


            // The Path template should contain no Points.
            for (
                int i = pathTemplate.transform.childCount - 1;
                i >= 0;
                i--
            )
            {
                DestroyImmediate(
                    pathTemplate.transform.GetChild(i).gameObject
                );
            }


            // ============================================================
            // DELETE OLD PATHS
            // ============================================================

            for (
                int i = targetDolly.transform.childCount - 1;
                i >= 0;
                i--
            )
            {
                Undo.DestroyObjectImmediate(
                    targetDolly.transform.GetChild(i).gameObject
                );
            }


            // ============================================================
            // BUILD NEW PATHS
            // ============================================================

            int createdPathCount = 0;
            int createdPointCount = 0;

            foreach (
                KeyValuePair<int, List<DollyPointJson>> pathEntry
                in paths
            )
            {
                int pathIndex = pathEntry.Key;

                // --------------------------------------------------------
                // PATH
                // --------------------------------------------------------

                GameObject pathObject = Instantiate(
                    pathTemplate
                );

                pathObject.hideFlags = HideFlags.None;
                pathObject.name = $"Path ({pathIndex})";

                Undo.RegisterCreatedObjectUndo(
                    pathObject,
                    "Create Dolly Path"
                );

                pathObject.transform.SetParent(
                    targetDolly.transform,
                    false
                );

                createdPathCount++;


                // --------------------------------------------------------
                // POINTS
                // --------------------------------------------------------

                foreach (
                    DollyPointJson pointData
                    in pathEntry.Value
                )
                {
                    GameObject pointObject = Instantiate(
                        pointTemplate
                    );

                    pointObject.hideFlags = HideFlags.None;
                    pointObject.name =
                        $"Point ({pointData.Index})";

                    Undo.RegisterCreatedObjectUndo(
                        pointObject,
                        "Create Dolly Point"
                    );

                    pointObject.transform.SetParent(
                        pathObject.transform,
                        false
                    );


                    // ====================================================
                    // POSITION / ROTATION
                    // ====================================================

                    Vector3 position = new Vector3(
                        pointData.Position.X,
                        pointData.Position.Y,
                        pointData.Position.Z
                    );

                    Vector3 rotation = new Vector3(
                        pointData.Rotation.X,
                        pointData.Rotation.Y,
                        pointData.Rotation.Z
                    );

                    if (pointData.IsLocal)
                    {
                        pointObject.transform.localPosition =
                            position;

                        pointObject.transform.localEulerAngles =
                            rotation;
                    }
                    else
                    {
                        pointObject.transform.position =
                            position;

                        pointObject.transform.eulerAngles =
                            rotation;
                    }


                    // ====================================================
                    // FIND DOLLY POINT COMPONENT
                    // ====================================================

                    Component pointComponent =
                        FindDollyComponent(
                            pointObject,
                            "Point"
                        );

                    if (pointComponent == null)
                    {
                        CleanupTemplates(
                            pathTemplate,
                            pointTemplate
                        );

                        if (firstImport)
                        {
                            ShowModifiedPrefabError();
                        }
                        else
                        {
                            ShowExistingDollyError();
                        }

                        return;
                    }


                    // ====================================================
                    // APPLY JSON VALUES
                    // ====================================================

                    ApplyPointValues(
                        pointComponent,
                        pointData
                    );

                    EditorUtility.SetDirty(
                        pointComponent
                    );

                    EditorUtility.SetDirty(
                        pointObject
                    );

                    createdPointCount++;
                }

                EditorUtility.SetDirty(
                    pathObject
                );
            }


            // ============================================================
            // CLEAN UP TEMPORARY TEMPLATES
            // ============================================================

            CleanupTemplates(
                pathTemplate,
                pointTemplate
            );


            // ============================================================
            // FINISH
            // ============================================================

            EditorUtility.SetDirty(
                targetDolly
            );

            Undo.CollapseUndoOperations(
                undoGroup
            );

            Selection.activeGameObject =
                targetDolly;

            EditorUtility.DisplayDialog(
                "Dolly Imported",
                $"Import complete!\n\n" +
                $"Paths: {createdPathCount}\n" +
                $"Points: {createdPointCount}\n\n" +
                "You can now edit the Camera Dolly Animation settings " +
                "according to your liking.\n\n" +
                "Remember to click \"Collect Paths & Points\" on the " +
                "Camera Dolly Animation component if necessary.",
                "OK"
            );
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);

            if (
                firstImport &&
                createdDolly != null
            )
            {
                DestroyImmediate(createdDolly);
                targetDolly = null;
            }

            EditorUtility.DisplayDialog(
                "Error Importing Camera Dolly",
                "There was an error importing the camera dolly.\n\n" +
                exception.Message,
                "OK"
            );
        }
    }


    // ================================================================
    // FIND STARTING PREFAB
    // ================================================================

    private GameObject FindStartingPrefab()
    {
        string[] prefabGuids = AssetDatabase.FindAssets(
            "t:Prefab",
            new string[]
            {
                PackageRoot
            }
        );

        foreach (string guid in prefabGuids)
        {
            string assetPath =
                AssetDatabase.GUIDToAssetPath(guid);

            GameObject prefab =
                AssetDatabase.LoadAssetAtPath<GameObject>(
                    assetPath
                );

            if (
                prefab != null &&
                ValidateDollyHierarchy(prefab)
            )
            {
                return prefab;
            }
        }

        return null;
    }


    // ================================================================
    // VALIDATE DOLLY
    // ================================================================

    private bool ValidateDollyHierarchy(
        GameObject dolly
    )
    {
        if (dolly == null)
        {
            return false;
        }

        // The root must have the Camera Dolly Animation component.
        if (
            FindDollyComponent(
                dolly,
                "Animation"
            ) == null
        )
        {
            return false;
        }

        // There must be at least one Path.
        if (dolly.transform.childCount == 0)
        {
            return false;
        }

        GameObject path =
            dolly.transform.GetChild(0).gameObject;

        // The first Path must have its Dolly Path component.
        if (
            FindDollyComponent(
                path,
                "Path"
            ) == null
        )
        {
            return false;
        }

        // The first Path must contain at least one Point.
        if (path.transform.childCount == 0)
        {
            return false;
        }

        GameObject point =
            path.transform.GetChild(0).gameObject;

        // The first Point must have its Dolly Point component.
        if (
            FindDollyComponent(
                point,
                "Point"
            ) == null
        )
        {
            return false;
        }

        return true;
    }


    // ================================================================
    // FIND DOLLY COMPONENT
    // ================================================================

    private Component FindDollyComponent(
        GameObject gameObject,
        string expectedName
    )
    {
        Component[] components =
            gameObject.GetComponents<Component>();

        foreach (Component component in components)
        {
            if (component == null)
            {
                continue;
            }

            if (component is Transform)
            {
                continue;
            }

            Type type = component.GetType();

            if (
                type.Name.IndexOf(
                    "Dolly",
                    StringComparison.OrdinalIgnoreCase
                ) >= 0 &&
                type.Name.IndexOf(
                    expectedName,
                    StringComparison.OrdinalIgnoreCase
                ) >= 0
            )
            {
                return component;
            }
        }

        return null;
    }


    // ================================================================
    // APPLY POINT VALUES
    // ================================================================

    private void ApplyPointValues(
        Component pointComponent,
        DollyPointJson data
    )
    {
        SerializedObject serializedPoint =
            new SerializedObject(
                pointComponent
            );

        serializedPoint.Update();

        SetFloat(
            serializedPoint,
            data.Zoom,
            "zoom",
            "Zoom",
            "_zoom"
        );

        SetFloat(
            serializedPoint,
            data.Duration,
            "duration",
            "Duration",
            "_duration"
        );

        SetFloat(
            serializedPoint,
            data.Speed,
            "speed",
            "Speed",
            "_speed"
        );

        SetFloat(
            serializedPoint,
            data.FocalDistance,
            "focalDistance",
            "FocalDistance",
            "_focalDistance"
        );

        SetFloat(
            serializedPoint,
            data.Aperture,
            "aperture",
            "Aperture",
            "_aperture"
        );

        SetFloat(
            serializedPoint,
            data.Hue,
            "hue",
            "Hue",
            "_hue"
        );

        SetFloat(
            serializedPoint,
            data.Saturation,
            "saturation",
            "Saturation",
            "_saturation"
        );

        SetFloat(
            serializedPoint,
            data.Lightness,
            "lightness",
            "Lightness",
            "_lightness"
        );

        SetFloat(
            serializedPoint,
            data.LookAtMeXOffset,
            "lookAtMeXOffset",
            "LookAtMeXOffset",
            "_lookAtMeXOffset"
        );

        SetFloat(
            serializedPoint,
            data.LookAtMeYOffset,
            "lookAtMeYOffset",
            "LookAtMeYOffset",
            "_lookAtMeYOffset"
        );

        SetFloat(
            serializedPoint,
            data.Exposure,
            "exposure",
            "Exposure",
            "_exposure"
        );

        serializedPoint.ApplyModifiedProperties();
    }


    // ================================================================
    // SET SERIALIZED FLOAT
    // ================================================================

    private bool SetFloat(
        SerializedObject serializedObject,
        float value,
        params string[] possibleNames
    )
    {
        foreach (
            string propertyName
            in possibleNames
        )
        {
            SerializedProperty property =
                serializedObject.FindProperty(
                    propertyName
                );

            if (
                property != null &&
                property.propertyType ==
                SerializedPropertyType.Float
            )
            {
                property.floatValue = value;
                return true;
            }
        }

        return false;
    }


    // ================================================================
    // CLEANUP
    // ================================================================

    private void CleanupTemplates(
        GameObject pathTemplate,
        GameObject pointTemplate
    )
    {
        if (pathTemplate != null)
        {
            DestroyImmediate(pathTemplate);
        }

        if (pointTemplate != null)
        {
            DestroyImmediate(pointTemplate);
        }
    }


    // ================================================================
    // ERROR POPUPS
    // ================================================================

    private void ShowModifiedPrefabError()
    {
        EditorUtility.DisplayDialog(
            "Error Importing Camera Dolly",
            "There was an error importing the camera dolly.\n\n" +
            "The Dolly Importer's starting prefab appears to have been " +
            "modified or is missing one or more objects or scripts required " +
            "for the import.\n\n" +
            "Please reimport the Dolly Importer package and try again.",
            "OK"
        );
    }

    private void ShowExistingDollyError()
    {
        EditorUtility.DisplayDialog(
            "Error Importing Camera Dolly",
            "There was an error importing the camera dolly.\n\n" +
            "The selected Dolly is missing one or more objects or scripts " +
            "required for the import.\n\n" +
            "Do not remove the first Path, the first Point inside that Path, " +
            "or their VRChat Dolly Path/Point scripts.",
            "OK"
        );
    }


    // ================================================================
    // JSON DATA
    // ================================================================

    [Serializable]
    private class DollyJsonWrapper
    {
        public DollyPointJson[] Points;
    }

    [Serializable]
    private class DollyPointJson
    {
        public bool IsLocal;

        public DollyVector Position;
        public DollyVector Rotation;

        public float FocalDistance;
        public float Aperture;

        public float Hue;
        public float Saturation;
        public float Lightness;

        public float LookAtMeXOffset;
        public float LookAtMeYOffset;

        public float Zoom;
        public float Exposure;

        public float Speed;
        public float Duration;

        public int Index;
        public int PathIndex;
    }

    [Serializable]
    private class DollyVector
    {
        public float X;
        public float Y;
        public float Z;
    }
}