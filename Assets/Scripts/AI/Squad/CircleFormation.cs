using UnityEngine;

namespace Squad
{
    public class CircleFormation : ISquadFormation
    {

        [SerializeField]
        private float minRadius = 1f;
        [SerializeField]
        private float unitSize = 2f;
        override public Vector3 GetOffset(uint index)
        {
            if (totalUnits == 1)
                return Vector3.zero;

            float angle = 2 * Mathf.PI * index / totalUnits;

            float radius = minRadius;
            if (totalUnits * unitSize > minRadius * Mathf.PI * 2)
                radius = totalUnits * unitSize / 2f / Mathf.PI;
            return new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);

        }
    }
}