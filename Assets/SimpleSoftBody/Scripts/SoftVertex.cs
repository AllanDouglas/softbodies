using UnityEngine;

namespace SimpleSoftBody
{
    public class SoftVertex
    {
        public readonly int vertexIndex;
        private Vector3 initialVertexPosition;
        private Vector3 currentVertexPosition;
        private Vector3 currentVelocity;
        private readonly float displacementTolerance;

        public SoftVertex(
            int vertexIndex,
            Vector3 initialVertexPosition,
            Vector3 currentVertexPosition,
            Vector3 currentVelocity,
            float displacementTolerance = 0)
        {
            this.vertexIndex = vertexIndex;
            this.initialVertexPosition = initialVertexPosition;
            this.currentVertexPosition = currentVertexPosition;
            this.currentVelocity = currentVelocity;
            this.displacementTolerance = displacementTolerance;
        }

        public Vector3 CurrentVelocity => currentVelocity;
        public Vector3 CurrentVertexPosition => currentVertexPosition;

        public Vector3 GetDisplacement() =>
            currentVertexPosition - initialVertexPosition;

        public void UpdateVelocity(in float bounceSpeed, in float deltaTime, in float stiffness)
        {
            var displacement = GetDisplacement();

            if (VelocityLow() && DisplacementLow())
            {
                currentVertexPosition = initialVertexPosition;
                currentVelocity = Vector3.zero;
                return;
            }

            currentVelocity -= displacement * bounceSpeed * deltaTime;
            currentVelocity *= 1f - stiffness * deltaTime;

            bool DisplacementLow()
            {
                return displacement.sqrMagnitude < displacementTolerance &&
                    displacement.sqrMagnitude > -displacementTolerance;
            }

            bool VelocityLow()
            {
                return currentVelocity.sqrMagnitude < displacementTolerance &&
                    currentVelocity.sqrMagnitude > -displacementTolerance;
            }
        }

        public void UpdateVertex(in float deltaTime)
        {
            currentVertexPosition += currentVelocity * deltaTime;
        }

        public void ApplyPressure(
            in Vector3 localPosition,
            in float pressure,
            in float deltaTime)
        {
            var displacement = currentVertexPosition - localPosition;
            ApplyPressureTo(displacement, displacement.normalized, pressure, in deltaTime);
        }

        public void ApplyPressure(
            in Vector3 localPosition,
            in Vector3 direction,
            in float pressure,
            in float deltaTime)
        {
            ApplyPressureTo(currentVertexPosition - localPosition, direction, pressure, deltaTime);
        }

        private void ApplyPressureTo(
            in Vector3 displacement,
            in Vector3 direction,
            in float pressure,
            in float deltaTime)
        {
            currentVelocity += direction * (pressure * displacement.sqrMagnitude * deltaTime);
        }

    }
}