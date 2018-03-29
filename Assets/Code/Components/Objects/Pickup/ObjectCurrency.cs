using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Currency
{
    Gold,
    Key
}

public class ObjectCurrency : ObjectPickup {

    public Currency type = Currency.Gold;
    public AnimationCurve curve = new AnimationCurve();
    public Transform coin;
    float coinTime;

    private void Awake()
    {
        fadePoint = new Vector3(GameManager.GameWidth * -0.5f, 480f);
    }

    public override bool CanPickup(CharacterPickupMaster character)
    {
        return !pickedUp;
    }

    private void Update()
    {
        if (GameManager.Paused) return;

        coinTime += Time.deltaTime * Random.Range(0.46f, 0.54f);
        if (coin) coin.localPosition = new Vector2(0, curve.Evaluate(coinTime) * 24f);
    }

    public override bool DoPickup(CharacterPickupMaster character)
    {
        if (pickedUp) return false;
        
        character.Collect(this);
        
        GameManager.AddScore(10);

        return true;
    }
}
