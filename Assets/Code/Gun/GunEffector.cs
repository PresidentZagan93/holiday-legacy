using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Data;

public class GunEffector : MonoBehaviour {
	
	[System.Serializable]
	public class GunEffect
	{
		public Item item;
        public Consumable consumable;
		public List<string> particleEffects = new List<string>();
	}
	
	public List<GunEffect> effectBinds = new List<GunEffect>();
	GunEffects effects;
    GunShooter gunShooter;
	
	public void SetParent(GunEffects effects)
	{
        gunShooter = effects.GetComponent<GunShooter>();
		this.effects = effects;
	}
	
	void Update()
	{
        if (effects == null || effects.Inventory == null) return;

        if(gunShooter)
        {
            transform.localScale = new Vector3(1, gunShooter.direction.x > 0 ? 1 : -1, 1);
        }

		for (int e = 0; e < effects.effects.Count;e++)
		{
            //for loop the effects applied
			bool has = false;
			for (int i = 0; i < effects.Inventory.items.Count; i++)
			{
				for (int n = 0; n < effectBinds.Count;n++)
				{
					if(effectBinds[n].item && effectBinds[n].item.name == effects.Inventory.items[i].name)
					{
						has = effectBinds[n].particleEffects.Contains(effects.effects[e].name);
					}
				}
			}
            if(!has)
            {
                if(effects.Inventory.HasConsumable)
                {
                    for (int n = 0; n < effectBinds.Count; n++)
                    {
                        if (effects.Inventory.GetConsumable().Active && effectBinds[n].consumable && effectBinds[n].consumable.name == effects.Inventory.GetConsumable().item.name)
                        {
                            has = effectBinds[n].particleEffects.Contains(effects.effects[e].name);
                        }
                    }
                }
            }

            if (gunShooter && gunShooter.Gun)
            {
                has |= gunShooter.Gun.name == effects.effects[e].name;
            }
			
			effects.effects[e].transform.gameObject.SetActive(has);
		}
	}
}
