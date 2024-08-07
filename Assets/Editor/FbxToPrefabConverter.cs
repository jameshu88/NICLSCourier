using UnityEditor;
using UnityEngine;
using System.IO;

public class FbxToPrefabConverter : EditorWindow
{
    [MenuItem("Tools/Convert FBX to Prefabs")]
    public static void ShowWindow()
    {
        GetWindow<FbxToPrefabConverter>("Convert FBX to Prefabs");
    }

    public string fbxFolderPath = "Assets/Models/Object Prefabs 2"; // Folder containing .fbx files
    public string prefabFolderPath = "Assets/Prefabs"; // Target folder for prefabs

    private void OnGUI()
    {
        GUILayout.Label("Convert FBX to Prefabs", EditorStyles.boldLabel);

        fbxFolderPath = EditorGUILayout.TextField("FBX Folder Path", fbxFolderPath);
        prefabFolderPath = EditorGUILayout.TextField("Prefab Folder Path", prefabFolderPath);

        if (GUILayout.Button("Convert"))
        {
            ConvertFbxToPrefabs();
        }
    }

    private void ConvertFbxToPrefabs()
    {
        if (!Directory.Exists(fbxFolderPath))
        {
            Debug.LogError($"FBX folder path does not exist: {fbxFolderPath}");
            return;
        }

        if (!Directory.Exists(prefabFolderPath))
        {
            Debug.LogError($"Prefab folder path does not exist: {prefabFolderPath}");
            return;
        }

        string[] fbxGUIDs = AssetDatabase.FindAssets("t:Model", new[] { fbxFolderPath });
        Debug.Log($"Found {fbxGUIDs.Length} FBX files in the specified folder.");

        foreach (string guid in fbxGUIDs)
        {
            string fbxPath = AssetDatabase.GUIDToAssetPath(guid);
            GameObject fbxModel = AssetDatabase.LoadAssetAtPath<GameObject>(fbxPath);
            if (fbxModel != null)
            {
                string prefabPath = Path.Combine(prefabFolderPath, Path.GetFileNameWithoutExtension(fbxPath) + ".prefab");
                prefabPath = AssetDatabase.GenerateUniqueAssetPath(prefabPath);

                GameObject prefab = PrefabUtility.SaveAsPrefabAsset(fbxModel, prefabPath);
                if (prefab != null)
                {
                    Debug.Log($"Created prefab: {prefabPath}");
                }
                else
                {
                    Debug.LogWarning($"Could not create prefab for FBX: {fbxPath}");
                }
            }
            else
            {
                Debug.LogWarning($"Could not load FBX model at path: {fbxPath}");
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}