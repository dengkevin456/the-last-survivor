using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrenadeGun : SpeedrunItem
{
    private PlayerMovement pm;
    private Camera playerCam;
    public GameObject bullet;

    [Header("Bullet force")] public float shootForce, upwardForce;

    [Header("Gun stats")] public float timeBetweenShooting;
    public float spread;
    public float reloadTime;
    public float timeBetweenShots;
    public int magazineSize, bulletsPerTap;
    public bool allowButtonHold;
    private int bulletsLeft, bulletsShot;

    private bool shooting, readyToShoot, reloading;

    // Player gameobjects
    private Transform cameraHolder;
    private Transform attackPoint;


    [Header("Bug fixing")] public bool allowInvoke = true;
    private void Awake()
    {
        pm = FindObjectOfType<PlayerMovement>();
        playerCam = pm.cameraHolder.GetComponentInChildren<Camera>();
        bulletsLeft = magazineSize;
        readyToShoot = true;
        bullet.SetActive(false);
    }

    private void Start()
    {
        attackPoint = pm.gunTip;
        
    }

    public override void UseItem()
    {
        if (allowButtonHold) shooting = Input.GetKey(KeyCode.Mouse0);
        else shooting = Input.GetKeyDown(KeyCode.Mouse0);
        // Reloading
        if (Input.GetKeyDown(KeyCode.R) && bulletsLeft < magazineSize && !reloading) Reload();
        // Reload automatically if no ammo left
        if (readyToShoot && shooting && !reloading && bulletsLeft <= 0) Reload();
        // Shooting
        if (readyToShoot && !reloading && shooting && bulletsLeft > 0)
        {
            bulletsShot = 0;
            Shoot();
        }
    }

    private void Shoot()
    {
        readyToShoot = false;
        Ray ray = playerCam.ViewportPointToRay(new Vector3(.5f, .5f, 0));
        RaycastHit hit;
        Vector3 targetPoint;
        if (Physics.Raycast(ray, out hit))
            targetPoint = hit.point;
        else
            targetPoint = ray.GetPoint(75);

        Vector3 directionWithoutSpread = targetPoint - attackPoint.position;

        float x = Random.Range(-spread, spread);
        float y = Random.Range(-spread, spread);
        Vector3 directionWithSpread = directionWithoutSpread + new Vector3(x, y, 0f);
        GameObject curBullet = Instantiate(bullet, attackPoint.position, Quaternion.identity);
        curBullet.SetActive(true);
        curBullet.transform.forward = directionWithoutSpread.normalized;
        curBullet.GetComponent<Rigidbody>().AddForce(directionWithSpread.normalized * shootForce, ForceMode.Impulse);
        curBullet.GetComponent<Rigidbody>().AddForce(playerCam.transform.up * upwardForce, ForceMode.Impulse);
        bulletsLeft--;
        bulletsShot++;

        if (allowInvoke)
        {
            Invoke(nameof(ResetShot), timeBetweenShooting);
            allowInvoke = false;
        }
        
        // Shoot multiple bullets
        if (bulletsShot < bulletsPerTap && bulletsLeft > 0)
            Invoke(nameof(Shoot), timeBetweenShots);
    }

    private void ResetShot()
    {
        readyToShoot = true;
        allowInvoke = true;
    }

    private void Reload()
    {
        reloading = true;
        Invoke(nameof(ReloadFinished), reloadTime);
    }

    private void ReloadFinished()
    {
        bulletsLeft = magazineSize;
        reloading = false;
    }

    public override void RemoveTemporaryPlayerComponents()
    {
        // Destroy(bullet.gameObject);
    }

    public override void LateUseItem()
    {
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
