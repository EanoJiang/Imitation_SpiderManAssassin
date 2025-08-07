using System;
using System.Collections;
using System.Collections.Generic;
using Invector.vItemManager;
using UnityEngine;
using UnityEngine.Events;

public class vEquipItemTrigger : MonoBehaviour
{
    public int itemID;
    public UnityEvent OnEquipSuccess, OnEquipFail;
    public vItemManager itemManager;

    public void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            var itemManager = other.GetComponent<vItemManager>();
            if (itemManager)
            {
                EquipItemIfExist(itemManager);
            }
        }
    }

    private void EquipItemIfExist(vItemManager itemManager)
    {
        if (itemManager.items.Exists(i => i.id == itemID))
        {
            var item = itemManager.items.Find(i => i.id == itemID);
            var indexOfArea = System.Array.FindIndex(itemManager.inventory.equipAreas, area => area.equipSlots.Exists(slot => slot.itemType.Contains(item.type)));

            itemManager.EquipItemToCurrentEquipSlot(item, indexOfArea);
            OnEquipSuccess.Invoke();
        }
        else
        {
            OnEquipFail.Invoke();
        }
    }
}
