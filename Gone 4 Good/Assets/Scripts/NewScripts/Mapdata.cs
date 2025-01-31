using UnityEditor;
using UnityEngine;

public class Mapdata : MonoBehaviour
{
    public Transform[] children;
    public Vector3 center;
    public float radius;
    // On Validate
    void OnValidate()
    {
        children = gameObject.GetComponentsInChildren<Transform>();
        // Calculate the average center of the map
        Vector3 min = this.children[0].position;
        Vector3 max = this.children[0].position;
        foreach (Transform child in this.children)
        {
            min = Vector3.Min(min, child.position);
            max = Vector3.Max(max, child.position);
        }
        this.center = (min + max) / 2;
        this.radius = Vector3.Distance(min, max) / 2;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(this.center, this.radius);
    }
}

#if UNITY_EDITOR
// EditorScript
public class MapdataEditor : Editor
{
    // OnInspectorGUI is called once per frame
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        Mapdata mapdata = (Mapdata)target;
        
    }


}
#endif
