using UnityEngine;

public class ObjectHealthKit : ObjectPickup {

    public int amount = 1;

    private void Awake()
    {
        fadePoint = new Vector3(-130f, -120f);
    }

    public override bool CanPickup(CharacterPickupMaster character)
    {
        return character.Health.hp < character.Health.maxHp;
    }

    public override bool DoPickup(CharacterPickupMaster character)
    {
        character.Collect(this);

        UIManager.DrawNotificationText(Helper.RandomID, transform.position, "<color=lime>+"+amount+" HP!</color>", 0.5f);
        GameManager.AddScore(50);

        return true;
    }
}
