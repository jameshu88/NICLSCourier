using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

//This script automates the assignment of scripts necessary for prefabs to work with the proximity script using the NavMesh

public class ShowObjectOnProximityAssigner : EditorWindow
{
    [MenuItem("Tools/Assign ShowObjectOnProximity")]
    public static void ShowWindow()
    {
        GetWindow<ShowObjectOnProximityAssigner>("Assign ShowObjectOnProximity");
    }

    public string prefabsFolderPath = "Assets/Models/Object Prefabs 2"; // Folder containing prefabs

    private void OnGUI()
    {
        GUILayout.Label("Assign ShowObjectOnProximity", EditorStyles.boldLabel);

        prefabsFolderPath = EditorGUILayout.TextField("Prefabs Folder Path", prefabsFolderPath);

        if (GUILayout.Button("Assign ShowObjectOnProximity"))
        {
            AssignShowObjectOnProximity();
        }
    }

    private void AssignShowObjectOnProximity()
    {
        if (!Directory.Exists(prefabsFolderPath))
        {
            Debug.LogError($"Prefabs folder path does not exist: {prefabsFolderPath}");
            return;
        }

        string[] prefabGUIDs = AssetDatabase.FindAssets("t:Prefab", new[] { prefabsFolderPath });
        Debug.Log($"Found {prefabGUIDs.Length} prefabs in the specified folder.");

        foreach (string guid in prefabGUIDs)
        {
            string prefabPath = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab != null)
            {
                GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);

                ShowObjectOnProximity script = instance.GetComponent<ShowObjectOnProximity>();
                if (script == null)
                {
                    script = instance.AddComponent<ShowObjectOnProximity>();
                }

                // Use reflection to set private serialized fields
                SetPrivateField(script, "distanceThreshold", 15f);
                SetPrivateField(script, "obj", instance);
                SetPrivateField(script, "objHeightOffset", 0f);

                PrefabUtility.SaveAsPrefabAsset(instance, prefabPath);
                DestroyImmediate(instance);

                Debug.Log($"Assigned ShowObjectOnProximity to prefab: {prefabPath}");
            }
            else
            {
                Debug.LogWarning($"Could not load prefab at path: {prefabPath}");
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private void SetPrivateField(object target, string fieldName, object value)
    {
        FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
        if (field != null)
        {
            field.SetValue(target, value);
        }
        else
        {
            Debug.LogWarning($"Field {fieldName} not found on target {target}");
        }
    }
}