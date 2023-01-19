using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hooking : SpeedrunItem
{
    private PlayerMovement pm;
    private Transform cam;
    public Transform gunTip;
    public LayerMask groundMask;
    public LineRenderer lr; 

    [Header("Grapple settings")] public float maxGrappleDistance;
    public float grappleDelayTime;
    public float overshootYAxis;
    private Vector3 grapplePoint;

    [Header("Cooldown")] public float grapplingCd;
    private float grapplingCdTimer;

    [Header("Input")] public KeyCode grappleKey = KeyCode.Mouse0;
    private bool grappling;
    private void Start()
    {
        pm = FindObjectOfType<PlayerMovement>();
        cam = pm.cameraHolder;
        lr.positionCount = 2;
    }

    private void StartGrapple()
    {
        if (grapplingCdTimer > 0) return;
        grappling = true;
        pm.freeze = true;
        RaycastHit hit;
        if (Physics.Raycast(cam.position, cam.forward, out hit, maxGrappleDistance, groundMask))
        {
            grapplePoint = hit.point;
            Invoke(nameof(ExecuteGrapple), grappleDelayTime);
        }
        else
        {
            grapplePoint = cam.position + cam.forward * maxGrappleDistance;
            Invoke(nameof(StopGrapple), grappleDelayTime);
        }

        lr.enabled = true;
        lr.SetPosition(1, grapplePoint);
    }

    private void ExecuteGrapple()
    {
        pm.freeze = false;
        Vector3 lowestPoint = new Vector3(transform.position.x, 
            transform.position.y - pm.transform.localScale.y, transform.position.z);
        float grapplePointRelativeYPos = grapplePoint.y - lowestPoint.y;
        float highestPointOnArc = grapplePointRelativeYPos + overshootYAxis;
        if (grapplePointRelativeYPos < 0) highestPointOnArc = overshootYAxis;
        pm.JumpToPosition(grapplePoint, highestPointOnArc);
        Invoke(nameof(StopGrapple), 1f);
    }

    public void StopGrapple()
    {
        pm.freeze = false;
        grappling = false;
        grapplingCdTimer = grapplingCd;
        lr.enabled = false;
    }

    public override void UseItem()
    {
        if (PauseMenu.gameIsPaused) return;
        if (Input.GetKeyDown(grappleKey))
        {
            StartGrapple();
        }
        if (grapplingCdTimer > 0) grapplingCdTimer -= Time.deltaTime;
    }

    public override void RemoveTemporaryPlayerComponents()
    {
    }

    public override void LateUseItem()
    {
        if (grappling)
            lr.SetPosition(0, gunTip.position);
    }

    public override void DisableCollider()
    {
        item.GetComponent<BoxCollider>().enabled = false;
    }

    public override void EnableCollider()
    {
        item.GetComponent<BoxCollider>().enabled = true;
    }
}
