// Copyright © 2017-2024 Vault Break Studios Pty Ltd

using UnityEngine;

namespace MxM
{
    //============================================================================================
    /**
    *  @brief A data structure representing a contact point in an MxM event. This stores both it's 
    *  position and y-axis rotation only.
    *         
    *********************************************************************************************/
    [System.Serializable]
    public struct EventContact
    {
        public Vector3 Position;
        public float RotationY;

        //============================================================================================
        /**
        *  @brief Constructor for the EventContact Struct
        *         
        *********************************************************************************************/
        public EventContact(Vector3 a_position, float a_rotationY)
        {
            Position = a_position;
            RotationY = a_rotationY;
        }

    }//End of class: EventContact
}//End of namespace: MxM
