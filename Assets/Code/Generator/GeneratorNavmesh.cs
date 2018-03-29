using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

public class GeneratorNavmesh : MonoBehaviour
{
    [System.Serializable]
    public struct NavMeshDoor
    {
        public NavMeshObstacle obstacle;
        public Generator.GeneratorBlock block;
    }

    [System.Serializable]
    public struct NavMeshAgentOwner
    {
        public GameObject owner;
        public NavMeshAgent agent;
    }

    List<Vector3> newVertices = new List<Vector3>();
    List<int> newTriangles = new List<int>();
    List<Vector2> newUV = new List<Vector2>();

    List<Vector3> colVertices = new List<Vector3>();
    List<int> colTriangles = new List<int>();
    int colCount;

    Mesh mesh;
    MeshCollider col;
    NavMeshSurface surface;

    List<Vector2> data = new List<Vector2>();
    int squareCount;
    public static GeneratorNavmesh singleton;
    public List<NavMeshAgentOwner> agents = new List<NavMeshAgentOwner>();
    public List<NavMeshDoor> doors = new List<NavMeshDoor>();
    public Vector2Int offset;

    public static Vector2Int Offset
    {
        get
        {
            if (!singleton) singleton = FindObjectOfType<GeneratorNavmesh>();

            return singleton.offset;
        }
    }

    void Awake()
    {
        singleton = this;

        surface = GetComponent<NavMeshSurface>();
        mesh = GetComponent<MeshFilter>().mesh;
        col = GetComponent<MeshCollider>();
    }

    public static NavMeshObstacle GetDoor(int x, int y)
    {
        if (!singleton) singleton = FindObjectOfType<GeneratorNavmesh>();

        for (int i = 0; i < singleton.doors.Count; i++)
        {
            if (singleton.doors[i].block.x == x && singleton.doors[i].block.y == y)
            {
                return singleton.doors[i].obstacle;
            }
        }

        return null;
    }

    public static Vector3 ToWorld(Vector3 pos)
    {
        return new Vector3((pos.x + Offset.x) * 32f, (pos.z + Offset.y - 1) * 32f, pos.z);
    }

    public static Vector3 ToNavMesh(Vector3 pos)
    {
        if (!singleton) singleton = FindObjectOfType<GeneratorNavmesh>();

        pos = new Vector3(pos.x / 32f, 0, pos.y / 32f);
        pos -= new Vector3(singleton.offset.x, 0, singleton.offset.y + 1);

        return pos;
    }

    public static NavMeshAgent AddAgent(GameObject owner)
    {
        if (!singleton) singleton = FindObjectOfType<GeneratorNavmesh>();

        for (int i = 0; i < singleton.agents.Count;i++)
        {
            if (singleton.agents[i].owner == owner) return singleton.agents[i].agent;
        }

        Transform agentTransform = new GameObject(owner.name).transform;
        agentTransform.SetParent(singleton.transform);
        agentTransform.localPosition = ToNavMesh(owner.transform.position);

        NavMeshAgent agent = agentTransform.gameObject.AddComponent<NavMeshAgent>();
        agent.radius = 0.5f;
        agent.acceleration = 1000;
        agent.angularSpeed = 1000f;
        agent.Warp(agentTransform.position);

        NavMeshAgentOwner newAgent = new NavMeshAgentOwner()
        {
            owner = owner,
            agent = agent
        };
        singleton.agents.Add(newAgent);

        return newAgent.agent;
    }

    public static void RemoveAgent(GameObject owner)
    {
        if (!singleton) singleton = FindObjectOfType<GeneratorNavmesh>();

        if (!singleton) return;

        for (int i = 0; i < singleton.agents.Count; i++)
        {
            if (singleton.agents[i].owner == owner)
            {
                Destroy(singleton.agents[i].agent.gameObject);
                singleton.agents.RemoveAt(i);
            }
        }
    }

    private void OnDrawGizmos()
    {
        for(int i = 0; i < agents.Count;i++)
        {
            Gizmos.DrawCube(agents[i].agent.nextPosition, new Vector3(agents[i].agent.radius, agents[i].agent.height, agents[i].agent.radius));
        }
    }

    public static void RefreshObstacles()
    {
        if (!singleton) singleton = FindObjectOfType<GeneratorNavmesh>();
        
        for (int i = 0; i < singleton.doors.Count; i++)
        {
            singleton.doors[i].obstacle.enabled = singleton.doors[i].block.type == GeneratorBlockType.DoorClosed || singleton.doors[i].block.type == GeneratorBlockType.Wall;
        }
    }

    public static void Generate()
    {
        if (!singleton) singleton = FindObjectOfType<GeneratorNavmesh>();

        for (int i = 0; i < singleton.doors.Count; i++)
        {
            Destroy(singleton.doors[i].obstacle.gameObject);
        }
        singleton.doors.Clear();

        List<Generator.GeneratorBlock> blocks = Generator.singleton.blocks;
        int maxX = int.MinValue;
        int maxY = int.MinValue;
        int minX = int.MaxValue;
        int minY = int.MaxValue;

        for (int i = 0; i < blocks.Count; i++)
        {
            bool isFloor = blocks[i].type != GeneratorBlockType.Wall;
            if (isFloor)
            {
                if (blocks[i].x > maxX) maxX = blocks[i].x;
                if (blocks[i].x < minX) minX = blocks[i].x;
                if (blocks[i].y > maxY) maxY = blocks[i].y;
                if (blocks[i].y < minY) minY = blocks[i].y;
            }
        }
        
        singleton.offset.x = minX;
        singleton.offset.y = minY;

        singleton.data.Clear();

        for (int i = 0; i < blocks.Count; i++)
        {
            if (blocks[i].type != GeneratorBlockType.Wall || blocks[i].canDestroy)
            {
                //if (Generator.singleton.preset.blocks.max == 0)
                {
                    int x = blocks[i].x - singleton.offset.x;
                    int y = blocks[i].y - singleton.offset.y;

                    singleton.data.Add(new Vector2(x, y));

                    if (blocks[i].type == GeneratorBlockType.Door || blocks[i].type == GeneratorBlockType.DoorClosed || blocks[i].canDestroy)
                    {
                        NavMeshDoor newDoor = new NavMeshDoor()
                        {
                            block = blocks[i]
                        };

                        NavMeshObstacle newObstacle = new GameObject("Door").AddComponent<NavMeshObstacle>();
                        newObstacle.carving = true;
                        newObstacle.transform.SetParent(singleton.transform);
                        newObstacle.transform.localPosition = new Vector3(x, 0, y - 1) + new Vector3(0.5f, 0f, 0.5f);

                        newDoor.obstacle = newObstacle;

                        singleton.doors.Add(newDoor);
                    }
                }
            }
        }

        singleton.BuildMesh();
        singleton.UpdateMesh();
        singleton.surface.Bake();
        RefreshObstacles();
    }

    void BuildMesh()
    {
        for (int i = 0; i < singleton.data.Count; i++)
        {
            int x = (int)singleton.data[i].x;
            int y = (int)singleton.data[i].y;
            //GenCollider(x, y);
            GenSquare(x, y);
        }
    }

    bool HasBlock(int x, int y)
    {
        for (int i = 0; i < data.Count; i++)
        {
            if (data[i].x == x && data[i].y == y)
            {
                return true;
            }
        }

        return false;
    }

    void GenCollider(int x, int y)
    {
        //Top
        if (HasBlock(x, y + 1))
        {
            colVertices.Add(new Vector3(x, y, 1));
            colVertices.Add(new Vector3(x + 1, y, 1));
            colVertices.Add(new Vector3(x + 1, y, 0));
            colVertices.Add(new Vector3(x, y, 0));

            ColliderTriangles();

            colCount++;
        }

        //bot
        if (HasBlock(x, y - 1))
        {
            colVertices.Add(new Vector3(x, y - 1, 0));
            colVertices.Add(new Vector3(x + 1, y - 1, 0));
            colVertices.Add(new Vector3(x + 1, y - 1, 1));
            colVertices.Add(new Vector3(x, y - 1, 1));

            ColliderTriangles();
            colCount++;
        }

        //left
        if (HasBlock(x - 1, y))
        {
            colVertices.Add(new Vector3(x, y - 1, 1));
            colVertices.Add(new Vector3(x, y, 1));
            colVertices.Add(new Vector3(x, y, 0));
            colVertices.Add(new Vector3(x, y - 1, 0));

            ColliderTriangles();

            colCount++;
        }

        //right
        if (HasBlock(x + 1, y))
        {
            colVertices.Add(new Vector3(x + 1, y, 1));
            colVertices.Add(new Vector3(x + 1, y - 1, 1));
            colVertices.Add(new Vector3(x + 1, y - 1, 0));
            colVertices.Add(new Vector3(x + 1, y, 0));

            ColliderTriangles();

            colCount++;
        }

    }

    void ColliderTriangles()
    {
        colTriangles.Add(colCount * 4);
        colTriangles.Add((colCount * 4) + 1);
        colTriangles.Add((colCount * 4) + 3);
        colTriangles.Add((colCount * 4) + 1);
        colTriangles.Add((colCount * 4) + 2);
        colTriangles.Add((colCount * 4) + 3);
    }

    void GenSquare(int x, int y)
    {
        newVertices.Add(new Vector3(x, 0, y));
        newVertices.Add(new Vector3(x + 1, 0, y));
        newVertices.Add(new Vector3(x + 1, 0, y - 1));
        newVertices.Add(new Vector3(x, 0, y - 1));

        newTriangles.Add(squareCount * 4);
        newTriangles.Add((squareCount * 4) + 1);
        newTriangles.Add((squareCount * 4) + 3);
        newTriangles.Add((squareCount * 4) + 1);
        newTriangles.Add((squareCount * 4) + 2);
        newTriangles.Add((squareCount * 4) + 3);

        newUV.Add(Vector2.zero);
        newUV.Add(Vector2.zero);
        newUV.Add(Vector2.zero);
        newUV.Add(Vector2.zero);

        squareCount++;
    }

    void UpdateMesh()
    {
        mesh.Clear();
        mesh.vertices = newVertices.ToArray();
        mesh.triangles = newTriangles.ToArray();
        mesh.uv = newUV.ToArray();
        mesh.RecalculateNormals();

        newVertices.Clear();
        newTriangles.Clear();
        newUV.Clear();
        squareCount = 0;

        Mesh newMesh = new Mesh()
        {
            vertices = colVertices.ToArray(),
            triangles = colTriangles.ToArray()
        };
        if (col)
        {
            col.sharedMesh = newMesh;
            colVertices.Clear();
            colTriangles.Clear();
        }
        colCount = 0;
    }
}