using UnityEngine;

public class Billboard : MonoBehaviour
{
    Transform cam;

    private void OnEnable()
    {
        if (cam == null)
        {
            cam = Camera.main.transform;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(cam != null)
        {
            transform.LookAt(transform.position + cam.transform.forward);
        }
        else if(Camera.main != null)
        {
            cam = Camera.main.transform;
        }
    }
}
