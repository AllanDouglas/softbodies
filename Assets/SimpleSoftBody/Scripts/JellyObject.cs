using UnityEngine;

namespace SimpleSoftBody
{
    [RequireComponent(typeof(MeshFilter))]
    public class JellyObject : MonoBehaviour
    {
        [SerializeField] public float dumping = .75f;
        [SerializeField] public float stiffness = 1;
        [SerializeField] public float mass = 1;
        [SerializeField] public float intensity = 1;

        MeshFilter meshFilter;
        Mesh mesh;
        
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
            meshVertices = meshFilter.mesh.vertices;
            positions = new Vector3[meshVertices.Length];

            mesh = meshFilter.mesh;
        }

        private void CreateJellyVertexArray()
        {
            jellyVertices = new JellyVertex[meshFilter.mesh.vertexCount];

            for (int i = 0; i < jellyVertices.Length; i++)
            {
                jellyVertices[i] = new JellyVertex(transform.TransformPoint(meshFilter.mesh.vertices[i]));
            }
        }
        private void FixedUpdate()
        {
            for (int i = 0; i < jellyVertices.Length; i++)
            {
                var jellyVertex = jellyVertices[i];
                var restPosition = transform.TransformPoint(meshVertices[i]);
                var intensity = (1 - (renderer.bounds.max.y - restPosition.y) / renderer.bounds.size.y) * this.intensity;

                var verticePosition = transform.InverseTransformPoint(jellyVertex.Shake(
                                    restPosition: restPosition,
                                    mass: mass,
                                    stiffness: stiffness,
                                    damping: dumping));

                positions[i] = Vector3.Lerp(meshVertices[i], verticePosition, intensity);
            }

            UpdateMesh(positions);
        }

        private void UpdateMesh(Vector3[] positions)
        {
            mesh.vertices = positions;
            mesh.RecalculateNormals();
            // mesh.reva();
        }
    }
}