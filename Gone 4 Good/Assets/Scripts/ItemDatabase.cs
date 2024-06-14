using UnityEngine;

public class ItemDatabase : MonoBehaviour
{
    public static ItemDatabase instance;

    public ItemBlueprint[] items;

    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("More than one instance of ItemDatabase found!");
            return;
        }
        else
        {
            instance = this;
        }
    }

    public static ItemBlueprint GetItem(int id)
    {
        return instance.items[id];
    }
}