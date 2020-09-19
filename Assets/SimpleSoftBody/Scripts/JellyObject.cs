using UnityEngine;
using UnityEngine.Networking.Match;

namespace SimpleSoftBody
{
    [RequireComponent(typeof(MeshFilter))]
    public class JellyObject : MonoBehaviour
    {
        [SerializeField] private float bounce = 50;
        [SerializeField] private float stiffness = 10;
        [SerializeField] private float fallForce = 25;

        MeshFilter meshFilter;
        Mesh mesh;
        private Camera mainCamera;
        private int layerMask;
        SoftVertext[] softVertexts;
        Vector3[] meshVertices;

        RaycastHit hit;

        private void Awake()
        {
            meshFilter = GetComponent<MeshFilter>();
            mesh = meshFilter.mesh;
            mainCamera = Camera.main;
            layerMask = 1 << gameObject.layer;
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

            if (Input.GetMouseButtonDown(0))
            {
                TryApplyPressure(deltaTime);
            }

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

        private void TryApplyPressure(float deltaTime)
        {
            var ray = mainCamera.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray.origin, ray.direction, out hit, Mathf.Infinity, layerMask))
            {
                if (hit.collider.gameObject == gameObject)
                    ApplyPressure(deltaTime, hit.point, fallForce);
            }
        }

        private void ApplyPressure(float deltaTime, Vector3 contactPoint, float force)
        {
            var localPosition = transform.InverseTransformPoint(contactPoint);

            foreach (var vertex in softVertexts)
            {
                vertex.ApplyPressure(
                    localPosition: in localPosition,
                    pressure: in force,
                    deltaTime: in deltaTime);
            }
        }


    }
}