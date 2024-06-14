using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraSoftFollowMouse : MonoBehaviour
{
    private Vector3 startOffset;
    public float followMultiplier = 0.3f;
    private void Start()
    {
        startOffset = transform.position;
    }
    // Update is called once per frame
    void Update()
    {
        

        Vector3 mousePos = Input.mousePosition;
        mousePos.x -= Screen.width / 2;
        mousePos.y -= Screen.height / 2;
        mousePos = new Vector3(Mathf.Clamp(mousePos.x, -Screen.width/2, Screen.width/2), Mathf.Clamp(mousePos.y, -Screen.height/2, Screen.height/2), 0);
        mousePos.z = mousePos.y;
        mousePos.y = 0;
        
        transform.position = Vector3.Lerp(transform.position, startOffset + mousePos * followMultiplier, 0.1f);
    }
}
