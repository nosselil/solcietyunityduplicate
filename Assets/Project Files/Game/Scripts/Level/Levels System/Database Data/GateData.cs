using UnityEngine;

namespace Watermelon
{
    [System.Serializable]
    public class GateData : AbstractData
    {
        // Tells the code how to handle the gate. does not replace id
        [SerializeField] GateType gateType;
        public GateType GateType => gateType;

        // if true - user can set value and enable updateOnHit in the Level Editor. if false - the user have to give an id of the thing it wants to get ("Character", "Weapon", etc)
        [SerializeField] bool isNumerical;
        public bool IsNumerical => isNumerical;

        [SerializeField] Texture2D previewImage;
        public Texture2D PreviewImage => previewImage;
    }
}
