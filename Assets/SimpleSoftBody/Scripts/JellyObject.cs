using UnityEngine;

namespace SimpleSoftBody
{
    [RequireComponent(typeof(MeshFilter))]
    public class JellyObject : MonoBehaviour
    {
        [SerializeField] public float dumping = 1;
        [SerializeField] public float stiffness = 1;
        [SerializeField] public float mass = 1;
        [SerializeField] public float intensity = 1;

        MeshFilter meshFilter;
        Mesh mesh;
        private Camera mainCamera;
        private int layerMask;
        SoftVertex[] softVertexes;
        Vector3[] meshVertices;

        JellyVertex[] jellyVertices;
        Vector3[] positions;
        new MeshRenderer renderer;
        RaycastHit hit;

        private void Awake()
        {
            meshFilter = GetComponent<MeshFilter>();
            renderer = GetComponent<MeshRenderer>();
            CreateJellyVertexArray();
            positions = meshFilter.mesh.vertices;

            mesh = meshFilter.mesh;
            mainCamera = Camera.main;
            layerMask = 1 << gameObject.layer;
            LoadVertices();
        }

        private void CreateJellyVertexArray()
        {
            jellyVertices = new JellyVertex[meshFilter.mesh.vertexCount];

            for (int i = 0; i < jellyVertices.Length; i++)
            {
                jellyVertices[i] = new JellyVertex(transform.TransformPoint(meshFilter.mesh.vertices[i]));
            }
        }

        private void LoadVertices()
        {
            softVertexes = new SoftVertex[mesh.vertices.Length];
            meshVertices = new Vector3[mesh.vertices.Length];
            for (int i = 0; i < softVertexes.Length; i++)
            {
                softVertexes[i] = new SoftVertex(i, mesh.vertices[i], mesh.vertices[i], Vector3.zero, .001f);
                meshVertices[i] = mesh.vertices[i];
            }
        }

        private void FixedUpdate()
        {
            for (int i = 0; i < jellyVertices.Length; i++)
            {
                var restPosition = jellyVertices[i].RestPosition;
                var intensity = (1 - (renderer.bounds.max.y - restPosition.y) / renderer.bounds.size.y) * this.intensity;
                var verticePosition = jellyVertices[i].Shake(
                                    mass: mass,
                                    stiffness: stiffness,
                                    damping: dumping,
                                    intensity: intensity);
                                    
                positions[i] = transform.InverseTransformPoint(verticePosition);
            }

            UpdateMesh(positions);
        }

        private void UpdateMesh(Vector3[] positions)
        {
            mesh.vertices = positions;
            mesh.RecalculateNormals();
        }
    }
}