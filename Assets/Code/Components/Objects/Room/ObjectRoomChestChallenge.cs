using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectRoomChestChallenge : ObjectRoom
{
    Transform chest;
    public bool challengeStart;
    int id;
    int timer = 0;
    float nextSecond;

    public int enemiesTotal;
    List<Character> enemies = new List<Character>();
    ObjectSoundEmitter sound;
    ObjectButton button;
    public int maxChallenges = 3;
    public int challenges;

    public override void InitializeRoom()
    {
        base.InitializeRoom();

        sound = gameObject.AddComponent<ObjectSoundEmitter>();
        sound.CreateSource("Room", AudioManager.AudioType.UI);

        id = Helper.RandomID;

        button = Instantiate(ObjectManager.GetPrefab("Button")).GetComponent<ObjectButton>();

        button.allowMultiplePresses = false;
        button.message = "START CHALLENGE";
        System.Action action = new System.Action(ButtonPress);
        button.Initialize(action);
        button.transform.position = rect.center;

        var middleBlock = Generator.FindBlock(rect.center.ToBlockPos().ToVector2Int());
        spawnsUsed.Add(middleBlock);
    }

    void ButtonPress()
    {
        if (challenges == maxChallenges)
        {
            return;
        }

        nextSecond = Time.time + 1f;
        timer = 5;
        open = false;
    }

    List<GameObject> tempProps = new List<GameObject>();

    private void Update()
    {
        if(GameManager.Paused)
        {
            nextSecond += Time.deltaTime;
            return;
        }

        if(!open)
        {
            if(!challengeStart)
            {
                UIManager.DrawText(id - 1, transform.position + Vector3.up * 24f, "CHALLENGE STARTS IN");
                if (Time.time > nextSecond)
                {
                    nextSecond = Time.time + 1f;
                    timer--;
                    if (timer == -1)
                    {
                        UIManager.DrawNotificationText(id + timer, transform.position + Vector3.up * 12f, "START!", 1f);
                        sound.PlaySound("BoxingRing", "Room");
                        challengeStart = true;

                        int enemiesToMake = Random.Range(10, 12) + (challenges * 2);
                        for (int i = 0; i < enemiesToMake; i++)
                        {
                            Character newEnemy = Instantiate(EnemyManager.GetEnemy("Clone").enemyPrefab).GetComponent<Character>();
                            
                            newEnemy.transform.position = GetFloorSpawn(false);
                            newEnemy.parentRoom = this;

                            enemies.Add(newEnemy);
                        }
                    }
                    else
                    {
                        UIManager.DrawNotificationText(id + timer, transform.position + Vector3.up * 12f, (timer + 1)+"", 0.8f);
                        sound.PlaySound("Fart2", "Room");
                    }
                }
            }
            else
            {
                for (int i = 0; i < enemies.Count; i++)
                {
                    if (!enemies[i] || enemies[i].isDead)
                    {
                        enemies.RemoveAt(i);
                        break;
                    }
                }

                if (enemies.Count == 0)
                {
                    if(challenges == maxChallenges)
                    {
                        button.buttonState = false;
                        button.pickedUp = true;

                        button.message = "";
                    }
                    else
                    {
                        button.buttonState = false;
                        button.pickedUp = false;
                    }

                    //remove all walls inside room

                    UIManager.DrawNotificationText(id, transform.position + Vector3.up * 12f, "CONGRATULATIONS!");
                    open = true;

                    ObjectChest chest = Instantiate(ObjectManager.GetPrefab("Chest")).GetComponent<ObjectChest>();
                    chest.transform.position = GetFloorSpawn();
                    tempProps.Add(chest.gameObject);

                    GameObject present = Instantiate(ObjectManager.GetPrefab("Present"));
                    present.transform.position = GetFloorSpawn();
                    tempProps.Add(present.gameObject);

                    chest = Instantiate(ObjectManager.GetPrefab("Chest")).GetComponent<ObjectChest>();
                    chest.transform.position = GetFloorSpawn();
                    tempProps.Add(chest.gameObject);

                    challenges++;
                    challengeStart = false;
                }
            }
        }
    }

    List<Generator.GeneratorBlock> spawnsUsed = new List<Generator.GeneratorBlock>();

    Vector2 GetFloorSpawn(bool useBlock = true)
    {
        Vector3 spawnPos = rect.center;
        var randomBlocks = blocks.Randomize();
        for (int i = 0; i < randomBlocks.Count; i++)
        {
            var block = randomBlocks[i];
            if (spawnsUsed.Contains(block)) continue;
            if (block.type == GeneratorBlockType.Floor || block.type == GeneratorBlockType.FloorAlt)
            {
                spawnPos = block.transform.position;
                if (useBlock)
                {
                    spawnsUsed.Add(block);
                }
                break;
            }
        }

        return spawnPos;
    }
}
