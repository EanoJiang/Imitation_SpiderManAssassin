
namespace MalbersAnimations.Scriptables
{
    [System.Serializable]
    public class BoolReference
    {
        public bool UseConstant = true;

        public bool ConstantValue;
        public BoolVar Variable;

        public BoolReference()
        {
            UseConstant = true;
            ConstantValue = false;
        }

        public BoolReference(bool value)
        {
            Value = value;
        }

        public bool Value
        {
            get { return UseConstant || Variable == null ? ConstantValue : Variable.Value; }
            set
            {
                if (UseConstant || Variable == null)
                    ConstantValue = value;
                else
                {
                    Variable.Value = value;
                }
            }
        }
        #region Operators
        public static implicit operator bool(BoolReference reference)
        {
            return reference.Value;
        }
        #endregion
    }
}