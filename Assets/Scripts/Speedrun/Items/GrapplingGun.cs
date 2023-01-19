using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrapplingGun : SpeedrunItem
{
    private PlayerMovement pm;
    private LineRenderer lr;
    private Vector3 grapplePoint;
    public LayerMask grappleMask;
    public float maxDistance = 50f;
    public Transform gunTip;
    private Transform camera;
    private SpringJoint joint;
    private void Awake()
    {
        lr = GetComponentInChildren<LineRenderer>();
        pm = FindObjectOfType<PlayerMovement>();
    }

    private void Start()
    {
        lr.positionCount = 0;
        camera = pm.cameraHolder;
    }

    public override void UseItem()
    {
        if (Input.GetMouseButtonDown(0))
        {
            StartGrapple();
        }
        else if (Input.GetMouseButtonUp(0))
        {
            StopGrapple();
        }
    }

    public override void RemoveTemporaryPlayerComponents()
    {
        StopGrapple();
        lr.positionCount = 0;
    }

    private void DrawRope()
    {
        lr.positionCount = 2;
        lr.SetPosition(0, gunTip.position);
        lr.SetPosition(1, grapplePoint);
    }

    private void StartGrapple()
    {
        RaycastHit hit;
        if (Physics.Raycast(camera.position, camera.forward, out hit, maxDistance, grappleMask))
        {
            grapplePoint = hit.point;
            joint = pm.gameObject.AddComponent<SpringJoint>();
            joint.autoConfigureConnectedAnchor = false;
            joint.connectedAnchor = grapplePoint;
            float distanceFromPoint = Vector3.Distance(pm.transform.position, grapplePoint);
            joint.maxDistance = distanceFromPoint * .8f;
            joint.minDistance = distanceFromPoint * .25f;

            joint.spring = 4.5f;
            joint.damper = 7f;
            joint.massScale = 4.5f;
        }
    }

    private void StopGrapple()
    {
        if (pm.TryGetComponent(out joint))
            Destroy(joint);
    }

    public override void DisableCollider()
    {
        Debug.Log("Collider disabled for grappling gun!");
        item.GetComponent<BoxCollider>().enabled = false;
    }

    public override void EnableCollider()
    {
        Debug.Log("Added box collider to grappling gun!");
        item.GetComponent<BoxCollider>().enabled = true;
    }

    public override void LateUseItem()
    {
        if (joint) DrawRope();
        else lr.positionCount = 0;
    }
}
