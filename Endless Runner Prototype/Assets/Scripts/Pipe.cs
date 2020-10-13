﻿using System;
using UnityEngine;

public class Pipe : MonoBehaviour
{
    // Use this for initialization
    public float torusRadius, RingDistance;
    public int torusSegmentCount;

    public float MinCurveRadaius, MaxCurveRadaius;
    public int MinCurveSegmentCount, MaxCurveSegmentCount;
    public PipeItemGenerator[] generators;

    //Mesh Render
    private Mesh mesh;
    private Vector3[] vertices;
    private int[] triangles;

    private float curveAngle;
    private float curveRadius;
    private int curveSegmentCount;
    private float relativeRotation;
    private Vector2[] uv;


    public int CurveSegmentCount
    {
        get
        {
            return curveSegmentCount;
        }
    }
    public float RelativeRotation
    {
        get
        {
            return relativeRotation;
        }
    }

    public float CurveRadius
    {
        get
        {
            return curveRadius;
        }
    }

    public float CurveAngle
    {
        get
        {
            return curveAngle;
        }
    }
    private void Awake()
    {
        GetComponent<MeshFilter>().mesh = mesh = new Mesh();
        mesh.name = "Torus";
    }

    public void Generate(bool withterms = true)
    {
        curveRadius = UnityEngine.Random.Range(MinCurveRadaius, MaxCurveRadaius);
        curveSegmentCount = UnityEngine.Random.Range(MinCurveSegmentCount, MaxCurveSegmentCount);
        mesh.Clear();
        SetVertices();
        SetUV();
        SetTriangles();
        mesh.RecalculateNormals();

        for (int i = 0; i < transform.childCount; i++)
        {
            Destroy(transform.GetChild(i).gameObject);
        }
        if (withterms)
        {
            generators[UnityEngine.Random.Range(0, generators.Length)].GenerateItems(this);
        }
    }
    


        void SetUV()
        {
            uv = new Vector2[vertices.Length];
            for (int i = 0; i < vertices.Length; i += 4)
            {
                uv[i] = Vector2.zero;
                uv[i + 1] = Vector2.right;
                uv[i + 2] = Vector2.up;
                uv[i + 3] = Vector2.one;
            }

            mesh.uv = uv;
        }

        public void AlignWith(Pipe pipe)
        {
            relativeRotation = UnityEngine.Random.Range(0, curveSegmentCount) * 360f / torusSegmentCount;
            transform.SetParent(pipe.transform, false);
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.Euler(0f, 0f, -pipe.curveAngle);
            transform.Translate(0f, pipe.curveRadius, 0f);
            transform.Rotate(relativeRotation, 0f, 0f);
            transform.Translate(0f, -curveRadius, 0f);
            transform.SetParent(pipe.transform.parent);
            transform.localScale = Vector3.one;
        }

        //Create Torus
        private Vector3 GetPointOnTorus(float u, float v)
        {
            Vector3 p;
            float r = (curveRadius + torusRadius * Mathf.Cos(v));
            p.x = r * Mathf.Sin(u);
            p.y = r * Mathf.Cos(u);
            p.z = torusRadius * Mathf.Sin(v);
            return p;
        }

        //Draw Torus
        /* private void OnDrawGizmos()
         {


             float uStep = (2f * Mathf.PI) / curveSegmentCount;
             float vStep = (2f * Mathf.PI) / torusSegmentCount;

             for (int u = 0; u < curveSegmentCount; u++)
             {
                 for (int v = 0; v < torusSegmentCount; v++)
                 {
                     Vector3 point = GetPointOnTorus(u * uStep, v * vStep);
                     Gizmos.color = new Color(1f, (float)v / torusSegmentCount, (float)u / curveSegmentCount);
                     Gizmos.DrawSphere(point, 0.1f);
                 }
             }
         }*/

        //each quad share vertices with its neighbors, or give each quad its own four vertices.
        private void SetVertices()
        {
            vertices = new Vector3[torusSegmentCount * curveSegmentCount * 4];
            float uStep = RingDistance / curveRadius;
            curveAngle = uStep * curveSegmentCount * (360 / (2f * Mathf.PI));
            CreateFirstQuadRing(uStep);
            int iDelta = torusSegmentCount * 4;
            for (int u = 2, i = iDelta; u <= curveSegmentCount; u++, i += iDelta)
            {
                CreateQuadRing(u * uStep, i);
            }
            mesh.vertices = vertices;
        }

        //Each quad has two triangles, so six vertex indices.
        private void SetTriangles()
        {
            triangles = new int[torusSegmentCount * curveSegmentCount * 6];
            for (int t = 0, i = 0; t < triangles.Length; t += 6, i += 4)
            {
                triangles[t] = i;
                triangles[t + 1] = triangles[t + 4] = i + 2;
                triangles[t + 2] = triangles[t + 3] = i + 1;
                triangles[t + 5] = i + 3;
            }
            mesh.triangles = triangles;
        }

        private void CreateFirstQuadRing(float u)
        {
            float vStep = (2f * Mathf.PI) / torusSegmentCount;

            Vector3 vertexA = GetPointOnTorus(0f, 0f);
            Vector3 vertexB = GetPointOnTorus(u, 0f);
            for (int v = 1, i = 0; v <= torusSegmentCount; v++, i += 4)
            {
                vertices[i] = vertexA;
                vertices[i + 1] = vertexA = GetPointOnTorus(0f, v * vStep);
                vertices[i + 2] = vertexB;
                vertices[i + 3] = vertexB = GetPointOnTorus(u, v * vStep);
            }
        }

        private void CreateQuadRing(float u, int i)
        {
            float vStep = (2f * Mathf.PI) / torusSegmentCount;
            int ringOffset = torusSegmentCount * 4;

            Vector3 vertex = GetPointOnTorus(u, 0f);
            for (int v = 1; v <= torusSegmentCount; v++, i += 4)
            {
                vertices[i] = vertices[i - ringOffset + 2];
                vertices[i + 1] = vertices[i - ringOffset + 3];
                vertices[i + 2] = vertex;
                vertices[i + 3] = vertex = GetPointOnTorus(u, v * vStep);
            }
        }
    }
