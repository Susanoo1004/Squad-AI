using UnityEngine;

namespace Squad
{
    public class ISquadFormation : MonoBehaviour
    {
        protected uint totalUnits;

        virtual public void UpdateUnitCount()
        {
            totalUnits = transform.GetComponent<SquadController>().NumberOfUnits;
        }

        void Start()
        {
            UpdateUnitCount();
        }
        void OnValidate()
        {
            UpdateUnitCount();
        }
        virtual public Vector3 GetOffset(uint index) { return Vector3.zero; }
        public Vector3 GetOffset(uint index, float cosAngle, float sinAngle)
        {
            Vector3 vecBase = GetOffset(index);
            return new Vector3(vecBase.x * cosAngle - vecBase.z * sinAngle, vecBase.y, vecBase.z * cosAngle + vecBase.x * sinAngle);
        }

    }
}
