using UnityEngine;

[CreateAssetMenu(fileName = "DirectorShop", menuName = "ScriptableObjects/DirectorShop", order = 1)]
public class DirectorShop : ScriptableObject
{
    public ShopResourceItem[] shopItems;
    public int AllItemAmounts 
    {  
        get 
        {  
            int sum = 0;
            foreach(ShopResourceItem i in shopItems)
            {
                sum += i.amount;
            }
            return sum;

        } 
    }

    public void IncreaseShopAmount(int index, int maxAmount)
    {
        shopItems[index].maxAmount += maxAmount;
    }

    public void CheckForRestock()
    {
        if(AllItemAmounts == 0) 
        { 
            for(int i = 0; i < shopItems.Length; i++)
            {
                shopItems[i].amount = shopItems[i].maxAmount;
            }
        }
    }


    public int GetRandomAffordableShopIndex(int credits)
    {

        int breakOut = 0;
        while(breakOut < 10)
        {
            int rnd = Random.Range(0, shopItems.Length);
            if (shopItems[rnd].amount > 0 && shopItems[rnd].cost <= credits)
            {
                return rnd;
            }
            breakOut++;
        }
        return -1;

    }
}

[System.Serializable]
public struct ShopResourceItem
{
    public ResoruceBlockData resourceBlockData;
    public int maxAmount;
    public int amount;
    public int cost;
}