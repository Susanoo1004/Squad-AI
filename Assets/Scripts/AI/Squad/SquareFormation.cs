using UnityEngine;

namespace Squad
{
    public class SquareFormation : ISquadFormation
    {

        [SerializeField]
        uint UnitPerLine = 3;
        [SerializeField]
        private float Spacing = 1f;
        private uint Lines;


        private float offset;
        override public void UpdateUnitCount()
        {
            totalUnits = transform.GetComponent<SquadController>().NumberOfUnits;
            Lines = totalUnits / UnitPerLine;
            if ((totalUnits % UnitPerLine) != 0)
                Lines++;
        }
        // Start is called before the first frame update
        void Start()
        {
            UpdateUnitCount();
        }
        void OnValidate()
        {
            UpdateUnitCount();
        }
        override public Vector3 GetOffset(uint index)
        {
            uint positionInLine = index % UnitPerLine;
            float Xoffset = ((UnitPerLine - 1) * -0.5f + positionInLine) * Spacing;
            uint line = index / UnitPerLine;
            float Yoffset = ((Lines - 1) * -0.5f + line) * Spacing;

            return new Vector3(Xoffset, 0f, Yoffset);

        }
    }
}
