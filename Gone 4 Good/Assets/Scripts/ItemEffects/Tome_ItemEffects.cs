using UnityEngine;
using static Unity.VisualScripting.Member;

[CreateAssetMenu(fileName = "Tome_ItemEffects", menuName = "ScriptableObjects/ItemInteractionEffects/Tome_ItemEffects", order = 1)]
public class Tome_ItemEffects : ItemInteractionEffects
{
    public int attackDamage = 0;
    public GameObject weaponPrefab;
    private GameObject instaniatedWeapon;
    public Skill[] skill;
    public override void OnUse(GameObject source, Item item)
    {

    }

    public override void OnUseEnd(GameObject source,Item item)
    {

    }

    public override string EffectDescription(Item item)
    {
        string result = $"+{attackDamage} Attackdamage\n";
        result += "Grant's the use of the spell: " + skill[item.varriant].skillName;
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
            skill[item.varriant].AssignSkill(source, 0);
        }

    }

    public override void OnUnequip(GameObject source, Item item)
    {
        source.GetComponent<StatusManager>().weaponAttackDamage = 0;
        Destroy(instaniatedWeapon);
        if (skill != null)
        {
            skill[item.varriant].RemoveSkill(source, 0);
        }
    }

    public override void OnDrop(GameObject source, Item item)
    {
        Debug.Log("Dropping " + item.id);
    }
}