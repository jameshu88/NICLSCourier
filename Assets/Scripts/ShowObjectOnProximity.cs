using System;
using System.Collections;
using UnityEngine;

public class ShowObjectOnProximity : MonoBehaviour
{
    [Tooltip("The distance at which the object should be shown")]
    [SerializeField] private float distanceThreshold = 10f;
    [Tooltip("The model of the object")]
    [SerializeField] private GameObject obj;
    [Tooltip("The offset of object in Y-direction")]
    [SerializeField] private float objHeightOffset = 0f;

    private GameObject player;
    [HideInInspector] public bool isObjectSeen;

    public static Action OnPlayObjectSound;

    void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        obj.SetActive(false);
    }

    void Update()
    {
        if (player != null)
        {
            float distance = Vector3.Distance(player.transform.position, transform.position);
            if (distance <= distanceThreshold)
            {
                if(!obj.activeInHierarchy)
                {
                    obj.SetActive(true);
                    OnPlayObjectSound?.Invoke();
                }
            }
            else
            {
                if(!isObjectSeen)
                {
                    obj.SetActive(false);
                }
            }
        }
    }

    public float GetHeightOffset()
    {
        return objHeightOffset;
    }
}
