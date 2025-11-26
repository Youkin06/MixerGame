using UnityEngine;

public class CutterLine : MonoBehaviour
{
    #region Variables
    
    [Header("Triangle Settings")]
    [SerializeField] private Transform _endPoint;
    [SerializeField] private float _triangleWidth = 0.1f;
    
    private MeshFilter _meshFilter;
    private MeshRenderer _meshRenderer;
    private Mesh _triangleMesh;
    private CutterController _cutterController;
    private Material _lineMaterial;
    private Color _originalLineColor;
    
    #endregion

    #region Unity Methods

    void Start()
    {
        _meshFilter = GetComponent<MeshFilter>();
        _meshRenderer = GetComponent<MeshRenderer>();
        
        if (_meshFilter == null)
        {
            _meshFilter = gameObject.AddComponent<MeshFilter>();
        }
        
        if (_meshRenderer == null)
        {
            _meshRenderer = gameObject.AddComponent<MeshRenderer>();
        }
        
        _triangleMesh = new Mesh();
        _triangleMesh.name = "TriangleMesh";
        _meshFilter.mesh = _triangleMesh;
        
        _cutterController = GetComponentInParent<CutterController>();
        
        if (_meshRenderer.material != null)
        {
            _lineMaterial = _meshRenderer.material;
            _originalLineColor = _lineMaterial.color;
        }
    }

    void Update()
    {
        if (_endPoint != null)
        {
            UpdateTriangleMesh();
        }
        
        UpdateTransparency();
    }
    
    #endregion

    #region Private Methods

    void UpdateTriangleMesh()
    {
        Vector3 startPos = transform.position;
        Vector3 endPos = _endPoint.position;
        Vector3 direction = (endPos - startPos).normalized;
        float distance = Vector3.Distance(startPos, endPos);
        
        Vector3 perpendicular = Vector3.Cross(direction, Vector3.forward).normalized;
        if (perpendicular.sqrMagnitude < 0.01f)
        {
            perpendicular = Vector3.Cross(direction, Vector3.up).normalized;
        }
        
        Vector3 halfWidth = perpendicular * (_triangleWidth * 0.5f);
        
        Vector3 baseLeft = startPos + halfWidth;
        Vector3 baseRight = startPos - halfWidth;
        Vector3 tip = endPos;
        
        Vector3[] vertices = new Vector3[3]
        {
            transform.InverseTransformPoint(baseLeft),
            transform.InverseTransformPoint(baseRight),
            transform.InverseTransformPoint(tip)
        };
        
        int[] triangles = new int[3] { 0, 1, 2 };
        
        Vector2[] uv = new Vector2[3]
        {
            new Vector2(0, 0),
            new Vector2(1, 0),
            new Vector2(0.5f, 1)
        };
        
        _triangleMesh.Clear();
        _triangleMesh.vertices = vertices;
        _triangleMesh.triangles = triangles;
        _triangleMesh.uv = uv;
        _triangleMesh.RecalculateNormals();
    }
    
    void UpdateTransparency()
    {
        if (_cutterController == null || _lineMaterial == null)
        {
            return;
        }
        
        Renderer cutterRenderer = _cutterController.GetComponent<Renderer>();
        if (cutterRenderer != null && cutterRenderer.material != null)
        {
            Color cutterColor = cutterRenderer.material.color;
            Color lineColor = _originalLineColor;
            lineColor.a = cutterColor.a;
            _lineMaterial.color = lineColor;
        }
    }
    
    #endregion
}
