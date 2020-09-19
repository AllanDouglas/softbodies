using UnityEngine;

namespace SimpleSoftBody
{
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(Rigidbody))]
    public class SoftBody : MonoBehaviour
    {
        [SerializeField] private float bounce = 50;
        [SerializeField] private float stiffness = 10;
        [SerializeField] private float minFallForce = 5;
        [SerializeField] private float maxFallForce = 25;

        MeshFilter meshFilter;
        Mesh mesh;
        SoftVertext[] softVertexts;
        Vector3[] meshVertices;
        private void Awake()
        {
            meshFilter = GetComponent<MeshFilter>();
            mesh = meshFilter.mesh;

            LoadVertices();
        }

        private void LoadVertices()
        {
            softVertexts = new SoftVertext[mesh.vertices.Length];
            meshVertices = new Vector3[mesh.vertices.Length];
            for (int i = 0; i < softVertexts.Length; i++)
            {
                softVertexts[i] = new SoftVertext(i, mesh.vertices[i], mesh.vertices[i], Vector3.zero, .001f);
                meshVertices[i] = mesh.vertices[i];
            }
        }

        private void Update()
        {
            
            var deltaTime = Time.deltaTime;
            var shouldUpdate = false;
            for (int i = 0; i < softVertexts.Length; i++)
            {
                var softVertext = softVertexts[i];

                if (shouldUpdate == false)
                {
                    shouldUpdate = softVertext.GetDisplacement().sqrMagnitude > 0;
                }

                softVertext.UpdateVelovity(in bounce, in deltaTime, in stiffness);
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
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();
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
                    force: other.relativeVelocity.sqrMagnitude);
            }
        }

        private void ApplyPressure(float deltaTime, Vector3 contactPoint, Vector3 direction, float force)
        {
            var localPosition = transform.InverseTransformPoint(contactPoint);
            var dir = direction * -1;
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