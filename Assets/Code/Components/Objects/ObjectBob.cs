using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectBob : MonoBehaviour {
	
	public float fps = 10f;
	public float fpsMultiplier = 1f;
    public int bobAmountVertical = 1;
	public bool matchFps = false;
	public bool matchFpsMultiplier = true;
    public bool bobHorizontal = true;
	float nextBob;
	
	SpritePlayer spritePlayer;
    CharacterArmor armor;
	bool bobDown = false;
    bool bobHoriz = false;
    Vector3 lastPos;
	
	void Awake()
	{
        spritePlayer = transform.parent.GetComponentInChildren<SpritePlayer>();
        armor = GetComponentInParent<CharacterArmor>();
    }
	
	void Update()
    {
        if (GameManager.Paused) return;

		if(spritePlayer)
		{
			if(matchFps) fps = spritePlayer.fps;
			if(matchFpsMultiplier) fpsMultiplier = spritePlayer.fpsMultiplier;
		}

        if (armor && spritePlayer)
        {
            transform.localPosition = spritePlayer.ArmorOffset;
            return;
        }

        if (Time.time > nextBob)
        {
            int bobAmountHorizontal = 0;
            if (lastPos != transform.position)
            {
                bobAmountHorizontal = Mathf.Clamp((transform.position - lastPos).magnitude * 0.1f, -1, 1).RoundToInt();
                if(bobAmountHorizontal != 0)
                {
                    transform.localPosition = new Vector3(0, transform.localPosition.y, transform.localPosition.z);
                }
                lastPos = transform.position;
            }

            nextBob = Time.time + (1f / fps * fpsMultiplier);
			
            if(bobHorizontal)
            {
                if(bobDown) bobHoriz = !bobHoriz;
                if(bobHoriz)
                {
                    transform.localPosition += Vector3.right * (bobDown ? -bobAmountHorizontal : bobAmountHorizontal);
                }
            }
            
            transform.localPosition += Vector3.up * (bobDown ? -bobAmountVertical : bobAmountVertical);

            bobDown = !bobDown;
		}
	}
}
