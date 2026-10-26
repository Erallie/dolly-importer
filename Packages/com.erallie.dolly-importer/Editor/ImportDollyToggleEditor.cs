#if UNITY_EDITOR && !COMPILER_UDONSHARP

using System.Collections.Generic;
using UdonSharpEditor;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using VRC.SDK3.Components;

[CustomEditor(typeof(ImportDollyToggle))]
[CanEditMultipleObjects]
public class ImportDollyToggleEditor : Editor
{
    private string referenceSearchMessage;

    public override void OnInspectorGUI()
    {
        // ============================================================
        // TITLE
        // ============================================================

        var titleStyle = new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize = 20,
            wordWrap = true
        };

        var descriptionStyle = new GUIStyle(EditorStyles.label)
        {
            wordWrap = true
        };

        EditorGUILayout.Space(8);

        EditorGUILayout.LabelField(
            ObjectNames.NicifyVariableName(target.GetType().Name),
            titleStyle
        );

        EditorGUILayout.Space(4);

        EditorGUILayout.LabelField(
            "Imports the assigned Camera Dolly Animation when this object is interacted with.",
            descriptionStyle
        );

        EditorGUILayout.Space(4);


        // ============================================================
        // PACKAGE VERSION
        // ============================================================

        var packageInfo =
            UnityEditor.PackageManager.PackageInfo.FindForAssembly(
                target.GetType().Assembly
            );

        if (packageInfo != null)
        {
            EditorGUILayout.LabelField(
                "Version " + packageInfo.version,
                EditorStyles.miniLabel
            );
        }


        // ============================================================
        // GITHUB
        // ============================================================

        if (EditorGUILayout.LinkButton(
            "View on GitHub",
            GUILayout.ExpandWidth(false)))
        {
            Application.OpenURL(
                "https://github.com/Erallie/dolly-importer"
            );
        }

        EditorGUILayout.Space(12);


        // ============================================================
        // UDONSHARP HEADER
        // ============================================================

        if (UdonSharpGUI.DrawDefaultUdonSharpBehaviourHeader(targets))
        {
            return;
        }


        // ============================================================
        // CAMERA DOLLY FIELD + FIND BUTTON
        // ============================================================

        serializedObject.Update();

        SerializedProperty cameraDolly =
            serializedObject.FindProperty("cameraDolly");

        if (cameraDolly != null)
        {
            DrawSceneReference(cameraDolly);

            EditorGUILayout.HelpBox(
                "The Camera Dolly Animation that will be imported when this object is interacted with.",
                MessageType.Info
            );

            EditorGUILayout.Space(6);

            if (!string.IsNullOrEmpty(referenceSearchMessage))
            {
                EditorGUILayout.HelpBox(
                    referenceSearchMessage,
                    MessageType.Info
                );
            }
        }

        serializedObject.ApplyModifiedProperties();
    }


    // ================================================================
    // DRAW REFERENCE + FIND BUTTON
    // ================================================================

    private void DrawSceneReference(
        SerializedProperty property
    )
    {
        float height =
            EditorGUI.GetPropertyHeight(
                property,
                true
            );

        Rect row =
            EditorGUILayout.GetControlRect(
                true,
                height
            );

        Rect field = row;

        field.width -= 54f;

        EditorGUI.PropertyField(
            field,
            property,
            true
        );

        Rect button = new Rect(
            row.xMax - 48f,
            row.yMax - EditorGUIUtility.singleLineHeight,
            48f,
            EditorGUIUtility.singleLineHeight
        );

        bool canSearch =
            CanFindSceneReference();

        using (
            new EditorGUI.DisabledScope(
                !canSearch
            )
        )
        {
            if (GUI.Button(
                button,
                new GUIContent(
                    "Find",
                    "Find a Camera Dolly Animation in this scene, including inactive objects. " +
                    "Multiple matches open a selection menu. Select components in the same scene and exit Play Mode."
                )
            ))
            {
                serializedObject.ApplyModifiedProperties();

                FindSceneReference();

                serializedObject.Update();
            }
        }
    }


    // ================================================================
    // CAN FIND
    // ================================================================

    private bool CanFindSceneReference()
    {
        if (
            EditorApplication
                .isPlayingOrWillChangePlaymode
        )
        {
            return false;
        }

        var first =
            target as Component;

        if (first == null)
        {
            return false;
        }

        foreach (
            UnityEngine.Object selected
            in targets
        )
        {
            var component =
                selected as Component;

            if (
                component == null ||
                EditorUtility.IsPersistent(component) ||
                !component.gameObject.scene.IsValid() ||
                !component.gameObject.scene.isLoaded ||
                component.gameObject.scene !=
                    first.gameObject.scene
            )
            {
                return false;
            }
        }

        return true;
    }


    // ================================================================
    // FIND CAMERA DOLLIES
    // ================================================================

    private void FindSceneReference()
    {
        var component =
            target as Component;

        var found =
            new List<VRCCameraDollyAnimation>();

        foreach (
            GameObject root
            in component.gameObject.scene.GetRootGameObjects()
        )
        {
            found.AddRange(
                root.GetComponentsInChildren<VRCCameraDollyAnimation>(
                    true
                )
            );
        }

        UnityEngine.Object[] owners =
            (UnityEngine.Object[])targets.Clone();


        // ------------------------------------------------------------
        // NONE FOUND
        // ------------------------------------------------------------

        if (found.Count == 0)
        {
            referenceSearchMessage =
                "No Camera Dolly Animation component was found in this scene. " +
                "The current reference was kept.";

            return;
        }


        // ------------------------------------------------------------
        // ONE FOUND
        // ------------------------------------------------------------

        if (found.Count == 1)
        {
            AssignSceneReference(
                owners,
                found[0]
            );

            referenceSearchMessage = null;

            return;
        }


        // ------------------------------------------------------------
        // MULTIPLE FOUND
        // ------------------------------------------------------------

        referenceSearchMessage = null;

        var menu =
            new GenericMenu();

        for (
            int i = 0;
            i < found.Count;
            i++
        )
        {
            VRCCameraDollyAnimation candidate =
                found[i];

            string label =
                (i + 1) +
                ". " +
                GetHierarchyPath(
                    candidate.transform
                );

            menu.AddItem(
                new GUIContent(label),
                false,
                () =>
                    AssignSceneReference(
                        owners,
                        candidate
                    )
            );
        }

        menu.ShowAsContext();
    }


    // ================================================================
    // HIERARCHY PATH
    // ================================================================

    private static string GetHierarchyPath(
        Transform item
    )
    {
        string result =
            item.name;

        while (item.parent != null)
        {
            item = item.parent;

            result =
                item.name +
                " > " +
                result;
        }

        return result;
    }


    // ================================================================
    // ASSIGN CAMERA DOLLY
    // ================================================================

    private static void AssignSceneReference(
        UnityEngine.Object[] owners,
        VRCCameraDollyAnimation candidate
    )
    {
        if (candidate == null)
        {
            return;
        }

        Undo.SetCurrentGroupName(
            "Find Scene Reference"
        );

        foreach (
            UnityEngine.Object owner
            in owners
        )
        {
            var component =
                owner as Component;

            if (
                component == null ||
                EditorUtility.IsPersistent(component) ||
                component.gameObject.scene !=
                    candidate.gameObject.scene
            )
            {
                continue;
            }

            var componentObject =
                new SerializedObject(owner);

            componentObject.Update();

            SerializedProperty reference =
                componentObject.FindProperty(
                    "cameraDolly"
                );

            if (
                reference != null &&
                reference.propertyType ==
                    SerializedPropertyType.ObjectReference &&
                reference.objectReferenceValue !=
                    candidate
            )
            {
                reference.objectReferenceValue =
                    candidate;

                if (
                    componentObject
                        .ApplyModifiedProperties()
                )
                {
                    EditorSceneManager.MarkSceneDirty(
                        component.gameObject.scene
                    );
                }
            }
        }
    }
}

#endif