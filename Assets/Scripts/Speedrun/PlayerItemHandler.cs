using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerItemHandler : MonoBehaviour
{
    [Header("References")]
    private PlayerMovement pm;
    public Transform orientation, itemHolder;
    private Transform playerCam;
    public LayerMask itemMask;
    private RaycastHit hit;
    [Header("Item handling")] public List<SpeedrunItem> items = new List<SpeedrunItem>();
    public int itemIndex;
    [Header("Keybinds")] public KeyCode pickKey = KeyCode.E;
    public KeyCode dropKey = KeyCode.Q;
    private void Awake()
    {
        pm = GetComponent<PlayerMovement>();
    }

    private void Start()
    {
        playerCam = pm.cameraHolder;
    }

    private void PickItem()
    {
        if (Input.GetKeyDown(pickKey) && items.Count == 0)
        {
            if (Physics.Raycast(playerCam.position, playerCam.forward, out hit, 20f, itemMask))
            {
                SpeedrunItem item = Instantiate(hit.collider.GetComponent<SpeedrunItem>(),
                    orientation.transform);
                item.LerpRotation(item.gameObject, itemHolder);
                item.nameClonedItem();
                if (item.TryGetComponent(out Rigidbody rb)) Destroy(rb);
                item.DisableCollider();
                item.transform.position = itemHolder.transform.position;
                items.Add(item);
                Destroy(hit.collider.gameObject);
            }
        }
    }

    private void DropItem()
    {
        if (Input.GetKeyDown(dropKey) && items.Count == 1)
        {
            GameObject oldItem = Instantiate(items[0].item, orientation.position, Quaternion.identity);
            oldItem.GetComponent<SpeedrunItem>().nameClonedItem();
            oldItem.GetComponent<SpeedrunItem>().EnableCollider();
            oldItem.GetComponent<SpeedrunItem>().RemoveTemporaryPlayerComponents();
            Rigidbody itemRB = oldItem.AddComponent<Rigidbody>();
            itemRB.drag = 5f;
            itemRB.AddForce(pm.transform.up * 3f, ForceMode.Force);
            Destroy(items[0].gameObject);
            items.Remove(items[0]);
        }
    }

    private void Update()
    {
        if (PauseMenu.gameIsPaused) return;
        PickItem();
        DropItem();
        if (items.Count > 0) items[0].UseItem();
    }
    // Late update: 1 frame after update: for grappling gun
    private void LateUpdate()
    {
        if (PauseMenu.gameIsPaused) return;
        if (items.Count > 0) items[0].LateUseItem();
    }
}
