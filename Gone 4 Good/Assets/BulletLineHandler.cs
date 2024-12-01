using System.Collections;
using UnityEngine;

public class BulletLineHandler : MonoBehaviour
{
    public Vector3 start;
    public Vector3 end;
    public float speed = 150;
    public bool firstFrameSkipped = false;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!firstFrameSkipped)
        {
            firstFrameSkipped = true;
            return;
        }
        transform.position = Vector3.MoveTowards(transform.position, end, speed * Time.deltaTime);
        if (transform.position == end)
        {
            Destroy(gameObject,2f);
            enabled = false;
        }
        
    }
}
