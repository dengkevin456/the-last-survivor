using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SpeedrunItem : MonoBehaviour
{
    public GameObject item;
    public string itemName;
    public abstract void UseItem();

    public abstract void RemoveTemporaryPlayerComponents();
    // Using item at late update
    public abstract void LateUseItem();

    public void LerpRotation(GameObject original, Transform final)
    {
        original.transform.rotation = Quaternion.Lerp(original.transform.rotation, final.rotation, 1.5f);
    }
    public void nameClonedItem()
    {
        item.name = itemName;
    }
    public abstract void DisableCollider();
    public abstract void EnableCollider();
}
