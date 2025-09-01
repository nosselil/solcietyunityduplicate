using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    using Upgrades;

    [CreateAssetMenu(menuName = "Data/Upgrades/Float Upgrade", fileName = "Float Upgrade")]
    public class FloatUpgrade : Upgrade<FloatUpgrade.Stage>
    {
        public override void Initialise()
        {

        }

        [System.Serializable]
        public class Stage : BaseUpgradeStage
        {
            public float value;
            public float Value => value;
        }

        [Button]
        public void UpdateValuesDev()
        {
            for (int i = 0; i < upgrades.Length; i++)
            {
                upgrades[i].value = upgrades[i].Value * 2;

                RuntimeEditorUtils.SetDirty(this);
            }
        }
    }
}