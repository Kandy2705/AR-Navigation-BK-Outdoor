using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class SetNavigation : MonoBehaviour
{
    [SerializeField]
    private Camera topDownCamera;

    [SerializeField]
    private GameObject navTargetObject;

    [SerializeField]
    private GameObject markerObject;

    [SerializeField]
    private float startWidth = 0.2f;

    [SerializeField]
    private float endWidth = 0.2f;

    private NavMeshPath path;
    private Mesh mesh;
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;

    void Start()
    {
        
        path = new NavMeshPath();

        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();

        mesh = new Mesh();
        meshFilter.mesh = mesh;

        // dùng Unlit shader để không bị ảnh hưởng ánh sáng và không xoay
        meshRenderer.material = new Material(Shader.Find("Unlit/Color"))
        {
            color = Color.green
        };
    }

    void Update()
    {
        if (!GlobalProperties.Instance.IsShowNavigation)
        {
            mesh.Clear();
            return;
        }

        if (navTargetObject == null || markerObject == null)
            return;

        // Tính toán đường đi NavMesh
        NavMesh.CalculatePath(
            markerObject.transform.position,
            navTargetObject.transform.position,
            NavMesh.AllAreas,
            path
        );

        if (path.corners.Length < 2)
        {
            mesh.Clear();
            return;
        }

        // Tạo mesh đường phẳng từ các điểm path
        BuildPathMesh(path.corners);
    }


    void BuildPathMesh(Vector3[] corners)
    {
        mesh.Clear();

        int n = corners.Length;
        if (n < 2) return;

        // 2 vertex cho mỗi corner: left, right
        Vector3[] verts = new Vector3[n * 2];
        Vector2[] uvs = new Vector2[n * 2];
        List<int> tris = new List<int>((n - 1) * 6);

        // Tạo các cặp left/right cho từng corner (world space)
        for (int i = 0; i < n; i++)
        {
            Vector3 worldPos = corners[i];
            // dính nhẹ lên mặt đất
            worldPos.y -= 0.2f;
        
            // hướng: nếu last corner, lấy hướng từ prev->cur; nếu first, từ cur->next; else trung bình
            Vector3 forward;
            if (i == 0) forward = (corners[1] - corners[0]).normalized;
            else if (i == n - 1) forward = (corners[n - 1] - corners[n - 2]).normalized;
            else
            {
                Vector3 a = (corners[i] - corners[i - 1]).normalized;
                Vector3 b = (corners[i + 1] - corners[i]).normalized;
                forward = (a + b).normalized;
                if (forward == Vector3.zero) forward = b; // fallback
            }

            Vector3 right = Vector3.Cross(Vector3.up, forward).normalized;

            // width interpolation along whole path
            float t = (float)i / (n - 1);
            float width = Mathf.Lerp(startWidth, endWidth, t) * 0.5f;

            Vector3 leftWorld = worldPos - right * width;
            Vector3 rightWorld = worldPos + right * width;

      
            verts[i * 2 + 0] = transform.InverseTransformPoint(leftWorld);
            verts[i * 2 + 1] = transform.InverseTransformPoint(rightWorld);

            // simple UVs along length
            float uvx = t * 1.0f; // you can scale by path length if want
            uvs[i * 2 + 0] = new Vector2(0, uvx);
            uvs[i * 2 + 1] = new Vector2(1, uvx);
        }

        // Build triangles between consecutive corner pairs
        for (int i = 0; i < n - 1; i++)
        {
            int i0 = i * 2;
            int i1 = i0 + 1;
            int i2 = (i + 1) * 2;
            int i3 = i2 + 1;

          
            tris.Add(i0);
            tris.Add(i2);
            tris.Add(i1);

           
            tris.Add(i2);
            tris.Add(i3);
            tris.Add(i1);
        }

        mesh.vertices = verts;
        mesh.triangles = tris.ToArray();
        mesh.uv = uvs;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        mesh.MarkDynamic();
    }

}

