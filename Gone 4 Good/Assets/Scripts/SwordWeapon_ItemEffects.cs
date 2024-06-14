using UnityEngine;
using static Unity.VisualScripting.Member;

[CreateAssetMenu(fileName = "SwordWeapon_ItemEffects", menuName = "ScriptableObjects/ItemInteractionEffects/SwordWeapon_ItemEffects", order = 1)]
public class SwordWeapon_ItemEffects : ItemInteractionEffects
{
    public int attackDamage = 1;
    public GameObject weaponPrefab;
    private GameObject instaniatedWeapon;
    public Skill skill;
    public override void OnUse(GameObject source, Item item)
    {
        if(isUsing)
        {
            source.GetComponent<PlayerController>().HandleAttack(true);
        }
    }

    public override void OnUseEnd(GameObject source,Item item)
    {
        source.GetComponent<PlayerController>().HandleAttack(false);
    }

    public override string EffectDescription(Item item)
    {
        string result = $"+{attackDamage} Attackdamage\n";
        result += "On Use: Strike infront of you";
        return result;
    }


    public override void OnEquip(GameObject source, Item item)
    {
        Transform weaponPivot = source.GetComponent<PlayerController>().weaponPivot.transform;
        instaniatedWeapon = Instantiate(weaponPrefab, weaponPivot);
        instaniatedWeapon.transform.localScale = weaponPrefab.transform.localScale;
        source.GetComponent<StatusManager>().weaponAttackDamage = attackDamage;
        if(skill != null)
        {
            skill.AssignSkill(source, 0);
        }

    }

    public override void OnUnequip(GameObject source, Item item)
    {
        source.GetComponent<StatusManager>().weaponAttackDamage = 0;
        Destroy(instaniatedWeapon);
        if (skill != null)
        {
            skill.RemoveSkill(source, 0);
        }
    }

    public override void OnDrop(GameObject source, Item item)
    {
        Debug.Log("Dropping " + item.id);
    }
}