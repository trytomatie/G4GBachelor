using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowGameObjectUI : MonoBehaviour
{
    public GameObject objectToFollow;
    private Camera mainCamera;
    private RectTransform rectTransform;
    public Vector3 offset;
    // Start is called before the first frame update
    void Start()
    {
        mainCamera = Camera.main;
        rectTransform = GetComponent<RectTransform>();
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (objectToFollow != null)
        {
            // Only follow Position if onscreen
            Vector3 screenPoint = mainCamera.WorldToViewportPoint(objectToFollow.transform.position + offset);
            bool onScreen = screenPoint.z > 0 && screenPoint.x > 0 && screenPoint.x < 1 && screenPoint.y > 0 && screenPoint.y < 1;
            if (onScreen)
            {
                Vector2 screenPosition = mainCamera.WorldToScreenPoint(objectToFollow.transform.position + offset);
                rectTransform.position = screenPosition;
            }
            else if (rectTransform.position != new Vector3(-1000, -1000))
            {
                rectTransform.position = new Vector2(-1000, -1000);
            }
        }
    }
}
