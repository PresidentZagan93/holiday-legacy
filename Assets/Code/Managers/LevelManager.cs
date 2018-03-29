using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LevelManager : MonoBehaviour {

    public static LevelManager singleton;

    [System.Serializable]
    public class LevelEnemy
    {
        [Enemy]
        public string name = "";
        [Range(0,100)]
        public int chance = 50;
        public int min = 1;
    }

    [System.Serializable]
    public class Level
    {
        public string levelName = "Welcome to Hell";
        
        [Tileset]
        public string tileset = "Area";

        public IntRange enemyAmount = new IntRange(8, 12);

        public List<LevelEnemy> enemies = new List<LevelEnemy>();
        [Enemy]
        public List<string> bosses = new List<string>();
    }

    [SerializeField]
    List<Level> order = new List<Level>();

    public static List<Level> Order
    {
        get
        {
            return singleton.order;
        }
    }

    void Awake()
    {
        singleton = this;
    }

    private void OnEnable()
    {
        singleton = this;
    }

    public static Level GetLevel(int stage)
    {
        return Order[stage % Order.Count];
    }
}
