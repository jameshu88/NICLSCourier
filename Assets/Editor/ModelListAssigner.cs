using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

//This script automates assigning prefabs into the ModelList serializable object so we don't have to drag in 100+ models by hand every time we iterate on the experiment

public class ModelListAssigner : EditorWindow
{
    [MenuItem("Tools/Assign Models to ModelList")]
    public static void ShowWindow()
    {
        GetWindow<ModelListAssigner>("Assign Models");
    }

    public ModelList modelList;
    public string modelsFolderPath = "Assets/Models/Object Prefabs"; // Default path with spaces

    private void OnGUI()
    {
        GUILayout.Label("Assign Models to ModelList", EditorStyles.boldLabel);

        modelList = (ModelList)EditorGUILayout.ObjectField("Model List", modelList, typeof(ModelList), false);
        modelsFolderPath = EditorGUILayout.TextField("Models Folder Path", modelsFolderPath);

        if (GUILayout.Button("Assign Models"))
        {
            AssignModels();
        }
    }

    private void AssignModels()
    {
        if (modelList == null)
        {
            Debug.LogError("Model List is not assigned.");
            return;
        }

        Debug.Log($"Searching for models in folder: {modelsFolderPath}");

        // Find both .fbx and .prefab files
        string[] modelGUIDs = AssetDatabase.FindAssets("t:Model", new[] { modelsFolderPath });
        string[] prefabGUIDs = AssetDatabase.FindAssets("t:Prefab", new[] { modelsFolderPath });
        List<GameObject> models = new List<GameObject>();

        Debug.Log($"Found {modelGUIDs.Length} models and {prefabGUIDs.Length} prefabs in the specified folder.");

        // Add models
        for (int i = 0; i < modelGUIDs.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(modelGUIDs[i]);
            GameObject model = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (model != null)
            {
                Debug.Log($"Adding model: {model.name} from path: {path}");
                models.Add(model);
            }
            else
            {
                Debug.LogWarning($"Could not load model at path: {path}");
            }
        }

        // Add prefabs
        for (int i = 0; i < prefabGUIDs.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(prefabGUIDs[i]);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab != null)
            {
                Debug.Log($"Adding prefab: {prefab.name} from path: {path}");
                models.Add(prefab);
            }
            else
            {
                Debug.LogWarning($"Could not load prefab at path: {path}");
            }
        }

        modelList.models = models;
        EditorUtility.SetDirty(modelList);

        Debug.Log($"Assigned {models.Count} models to the Model List.");
    }
}