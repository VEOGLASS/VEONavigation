using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XploriaAR
{  
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(MeshFilter))]

    [DisallowMultipleComponent]
    public class GridRenderer : MonoBehaviour
    {
        #region Private fields

        private MeshRenderer MeshRenderer;
        private MeshFilter MeshFilter;
        private Mesh Mesh;

        private Material GridMaterial;

        #endregion

        #region Inspector fields

        [Tooltip("Squared size.")]
        public int gridSize = 300;
        [Tooltip("Size of single cell.")]
        public int cellSize = 1;

        [Space]

        [SerializeField]
        public Color32 gridColor = new Color32(128, 128, 128, 100);

        #endregion

        #region Init methods

        private void Awake()
        {
            #region Self-Injection

            MeshRenderer = GetComponent<MeshRenderer>();
            MeshFilter = GetComponent<MeshFilter>();

            #endregion
        }
        private void Start()
        {
            #region Basic init

            DrawGrid();

            #endregion
        }

        private void OnValidate()
        {
            gridSize = Mathf.Max(1, gridSize);
            cellSize = Mathf.Max(1, cellSize);
        }

        #endregion

        #region Private methods

        private void DrawGrid()
        {
            var i = 0;
            //line step calculation
            var step = (1 / ((float) gridSize / (float) cellSize));

            //materials and slef-injection
            GridMaterial = new Material(Shader.Find("Sprites/Default"));
            Mesh = new Mesh();

            var verticies = new List<Vector3>();
            var indices = new List<int>();

            //OX drawing
            for (float x = -0.5f; x <= 0.5f; x += step)
            {
                verticies.Add(new Vector3(x, 0.5f, -0.5f));
                verticies.Add(new Vector3(x, 0.5f, 0.5f));
                indices.Add(i);
                indices.Add(i + 1);
                i += 2;
            }

            //OY drawing
            for (float y = -0.5f; y <= 0.5f; y += step)
            {
                verticies.Add(new Vector3(-0.5f, 0.5f, y));
                verticies.Add(new Vector3(0.5f, 0.5f, y));
                indices.Add(i);
                indices.Add(i + 1);
                i += 2;
            }

            //setting new topology
            Mesh.vertices = verticies.ToArray();
            Mesh.SetIndices(indices.ToArray(), MeshTopology.Lines, 0);
            MeshFilter.mesh = Mesh;

            MeshRenderer.material = GridMaterial;
            MeshRenderer.material.color = gridColor;
        }

        #endregion

        #region Public methods

        public void SetGrid(int cellSize, int gridSize)
        {
            this.cellSize = cellSize;
            this.gridSize = gridSize;

            DrawGrid();
        }

        #endregion
    }
}