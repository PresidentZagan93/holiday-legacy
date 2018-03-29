using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectContactDamage : MonoBehaviour
{
    Character character;
    public int amount = 1;
    public float hitRate = 0.5f;

    float nextDamage;

    private void Awake()
    {
        character = GetComponent<Character>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (character.isDead) return;

        if(character.isPlayer)
        {
            if(character.Armor.HasShield && character.Armor.ShieldEnabled && character.Armor.ContactDamage)
            {
                //all the requirements met, let the player deal damage
            }
            else
            {
                return;
            }
        }

        if(Time.time > nextDamage)
        {
            nextDamage = Time.time + hitRate;
            Health healthHit = collision.transform.GetComponentInParent<Health>();
            if (healthHit && healthHit.takeContactDamage && healthHit.Team != character.Team)
            {
                if (healthHit.Armor && healthHit.Armor.BlockContact(collision.contacts[0].point))
                {
                    return;
                }
                healthHit.Damage(amount, character);
            }
        }
    }
}
