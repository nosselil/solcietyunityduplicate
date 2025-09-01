using UnityEngine;

namespace Watermelon
{
    [System.Serializable]
    public class BoosterLevelData : AbstractLevelData
    {
        [SerializeField] BoosterType boosterType;
        public BoosterType BoosterType => boosterType;

        // Data for isNumerical == true
        [SerializeField] OperationType operationType;
        public OperationType OperationType => operationType;

        [SerializeField] float numericalValue;
        public float NumericalValue => numericalValue;

        // Id of the character or a gun
        [SerializeField] string explicitId;
        public string ExplicitId => explicitId;

        // Only for when the gateType == Character
        [SerializeField] int charactersAmount;
        public int CharactersAmount => charactersAmount;
    }
}
