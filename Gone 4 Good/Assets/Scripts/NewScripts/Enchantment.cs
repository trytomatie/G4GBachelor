using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;


public class Enchantment 
{
    public string prefix;
    public string suffix;
    public Color textColor = Color.white;
    public int level;
    public int bulletVFX = 0;
    public Affinity affinity = Affinity.None;
    public float bulletSpeedModifier = 1;

    public static Enchantment GenerateRandomEnchantment()
    {
        int rnd = Random.Range(0, 101);
        switch(rnd)
        {
            case <50:
                return new Enchantment();
            case <100:
                return new IceEntchantment();
            default:
                return new FireEncahntment();
        }
    }
}

public class IceEntchantment : Enchantment
{
    public IceEntchantment()
    {
        prefix = "Ice";
        textColor = Color.blue;
        affinity = Affinity.Ice;
        bulletVFX = 1;
        bulletSpeedModifier = 0.5f;
    }
}

public class FireEncahntment : Enchantment
{
    public FireEncahntment()
    {
        prefix = "Fire";
        textColor = Color.red;
        affinity = Affinity.Fire;
        bulletVFX = 2;
        bulletSpeedModifier = 1f;
    }
}

public enum Affinity
{
    None,
    Fire,
    Ice,
    Lightning,
    Slowness,
    Rejuvenation,
    Swiftness,
}
