using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaticCannon : MonoBehaviour
{
    private PlayerMovement pm;
    public Transform turret, turretTip;
    public Rigidbody bullet;
    public float distance = 50f;
    private bool alreadyAttacked;
    public float cooldown = 1f;
    private void Start()
    {
        pm = FindObjectOfType<PlayerMovement>();
    }

    private void Update()
    {
        if (Vector3.Distance(turret.position, pm.cameraPos.position) < distance)
        {
            if (!alreadyAttacked)
            {
                turret.LookAt(pm.cameraPos);
                Rigidbody newBullet = Instantiate(bullet, turretTip.position, Quaternion.identity);
                newBullet.rotation = turretTip.rotation;
                newBullet.gameObject.SetActive(true);
                Vector3 direction = pm.cameraPos.position - turretTip.position;
                newBullet.transform.forward = direction.normalized;
                newBullet.GetComponent<CannonBullet>().Shoot(direction);
                alreadyAttacked = true;
                Invoke(nameof(ResetAttack), cooldown);
            }
        }
    }

    private void ResetAttack()
    {
        alreadyAttacked = false;
    }
}
