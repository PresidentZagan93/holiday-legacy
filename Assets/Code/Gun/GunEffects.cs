using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunEffects : GameBehaviour {
	
	[HideInInspector]
	public List<GunEffectTransform> effects = new List<GunEffectTransform>();

    [System.Serializable]
    public struct GunEffectTransform
    {
        public string name;
        public Transform transform;

        public GunEffectTransform(string name, Transform transform)
        {
            this.name = name;
            this.transform = transform;
        }
    }
	
	public override void Awake()
	{
        base.Awake();
		
		Transform child = null;
		Transform effectProp = Instantiate(ObjectManager.GetPrefab("GunEffects")).transform;
		effectProp.GetComponent<GunEffector>().SetParent(this);
		if(GunShooter)
		{
			effectProp.SetParent(GunShooter.root);
		}
		else
		{
			effectProp.SetParent(transform);
        }
        effectProp.localPosition = Vector3.zero;

        for (int i = 0; i < effectProp.childCount; i++)
		{
			child = effectProp.GetChild(i);
			if (child.parent == effectProp)
			{
				effects.Add(new GunEffectTransform(child.name, child));
			}
		}
	}
}
