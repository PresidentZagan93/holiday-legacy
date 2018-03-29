using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectRoomBoss : ObjectRoom {

    public string bossName = "";
    public Character boss;
    ObjectSoundEmitter sound;
    bool lastBossOpen;
    bool firstEnter;
    bool bossDied;

    public override void InitializeRoom()
    {
        base.InitializeRoom();
        sound = gameObject.AddComponent<ObjectSoundEmitter>();
        sound.CreateSource("Room", AudioManager.AudioType.UI);
    }

    private void Update()
    {
        open = EnemyManager.EnemiesAlive == 0;

        if (boss && !boss.isDead)
        {
            for (int i = 0; i < checkersInside.Count; i++)
            {
                if (checkersInside[i] && checkersInside[i].room.transform == transform && checkersInside[i].character && checkersInside[i].character.isPlayer)
                {
                    if (!firstEnter)
                    {
                        sound.PlaySound("BoxingRing", "Room");
                        firstEnter = true;
                        GameObject.Find("BossHealthRoot").GetComponent<InterfaceHealthBar>().Assign(boss.Health);
                    }

                    open = false;
                }
            }
        }
        else
        {
            if (!bossDied && firstEnter)
            {
                sound.PlaySound("BossWin", "Room");
                GameObject.Find("BossHealthRoot").GetComponent<InterfaceHealthBar>().Assign(null);
                bossDied = true;
            }
            open = true;
        }

        if(lastBossOpen != open)
        {
            lastBossOpen = open;
            if(open && !GeneratorManager.Generating && boss && !boss.isDead)
            {
                sound.PlaySound("DeepHorn", "Room");
            }
        }
    }
}
