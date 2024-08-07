using UnityEngine;
using UnityEditor;
using System.Collections;
  
public class ColliderToFit : MonoBehaviour {
      
    [MenuItem("My Tools/Collider/Fit to Children")]
    static void FitToChildren() {
        foreach (GameObject obj in Selection.gameObjects) {
            if (!(obj.GetComponent<Collider>() is BoxCollider)) {
                obj.AddComponent<BoxCollider>();
            }
            Quaternion prevLocalRot = obj.transform.localRotation;
            obj.transform.localRotation = Quaternion.identity;
            
            Vector3 pos = obj.transform.position;
            Bounds bounds = CalculateLocalBounds(obj);
        
            BoxCollider collider = (BoxCollider)obj.GetComponent<Collider>();
            collider.size = bounds.size * .85f; // scale down bounding boxes
            collider.center = bounds.center;
            obj.transform.localRotation = prevLocalRot;
        }
    }

    public static Bounds CalculateLocalBounds(GameObject obj)
    {
        Quaternion currentRotation = obj.transform.rotation;
        Vector3 currentScale = obj.transform.localScale;
        obj.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        obj.transform.localScale = Vector3.one;

        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
        Bounds bounds = renderers[0].bounds;

        foreach (Renderer renderer in renderers)
        {
            bounds.Encapsulate(renderer.bounds);
        }

        Vector3 localCenter = bounds.center - obj.transform.position;
        bounds.center = localCenter;

        Debug.Log("The local bounds of this model is " + bounds);
        obj.transform.rotation = currentRotation;
        obj.transform.localScale = currentScale;
        return bounds;
    }
}
