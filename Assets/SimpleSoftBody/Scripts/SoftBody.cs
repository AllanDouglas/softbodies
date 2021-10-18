using UnityEngine;

namespace SimpleSoftBody
{
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(Rigidbody))]
    public class SoftBody : MonoBehaviour
    {
        [SerializeField] public float bounce = 50;
        [SerializeField] public float stiffness = 10;
        [SerializeField] public float minFallForce = 5;
        [SerializeField] public float maxFallForce = 25;
        [SerializeField] public float displacementTolerance = .0001f;
        [SerializeField] public float pressureMultiplier = 1;

        MeshFilter meshFilter;
        Mesh mesh;
        SoftVertex[] softVertexts;
        Vector3[] meshVertices;
        private void Awake()
        {
            meshFilter = GetComponent<MeshFilter>();
            mesh = meshFilter.mesh;

            LoadVertices();
        }

        private void LoadVertices()
        {
            softVertexts = new SoftVertex[mesh.vertices.Length];
            meshVertices = new Vector3[mesh.vertices.Length];
            for (int i = 0; i < softVertexts.Length; i++)
            {
                softVertexts[i] = new SoftVertex(i, mesh.vertices[i], mesh.vertices[i], Vector3.zero, displacementTolerance);
                meshVertices[i] = mesh.vertices[i];
            }
        }

        private void Update()
        {

            if (transform.hasChanged == false) return;
            transform.hasChanged = false;

            var deltaTime = Time.deltaTime;
            var shouldUpdate = false;
            for (int i = 0; i < softVertexts.Length; i++)
            {
                var softVertext = softVertexts[i];

                if (shouldUpdate == false)
                {
                    shouldUpdate = softVertext.GetDisplacement().sqrMagnitude > 0;
                }

                softVertext.UpdateVelocity(in bounce, in deltaTime, in stiffness);
                softVertext.UpdateVertex(in deltaTime);
                meshVertices[softVertext.vertexIndex] = softVertext.CurrentVertexPosition;
            }

            if (!shouldUpdate)
                return;

            UpdateMesh();

        }

        private void UpdateMesh()
        {
            mesh.vertices = meshVertices;
            mesh.RecalculateNormals();
        }

        private void OnCollisionEnter(Collision other)
        {
            var contacts = other.contacts;
            var deltaTime = Time.deltaTime;

            foreach (var contact in contacts)
            {
                ApplyPressure(
                    deltaTime: deltaTime,
                    contactPoint: contact.point,
                    direction: contact.normal,
                    force: other.relativeVelocity.sqrMagnitude * other.relativeVelocity.sqrMagnitude * pressureMultiplier);
            }
        }

        private void ApplyPressure(float deltaTime, Vector3 contactPoint, Vector3 direction, float force)
        {
            var localPosition = transform.InverseTransformPoint(contactPoint);
            var dir = transform.InverseTransformDirection(direction * -1);
            var pressure = Mathf.Clamp(force, minFallForce, maxFallForce);

            foreach (var vertex in softVertexts)
            {
                vertex.ApplyPressure(
                    localPosition: in localPosition,
                    direction: in dir,
                    pressure: in pressure,
                    deltaTime: in deltaTime);
            }
        }


    }
}