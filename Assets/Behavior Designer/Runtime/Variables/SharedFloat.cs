using UnityEngine;
using System.Collections;

namespace BehaviorDesigner.Runtime
{
    [System.Serializable]
    public class SharedFloat : SharedVariable
    {
        public float Value { get { return mValue; } set { mValue = value; } }
        [SerializeField]
        private float mValue;

        public SharedFloat() { mValueType = SharedVariableTypes.Float; }

        public override object GetValue() { return mValue; }
        public override void SetValue(object value) { mValue = (float)value; }
    }
}