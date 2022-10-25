using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fluid : MonoBehaviour
{
    private static int n = 30;
    private float dx = 500.0f / n;
    private float g = 5;

    private float[] h = new float[n];
    private float[] hu = new float[n];

    private float[] h_mid = new float[n];
    private float[] hu_mid = new float[n];
    
    private float[] dhdt = new float[n];
    private float[] dhudt = new float[n];

    private float[] dhdt_mid = new float[n];
    private float[] dhudt_mid = new float[n];
    
    private Mesh mesh;
    private Vector3[] vertices;
    private int[] triangles;
    
    void SweMidpoint(float dt)
    {
        // 1
        for (int i = 0; i < n-1; i++)
        {
            h_mid[i] = (h[i + 1] + h[i]) / 2;
            hu_mid[i] = (hu[i + 1] + hu[i]) / 2;
        }
        h_mid[0] = h_mid[n - 2];
        h_mid[n - 1] = h_mid[1];
        hu_mid[0] = hu_mid[n - 2];
        hu_mid[n - 1] = hu_mid[1];
        // 2
        for (int i = 0; i < n - 1; i++)
        {
            float dhudx_mid = (hu[i + 1] - hu[i]) / dx;
            dhdt_mid[i] = -dhudx_mid;
            
            float dhu2dx_mid = (Mathf.Pow(hu[i + 1], 2) / h[i + 1] - 
                            Mathf.Pow(hu[i], 2) / h[i]) / dx;
            float dgh2dx_mid = g*(Mathf.Pow(h[i + 1], 2) - Mathf.Pow(h[i], 2))/dx;
            dhudt_mid[i] = -(dhu2dx_mid + 0.5f * dgh2dx_mid);
        }
        // 3
        for (int i = 0; i < n-1; i++)
        {
            h_mid[i] += dhdt_mid[i]* (dt/2);
            hu_mid[i] += dhudt_mid[i] * (dt / 2);
        }
        // 4
        for (int i = 1; i < n - 1; i++)
        {
            float dhudx = (hu_mid[i] - hu_mid[i-1]) / dx;
            dhdt[i] = -dhudx;

            float dhu2dx = (Mathf.Pow(hu_mid[i], 2) / h_mid[i] -
                                Mathf.Pow(hu_mid[i-1], 2) / h_mid[i-1]) / dx;
            float dgh2dx = g * (Mathf.Pow(h_mid[i], 2) - Mathf.Pow(h_mid[i-1], 2)) / dx;
            dhudt[i] = -(dhu2dx + 0.5f * dgh2dx);
        }
        // 5
        float damp = 0.9f;
        for (int i = 0; i < n-1; i++)
        {
            h[i] += damp*dhdt[i]*dt;
            hu[i] += damp*dhudt[i] * dt;
        }
        //boundary
        h[0] = h[n - 2];
        h[n - 1] = h[1];
        hu[0] = hu[n - 2];
        hu[n - 1] = hu[1];
    }

    // Start is called before the first frame update
    void Start()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        for (int i = 0; i < h.Length; i++)
        {
            if (i < 15)
            {
                h[i] = 15;
            }
            else
            {
                h[i] = i*1f;
            }
        }
        CreateMesh();
    }

    private void OnDrawGizmos()
    {
        int z = 0;
        for (int i = 0; i < h.Length - 1; i++)
        {
            // print(h[i]);
            // Gizmos.DrawSphere(new Vector3(0, h[i], z), .1f);
            // z += 5;
            Gizmos.DrawLine(new Vector3(0, h[i], z), new Vector3(0, h[i + 1], z + 5));
            z += 5;
        }
    }

    void CreateMesh()
    {
        vertices = new Vector3[n * 2];
        int vertidx = 0;
        int z = 0;
        for (int i = 0; i < n; i++)
        {
            vertices[vertidx] = new Vector3(0, h[i], z);
            vertidx++;
            vertices[vertidx] = new Vector3(145, h[i], z);
            vertidx++;
            z += 5;
        }
    

        triangles = new int[(n-1) * 6];
        int vert = 0;
        int tris = 0;
        for (int i = 0; i < n-1; i++)
        {
            triangles[tris + 0] = vert + 0;
            triangles[tris + 1] = vert + 2;
            triangles[tris + 2] = vert + 1;
            triangles[tris + 3] = vert + 2;
            triangles[tris + 4] = vert + 3;
            triangles[tris + 5] = vert + 1;

            vert += 2;
            tris += 6;
        }

    }

    void UpdateMesh()
    {
        int vertidx = 0;
        int z = 0;
        for (int i = 0; i < n; i++)
        {
            vertices[vertidx] = new Vector3(0, h[i], z);
            vertidx++;
            vertices[vertidx] = new Vector3(145, h[i], z);
            vertidx++;
            z += 5;
        }
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
    }
    // Update is called once per frame
    void FixedUpdate()
    {
        float dt = 0.01f;
        float sim_dt = 0.001f;
        for (int i = 0; i < (int)(dt/sim_dt); i++){
            SweMidpoint(dt);
        }
        if (vertices != null)
        {
            UpdateMesh();
        }
    }
}
