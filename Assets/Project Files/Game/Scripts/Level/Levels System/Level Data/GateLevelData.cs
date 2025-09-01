using UnityEngine;

namespace Watermelon
{
    [System.Serializable]
    public class GateLevelData : AbstractLevelData
    {
        [SerializeField] GateType gateType;
        public GateType GateType => gateType;

        // Data for isNumerical == true
        [SerializeField] OperationType operationType;
        public OperationType OperationType => operationType;

        // Data for isNumerical == true
        [SerializeField] float numericalValue;
        public float NumericalValue => numericalValue;

        // Data for isNumerical == true
        [SerializeField] bool updateOnHit;
        public bool UpdateOnHit => updateOnHit;

        // Data for isNumerical == true temp
        public float step;
        public float Step => step;

        // Id of the character or a gun
        [SerializeField] string explicitId;
        public string ExplicitId => explicitId;

        // Only for when the gateType == Character
        [SerializeField] int charactersAmount;
        public int CharactersAmount => charactersAmount;
    }
}
