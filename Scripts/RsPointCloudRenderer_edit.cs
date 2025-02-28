using System;
using UnityEngine;
using Intel.RealSense;
using UnityEngine.Rendering;
using UnityEngine.Assertions;
using UnityEngine.UI;
using System.Runtime.InteropServices;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class RsPointCloudRenderer_edit : MonoBehaviour
{
    public RsFrameProvider Source;
    private Mesh mesh;
    private Texture2D uvmap;
    public GameObject EnclosingHand;


    [NonSerialized]
    private Vector3[] vertices;
    private List<CapsuleCollider> colliderlist;
    // private Vector3[] normals;
    // private Vector3[] phoneVertices;

    FrameQueue q;

    void Start()
    {
        Source.OnStart += OnStartStreaming;
        Source.OnStop += Dispose;

        colliderlist = new List<CapsuleCollider>();
        if (EnclosingHand != null)
        {
            colliderlist.AddRange(EnclosingHand.GetComponentsInChildren<CapsuleCollider>());
        }
    }

    private void OnStartStreaming(PipelineProfile obj)
    {
        q = new FrameQueue(1);

        using (var depth = obj.Streams.FirstOrDefault(s => s.Stream == Stream.Depth && s.Format == Format.Z16).As<VideoStreamProfile>())
            ResetMesh(depth.Width, depth.Height);

        Source.OnNewSample += OnNewSample;
    }

    private void ResetMesh(int width, int height)
    {
        Assert.IsTrue(SystemInfo.SupportsTextureFormat(TextureFormat.RGFloat));
        uvmap = new Texture2D(width, height, TextureFormat.RGFloat, false, true)
        {
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Point,
        };
        GetComponent<MeshRenderer>().sharedMaterial.SetTexture("_UVMap", uvmap);

        if (mesh != null)
            mesh.Clear();
        else
            mesh = new Mesh()
            {
                indexFormat = IndexFormat.UInt32,
            };

        vertices = new Vector3[width * height];
        // normals = new Vector3[width * height];

        var indices = new int[vertices.Length];
        for (int i = 0; i < vertices.Length; i++)
            indices[i] = i;

        mesh.MarkDynamic();
        mesh.vertices = vertices;
        // mesh.normals = normals;

        var uvs = new Vector2[width * height];
        Array.Clear(uvs, 0, uvs.Length);
        for (int j = 0; j < height; j++)
        {
            for (int i = 0; i < width; i++)
            {
                uvs[i + j * width].x = i / (float)width;
                uvs[i + j * width].y = j / (float)height;
            }
        }

        mesh.uv = uvs;

        mesh.SetIndices(indices, MeshTopology.Points, 0, false);
        mesh.bounds = new Bounds(Vector3.zero, Vector3.one * 10f);

        GetComponent<MeshFilter>().sharedMesh = mesh;
    }

    void OnDestroy()
    {
        if (q != null)
        {
            q.Dispose();
            q = null;
        }

        if (mesh != null)
            Destroy(null);
    }

    private void Dispose()
    {
        Source.OnNewSample -= OnNewSample;

        if (q != null)
        {
            q.Dispose();
            q = null;
        }
    }

    private void OnNewSample(Frame frame)
    {
        if (q == null)
            return;
        try
        {
            if (frame.IsComposite)
            {
                using (var fs = frame.As<FrameSet>())
                using (var points = fs.FirstOrDefault<Points>(Stream.Depth, Format.Xyz32f))
                {
                    if (points != null)
                    {
                        q.Enqueue(points);
                    }
                }
                return;
            }

            if (frame.Is(Extension.Points))
            {
                q.Enqueue(frame);
            }
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }
    
    protected void LateUpdate()
    {
        if (q != null)
        {
            Points points;
            if (q.PollForFrame<Points>(out points))
                using (points)
                {
                    if (points.Count != mesh.vertexCount)
                    {
                        using (var p = points.GetProfile<VideoStreamProfile>())
                            ResetMesh(p.Width, p.Height);
                    }

                    if (points.TextureData != IntPtr.Zero)
                    {
                        uvmap.LoadRawTextureData(points.TextureData, points.Count * sizeof(float) * 2);
                        uvmap.Apply();
                    }

                    if (points.VertexData != IntPtr.Zero)
                    {
                        Debug.Log("Number of vertices: " + vertices.Length);
                        points.CopyVertices(vertices);

                        Debug.Log("Number of vertices: " + vertices.Length);
                        Debug.Log("vertice type: " + vertices.GetType().ToString());
                        // Remove points that are inside the enclosing mesh
                        if (colliderlist != null)
                        {
                            Debug.Log("Number of Colliders found: " + colliderlist.Count);
                        }

                        // vertices = FilterCollider(vertices, colliderlist);


                        //     List<Vector3> validVertices = new List<Vector3>();
                        //     for (int i = 0; i < vertices.Length; i++)
                        //     {
                        //         // Check if the point is inside the collider
                        //         if (!EnclosingHandCollider.bounds.Contains(vertices[i]))
                        //         {
                        //             validVertices.Add(vertices[i]);
                        //         }
                        //     }
                        //     Debug.Log("filtered vertices:" + validVertices.Count);

                        //     // Update mesh with the remaining points
                        //     mesh.Clear();
                        //     mesh.vertices = validVertices.ToArray();

                        //     // Update indices for the remaining points
                        //     var indices = new int[validVertices.Count];
                        //     for (int i = 0; i < validVertices.Count; i++)
                        //     {
                        //         indices[i] = i;
                        //     }

                        //     mesh.SetIndices(indices, MeshTopology.Points, 0);
                        // }

                        mesh.vertices = vertices;
                        mesh.UploadMeshData(false);
                    }
                }
        }
    }  

    Vector3[] FilterCollider(Vector3[] allpoints, List<CapsuleCollider> colliders)
    {
        List<Vector3> FilteredPoints = new List<Vector3>();

        foreach (var point in allpoints)
        {
            bool isInsideAnyCollider = false;

            foreach (var collider in colliders)
            {
                Vector3 closestPoint = collider.ClosestPoint(point);
                if (Vector3.Distance(closestPoint, point) < Mathf.Epsilon) // point lies within the collider
                {
                    isInsideAnyCollider = true;
                    break;
                }
            }

            if (!isInsideAnyCollider)
            {
                FilteredPoints.Add(point);
            }
        }

        return FilteredPoints.ToArray();
    }
      

    // cull mesh by normals
    // protected void LateUpdate()
    // {
    //     Debug.Log("frame is updating");
    //     if (q != null)
    //     {
    //         Points points;
    //         if (q.PollForFrame<Points>(out points))
    //             using (points)
    //             {
    //                 if (points.Count != mesh.vertexCount)
    //                 {
    //                     using (var p = points.GetProfile<VideoStreamProfile>())
    //                         ResetMesh(p.Width, p.Height);
    //                 }

    //                 if (points.TextureData != IntPtr.Zero)
    //                 {
    //                     uvmap.LoadRawTextureData(points.TextureData, points.Count * sizeof(float) * 2);
    //                     uvmap.Apply();
    //                 }

    //                 if (points.VertexData != IntPtr.Zero)
    //                 {
    //                     points.CopyVertices(vertices);
    //                     mesh.vertices = vertices;
    //                     mesh.RecalculateNormals();
    //                     normals = mesh.normals;

    //                     phoneVertices = VerticesWithCommonNormals();

    //                     mesh.Clear();
    //                     mesh.vertices = phoneVertices;
    //                     mesh.RecalculateNormals();
    //                     mesh.UploadMeshData(false);
    //                 }
    //             }
    //     }
    // }
    // 
    // Vector3[] VerticesWithCommonNormals()
    // {
    //     if (vertices.Length == 0 || normals.Length ==0)
    //     {
    //         Debug.Log("Mesh has no vertices or normals");
    //         return new Vector3[0];
    //     }

    //     Dictionary<Vector3, List<int>> normalToVertices = new Dictionary<Vector3, List<int>>();

    //     for(int i = 0; i < normals.Length; i++)
    //     {
    //         Vector3 normal = normals[i].normalized;

    //         if (!normalToVertices.ContainsKey(normal))
    //         {
    //             normalToVertices[normal] = new List<int>();
    //         }
    //         normalToVertices[normal].Add(i);
    //     }

    //     Vector3 mostCommonNormal = normalToVertices.OrderByDescending(kvp => kvp.Value.Count).First().Key;
    //     List<int> mostCommonNormalVertexIndices = normalToVertices[mostCommonNormal];

    //     Vector3[] commonNormalVertices = new Vector3[mostCommonNormalVertexIndices.Count];

    //     for (int i = 0; i< mostCommonNormalVertexIndices.Count; i++)
    //     {
    //         int vertexIndex = mostCommonNormalVertexIndices[i];
    //         commonNormalVertices[i] = vertices[vertexIndex];
    //     }
    //     return commonNormalVertices;

    // }

}