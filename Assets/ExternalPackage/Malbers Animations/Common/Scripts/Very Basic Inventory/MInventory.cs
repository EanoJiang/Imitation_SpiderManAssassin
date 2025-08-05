using UnityEngine;
using MalbersAnimations.Events;

namespace MalbersAnimations
{
    public class MInventory : MonoBehaviour
    {
        public GameObject[] Inventory;
        public GameObjectEvent OnEquipItem; 

        public virtual void EquipItem(int Slot)
        {
            if (Slot < Inventory.Length)
            {
                OnEquipItem.Invoke(Inventory[Slot]);
            }
        }
    }
}
