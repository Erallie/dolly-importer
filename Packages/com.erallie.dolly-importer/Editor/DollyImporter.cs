using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class DollyImporter : EditorWindow
{
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
            targetDolly == null ||
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
            "The selected Dolly must already contain at least one Path with at least " +
            "one Point. The first Path and Point will be used as templates.\n\n" +
            "The existing Path hierarchy will be replaced by the contents of the JSON.",
            MessageType.Info
        );
    }

    private void ImportDolly()
    {
        try
        {
            // ============================================================
            // VALIDATE TEMPLATE HIERARCHY
            // ============================================================

            if (targetDolly.transform.childCount == 0)
            {
                ShowTemplateError();
                return;
            }

            Transform originalPathTemplate = targetDolly.transform.GetChild(0);

            if (originalPathTemplate.childCount == 0)
            {
                ShowTemplateError();
                return;
            }

            Transform originalPointTemplate = originalPathTemplate.GetChild(0);


            // ============================================================
            // READ JSON
            // ============================================================

            string json = File.ReadAllText(jsonPath);

            // VRChat's exported Dolly is a raw JSON array.
            // JsonUtility needs an object at the root, so wrap it.
            string wrappedJson = "{\"Points\":" + json + "}";

            DollyJsonWrapper wrapper =
                JsonUtility.FromJson<DollyJsonWrapper>(wrappedJson);

            if (
                wrapper == null ||
                wrapper.Points == null ||
                wrapper.Points.Length == 0
            )
            {
                EditorUtility.DisplayDialog(
                    "Dolly Importer",
                    "No Dolly points were found in the JSON.",
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
            // CREATE TEMPORARY COPIES OF OUR TEMPLATES
            //
            // We need these because we're about to destroy the old
            // hierarchy.
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


            // Remove children from the path template.
            // We only want the Path object and its components.
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

                        EditorUtility.DisplayDialog(
                            "Dolly Importer",
                            "Could not identify the Dolly Point " +
                            "component on the template Point.",
                            "OK"
                        );

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
                "Now click \"Collect Paths & Points\" " +
                "on the VRC Camera Dolly Animation component.",
                "OK"
            );
        }
        catch (Exception exception)
        {
            Debug.LogException(
                exception
            );

            EditorUtility.DisplayDialog(
                "Dolly Import Failed",
                exception.Message,
                "OK"
            );
        }
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

            // Ignore Transform.
            if (component is Transform)
            {
                continue;
            }

            Type type = component.GetType();

            /*
             * We don't need to know the namespace.
             *
             * We're simply looking for a component whose type name
             * contains both "Dolly" and the expected object type,
             * e.g. Dolly + Point.
             */
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

        /*
         * Exposure is present in VRChat's exported JSON.
         * If the current SDK exposes a serialized Exposure field,
         * populate it. Otherwise it will simply be skipped.
         */
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
                property.floatValue =
                    value;

                return true;
            }
        }

        /*
         * Don't throw an error here.
         *
         * Different SDK versions may expose different serialized
         * properties. Missing optional properties are simply skipped.
         */
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
            DestroyImmediate(
                pathTemplate
            );
        }

        if (pointTemplate != null)
        {
            DestroyImmediate(
                pointTemplate
            );
        }
    }


    // ================================================================
    // TEMPLATE ERROR
    // ================================================================

    private void ShowTemplateError()
    {
        EditorUtility.DisplayDialog(
            "Dolly Importer",
            "The selected Dolly needs an existing template hierarchy.\n\n" +
            "It should contain at least:\n\n" +
            "Dolly\n" +
            "└── Path\n" +
            "    └── Point\n\n" +
            "The Path and Point should already have their normal " +
            "VRChat Dolly components.",
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