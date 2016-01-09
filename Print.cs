// print
// 
// Copyright (c) 2016 Robin Southern -- github.com/r57s/print
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Camera))]
class Print : MonoBehaviour
{
    
    public static int Scale
    {
        get { return scale; }
        set {
            scale = value;
            if (scale < 1)
                scale = 1;
            tr = Matrix4x4.TRS(new Vector3(-Screen.width * 0.5f, Screen.height * 0.5f, 0.0f), Quaternion.identity, new Vector2(scale, -scale));
        }
    }
    
    void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("Cannot have two Print Monobehaviours in the same scene!");
            return;
        }
        
        if (Texture == null)
        {
            Texture = new Texture2D(128, 128, TextureFormat.ARGB32, false, false); 
            Texture.hideFlags = HideFlags.HideAndDontSave;
            Texture.LoadImage(System.Convert.FromBase64String(EncodedTexture));
            Texture.filterMode = FilterMode.Point;
            
            material = new Material(Shader.Find("Hidden/Internal-GUITexture"));
            material.mainTexture = Texture;
        }
        
        Cam = GetComponent<Camera>();
        
        if (Cam == Camera.main)
        {
            Debug.LogError("Print MonoBehaviour should not be attached to the 'MainCamera'. It should be on a secondary camera");
        }
        
        Cam.depth = 200;
        Cam.clearFlags = CameraClearFlags.Nothing;
        Cam.orthographic = true;
        Cam.nearClipPlane = 0.0f;
        Cam.farClipPlane = 1.0f;   
        Cam.orthographicSize = Screen.height * 0.5f;
        lastHeight = Screen.height;
        
        vertices = new List<Vector3>();
        indexes = new List<int>();
        uvs = new List<Vector2>();
        
        mesh = new Mesh();
        mesh.MarkDynamic();
        
        tr = Matrix4x4.TRS(new Vector3(-Screen.width * 0.5f, Screen.height * 0.5f, 0.0f), Quaternion.identity, new Vector2(scale, -scale));
    }
    
    void Update()
    {
        if (lastHeight != Screen.height)
        {
            lastHeight = Screen.height;
            Cam.orthographicSize = Screen.height * 0.5f;
            tr = Matrix4x4.TRS(new Vector3(-Screen.width * 0.5f, Screen.height * 0.5f, 0.0f), Quaternion.identity, new Vector2(scale, -scale));
        }
        
        if (draw)
        {
            draw = false;       
            mesh.SetVertices(vertices);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(indexes, 0);          
            mesh.RecalculateBounds();
        }
    }
    
    void OnPostRender()
    {
      material.SetPass(0);
      Graphics.DrawMeshNow(mesh, tr);
    }
    
    public static void Clear()
    {
        vertices.Clear();
        indexes.Clear();
        uvs.Clear();
        draw = true;
        index = 0;
    }
    
    public static void Text(int x, int y, string text)
    {
        float xx = x / (float) scale;
        float yy = y / (float) scale;
        foreach(char c in text)
        {
            if (c == ' ')
            {
                xx += 8;
                continue;
            }
            
            if (c <= ' ' || c > '}')
                continue;
            
            PushChar(xx, yy, c);
            xx += 8;          
        }
        draw = true;
    }
    
    public static void Format(int x, int y, string format, params object[] args)
    {
        Text(x, y, string.Format(format, args));
    }
    
    public static void Value(int x, int y, object obj)
    {
        Text(x, y, obj == null ? "null" : obj.ToString());
    }
    
#region Internal

    static Vector3 tP0, tP1, tP2, tP3;
    static Vector2 tU0, tU1, tU2, tU3; 
    
    const float w = 8;
    
    const float h = 8;
    Camera Cam;

    static int lastHeight = 0;
    
    static bool draw;

    static List<Vector3> vertices;
    static List<int> indexes;
    
    static List<Vector2> uvs;
    
    static int index;
    
    static Mesh mesh;
    
    static Material material;
    
    static  Matrix4x4 tr;
    
    static int scale = 1;
    
    static readonly string EncodedTexture= "iVBORw0KGgoAAAANSUhEUgAAAIAAAACAAQMAAAD58POIAAAABlBMVEUAAAD///+l2Z/dAAAAAXRSTlMAQObYZgAAAd1JREFUSMftkCGOG0EQRb8CFhVY2MDaM5SigAFWzlJaRa0FDYxGBQbkADlADpIDNGpUWrjIsowGroxWA1q9qR47JqaB/jOaGT39qf+7gJASY5Ag6HoAtilNJQlzZAfkAC3bkCVsOwj9ThpGSPisqOAOGvEeF8c6KS3R0GcwOK+/cBnRUxz8xFX//BOrbupQVbtZFGVnts1iZtxnGF6ZKC2ZOpAaCAceBqMl6ARkUMAHCzUyNupDA2Nms+63wMyVPaVWfVJl3TF7Ct/U8B51u7S2NX2aT6UMKBpjen6OxhTlfU44WMzx5SU7SNI+ioMpR9VcmVTawUFdHa/Glp7fXguy9RniM/yjlAS1nnL6bqpz6yk3quqXWSnWNOsjEC3anxUUkTCs4MgOkv0SDsmr27Rz8DZokBAKIPu0cdAG/ioU4EB7BX1PHOWhg9NmVq1cijb9orjRJxhHbDDgyHTcOXEgICR/XvYXVJRUxrSjyLYoQN7ETjYyJW5m3ixmq2KRSc/AolmWyf2J3zoY9zrqiWa1WYtdmyyA4qpHrNqE44hVfHkF3gP7cT9N64KqA8fRxtz3key3n2EFUdd9GJ8d0yT0zbSl6ccZCJ2WMHIqVD2lL+jp2uSuu+66667/qL/ccRo2jKF3OQAAAABJRU5ErkJggg==";
    
    static Texture2D Texture;
    
    static Print Instance;
    
    static void PushChar(float x, float y, char c)
    {
        
        int k = c - 32;
        
        float u = (k % 16) * 8;
        float v = (k / 16) * 8;
        
        float uu = u + 8;
        float vv = v + 8;
        
        const float m = 1.0f / 128.0f;
        u *= m;
        v *= m;
        uu *= m;
        vv *= m;
        
        v = 1.0f - v;
        vv = 1.0f - vv;
        
        tP0.x = (x);
        tP0.y = (y);
        tU0.x = u; // sprite.x0;
        tU0.y = v; // sprite.y1;

        tP1.x = (x + w);
        tP1.y = (y);
        tU1.x = uu; //sprite.x1;
        tU1.y = v; //sprite.y1;

        tP2.x = (x + w);
        tP2.y = (y + h);
        tU2.x = uu; //sprite.x1;
        tU2.y = vv; //sprite.y0;

        tP3.x = (x);
        tP3.y = (y + h);
        tU3.x = u; //sprite.x0;
        tU3.y = vv; //sprite.y0;
        
        vertices.Add(tP0);
        vertices.Add(tP1);
        vertices.Add(tP2);
        vertices.Add(tP3);
        
        uvs.Add(tU0);
        uvs.Add(tU1);
        uvs.Add(tU2);
        uvs.Add(tU3);
        
        indexes.Add(index);
        indexes.Add(index + 1);
        indexes.Add(index + 2);
        
        indexes.Add(index);
        indexes.Add(index + 2);
        indexes.Add(index + 3);
        
        index += 4;
    }
    
#endregion

}