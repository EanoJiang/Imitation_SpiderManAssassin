// Copyright © 2017-2024 Vault Break Studios Pty Ltd

namespace MxMEditor
{
    [System.Serializable]
    public class MotionPreset
    {
        public string MotionName;
        public float MotionTiming;
        public EMotionModType MotionType;
        
        public MotionPreset(string a_variableName, float a_motionTiming, EMotionModType a_motionType)
        {
            MotionName = a_variableName;
            MotionTiming = a_motionTiming;
            MotionType = a_motionType;
        }
    }//End of class: MotionPreset
}//End of namespace: MxM