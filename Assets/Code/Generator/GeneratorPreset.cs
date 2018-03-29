using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

[Serializable]
public class GeneratorProp
{
    public IntRange amount = new IntRange(1, 1);
    public GameObject prop;
    public bool randomOrient = true;
    public bool ignoreContains = false;
}

[Serializable]
public class GeneratorTile
{
    [Serializable]
    public class GeneratorTileSingle
    {
        public GameObject tile;
        public int chance = 100;
    }

    [HideInInspector]
    public string idName = "";
    public int id = 0;
    public List<GeneratorTileSingle> tiles = new List<GeneratorTileSingle>();

    public static GeneratorTileSingle Random(GeneratorTileSingle[] tiles)
    {
        List<int> possibleTiles = new List<int>();
        for (int e = 0; e < tiles.Length; e++)
        {
            for (int ec = 0; ec < tiles[e].chance; ec++)
            {
                possibleTiles.Add(e);
            }
        }

        int tileIndex = UnityEngine.Random.Range(0, possibleTiles.Count);
        return tiles[possibleTiles[tileIndex]];
    }
}

public class GeneratorPreset : MonoBehaviour
{
    [Serializable]
    public class GeneratorRoomTypePointer
    {
        public Type Type
        {
            get
            {
                return Type.GetType(typeClassName);
            }
        }
        public string typeClassName;
    }

    [Serializable]
    public class GeneratorRoom
    {
        [Tileset]
        public string tileset = "Room";

        public RoomDoorType doorType = RoomDoorType.Open;
        public bool usesKey = false;
        public int blocks = 0;
        public GeneratorRoomTypePointer type = new GeneratorRoomTypePointer();
        public IntRange amount = new IntRange(3, 5);
        public IntRange dimension = new IntRange(4, 6);
    }

    [Serializable]
    public class GeneratorEffects
    {
        public AudioClip ambience;
        public bool underground = true;

        public AudioReverbPreset reverb = AudioReverbPreset.Concerthall;

        public Color minimapColor = Color.white;
        public Color minimapOutline = Color.white;

        public Color ambientShadingColor = Color.black;
        public Color backgroundColor = Color.white;

        public Sprite overlay;
    }

    public Sprite banner;

    [Tileset]
    public string tileset = "Sewers";
    [AlternateTileset]
    public string alternateTileset = "Sewers2";

    public List<GeneratorRoom> rooms = new List<GeneratorRoom>();
    
    public GeneratorEffects effects;

    public List<GeneratorProp> props = new List<GeneratorProp>();
    public List<Generator.GeneratorWallProp> propsWall = new List<Generator.GeneratorWallProp>();
    public List<GeneratorProp> pickups = new List<GeneratorProp>();

    public int turns = 60;
    public int altStreakChance = 20;
    public int altStreakAmount = 6;

    public int bloatChance = 0;
    public int bloatSize = 2;

    public int turnChance;

    public GeneratorRoom GetRoom(Type type)
    {
        for(int i = 0; i < rooms.Count;i++)
        {
            if(rooms[i].type.Type == type)
            {
                return rooms[i];
            }
        }

        return null;
    }
}
