using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StairClimbing : MonoBehaviour
{
    Rigidbody rigidBody;
    [SerializeField] GameObject stepRayUpper;
    [SerializeField] GameObject stepRayLower;
    [SerializeField] float stepHeight = 0.3f;
    [SerializeField] float stepSmooth = 2f;

    private void Awake()
    {
        rigidBody = GetComponent<Rigidbody>();

        // stepRayUpper.transform.position = new Vector3(stepRayUpper.transform.position.x, stepHeight, stepRayUpper.transform.position.z);
    }
    
    private void FixedUpdate()
    {
        stepClimb();
    }

    private void stepClimb()
    {
        RaycastHit hitLower;
        if (Physics.Raycast(stepRayLower.transform.position, stepRayLower.transform.forward, out hitLower, 0.4f, LayerMask.GetMask("Ground")))
        {
            RaycastHit hitUpper;
            if (!Physics.Raycast(stepRayUpper.transform.position, stepRayUpper.transform.forward, out hitUpper, 0.5f, LayerMask.GetMask("Ground")))
            {
                Debug.Log($"Detecting! {Time.frameCount}");
                rigidBody.position += Vector3.up * stepSmooth * Time.deltaTime;
                // rigidBody.position -= new Vector3(0f, -stepSmooth * Time.deltaTime, 0f);
            }
        }
        
        
    }
}
