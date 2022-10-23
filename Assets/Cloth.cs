using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;

[RequireComponent(typeof(MeshFilter))]
public class Cloth : MonoBehaviour
{
    // Start is called before the first frame update
    float floor = 500f;
    Vector3 gravity = new(0, -10, 0);
    float radius = 1f;
    Vector3 stringTop = new(0, 10, 0);
    float restLen = 3f;
    float mass = 1.0f;
    float k = 300;
    float kv = 30;
    public Material ballMaterial;
    public GameObject backsideMeshObject;

    Vector3 spherePos = new(7, -7, 17);
    float sphereRadius = 7;

    //Initial positions and velocities of masses
    static int maxNodes = 100;
    Vector3[,] pos = new Vector3[maxNodes, maxNodes];
    Vector3[,] vel = new Vector3[maxNodes, maxNodes];
    Vector3[,] acc = new Vector3[maxNodes, maxNodes];

    int numNodes = 10;

    private Mesh mesh;
    private Mesh meshback;
    private Vector3[] vertices;
    private int[] triangles;
    private int[] trianglesback;

    void Start()
    {
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.transform.position = spherePos;
        sphere.transform.localScale = new Vector3(sphereRadius * 2, sphereRadius * 2, sphereRadius * 2);
        sphere.GetComponent<Renderer>().material = ballMaterial;
        mesh = new Mesh();
        meshback = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        backsideMeshObject.GetComponent<MeshFilter>().mesh = meshback;
        CreateNodes();
        CreateMesh();
        UpdateMesh();
    }

    void CreateNodes()
    {
        for (int i = 0; i < numNodes; i++)
        {
            for (int j = 0; j < numNodes; j++)
            {
                pos[i, j] = new Vector3(0, 0, 0);
                pos[i, j].x = stringTop.x + 2 * i;
                pos[i, j].z = stringTop.z + 2 * j;
                pos[i, j].y = stringTop.y;
                vel[i, j] = new Vector3(0, 0, 0);
            }
        }
    }

    void CreateMesh()
    {
        vertices = new Vector3[numNodes * numNodes];
        int vertidx = 0;
        for (int i = 0; i < numNodes; i++)
        {
            for (int j = 0; j < numNodes; j++)
            {
                vertices[vertidx] = pos[i, j];
                vertidx++;
            } 
        }

        triangles = new int[(numNodes - 1) * (numNodes - 1) * 6];
        int vert = 0;
        int tris = 0;
        for (int z = 0; z < numNodes - 1; z++)
        {
            for (int x = 0; x < numNodes - 1; x++)
            {
                triangles[tris + 0] = vert + 1;
                triangles[tris + 1] = vert + numNodes;
                triangles[tris + 2] = vert + 0;
                triangles[tris + 3] = vert + numNodes + 1;
                triangles[tris + 4] = vert + numNodes;
                triangles[tris + 5] = vert + 1;

                vert++;
                tris += 6;
            }
            vert++;
        }

        trianglesback = new int[(numNodes - 1) * (numNodes - 1) * 6];
        vert = 0;
        tris = 0;
        for (int z = 0; z < numNodes - 1; z++)
        {
            for (int x = 0; x < numNodes - 1; x++)
            {
                trianglesback[tris + 0] = vert + 0;
                trianglesback[tris + 1] = vert + numNodes;
                trianglesback[tris + 2] = vert + 1;
                trianglesback[tris + 3] = vert + 1;
                trianglesback[tris + 4] = vert + numNodes;
                trianglesback[tris + 5] = vert + numNodes + 1;

                vert++;
                tris += 6;
            }

            vert++;
        }
    }

    void UpdateMesh()
    {
        int vertidx = 0;
        for (int i = 0; i < numNodes; i++)
        {
            for (int j = 0; j < numNodes; j++)
            {
                vertices[vertidx] = pos[i, j];
                vertidx++;
            } 
        }
        mesh.Clear();
        meshback.Clear();
        mesh.vertices = vertices;
        meshback.vertices = vertices;
        mesh.triangles = triangles;
        meshback.triangles = trianglesback;
        mesh.RecalculateNormals();
        meshback.RecalculateNormals();
    }

    void UpdateNodes(float dt)
    {
        //Reset accelerations each timestep (momenum only applies to velocity)
        for (int i = 0; i < numNodes; i++)
        {
            for (int j = 0; j < numNodes; j++)
            {
                acc[i, j] = new Vector3(0, 0, 0);
                acc[i, j] += gravity;
            }
        }

        //Compute (damped) Hooke's law for each spring
        for (int i = 0; i < numNodes; i++)
        {
            for (int j = 0; j < numNodes - 1; j++)
            {
                Vector3 diff = pos[i, j + 1] - pos[i, j];
                float stringF = -k * (diff.magnitude - restLen);
                //println(stringF,diff.length(),restLen);

                Vector3 stringDir = diff.normalized;
                float projVbot = Vector3.Dot(vel[i, j], stringDir);
                float projVtop = Vector3.Dot(vel[i, j + 1], stringDir);
                float dampF = -kv * (projVtop - projVbot);

                Vector3 force = stringDir * (stringF + dampF);
                acc[i, j] += (force * (-1.0f / mass));
                acc[i, j + 1] += (force * (1.0f / mass));
            }
        }

        for (int i = 0; i < numNodes - 1; i++)
        {
            for (int j = 0; j < numNodes; j++)
            {
                Vector3 diff = pos[i + 1, j] - (pos[i, j]);
                float stringF = -k * (diff.magnitude - restLen);
                //println(stringF,diff.length(),restLen);

                Vector3 stringDir = diff.normalized;
                float projVbot = Vector3.Dot(vel[i, j], stringDir);
                float projVtop = Vector3.Dot(vel[i + 1, j], stringDir);
                float dampF = -kv * (projVtop - projVbot);

                Vector3 force = stringDir * (stringF + dampF);
                acc[i, j] += (force * (-1.0f / mass));
                acc[i + 1, j] += (force * (1.0f / mass));
            }
        }

        //Eulerian integration
        for (int i = 0; i < numNodes; i++)
        {
            for (int j = 1; j < numNodes; j++)
            {
                vel[i, j] += (acc[i, j] * (dt));
                pos[i, j] += (vel[i, j] * (dt));
            }
        }

        //Collision detection and response
        for (int i = 0; i < numNodes; i++)
        {
            for (int j = 0; j < numNodes; j++)
            {
                if (pos[i, j].y > floor)
                {
                    vel[i, j].y *= -.9f;
                    pos[i, j].y = floor;
                }

                if (Vector3.Distance(pos[i, j], spherePos) < (sphereRadius + radius))
                {
                    Vector3 normal = (pos[i, j] - (spherePos)).normalized;
                    pos[i, j] = spherePos + (normal * ((sphereRadius + radius) * (1.01f)));
                    Vector3 velNormal = normal * (Vector3.Dot(vel[i, j], normal));
                    vel[i, j] -= (velNormal * (0.7f));
                }
            }
        }
    }

    // private void OnDrawGizmos()
    // {
    //     if (vertices == null)
    //     {
    //         return;
    //     }
    //
    //     foreach (var t in vertices)
    //     {
    //         Gizmos.DrawSphere(t, .1f);
    //     }
    // }
    private void Update()
    {
        if (Input.GetKeyDown("r"))
        {
            CreateNodes();
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        for (int i = 0; i < 20; i++)
        {
            UpdateNodes(Time.fixedDeltaTime * (1f/20f));
        }

        if (vertices != null)
        {
            UpdateMesh();
        }
    }
}