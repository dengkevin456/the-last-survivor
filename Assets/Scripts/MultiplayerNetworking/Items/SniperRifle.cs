using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class SniperRifle : MultiplayerGun
{
    private AudioSource gunAudioSource;
    public GameObject scopeImage;
    private int currentAmmo;
    public int maxAmmo = 10;
    public float reloadTime = 3f;
    private float nextTimeToFire;
    public float fireRate;
    private bool isReloading = false;
    public float spread = 0.5f;
    public Animator animator;
    private static readonly int Reloading = Animator.StringToHash("Reloading");
    private static readonly int Shooting = Animator.StringToHash("Shooting");
    private void OnEnable()
    {
        isReloading = false;
        animator.SetBool(Reloading, false);
    }

    private void Awake()
    {
        bulletPrefab.gameObject.SetActive(false);
        gunAudioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        if (scopeImage != null) scopeImage.SetActive(false);
        if (gunAudioSource == null) return;
        gunAudioSource.playOnAwake = false;
        gunAudioSource.loop = false;
        if (currentAmmo == -1) currentAmmo = maxAmmo;
    }

    public override void Use()
    {
        if (isReloading) return;
        if (currentAmmo <= 0)
        {
            StartCoroutine(Reload());
        }
        if (Input.GetMouseButtonDown(0) && Time.time >= nextTimeToFire)
        {
            if (gunAudioSource != null) gunAudioSource.Play();
            nextTimeToFire = Time.time + 1f / fireRate;
            Shoot();
        }

        ammoText.text = $"{currentAmmo} / {maxAmmo} !!!";
    }

    public override void AlternateUse()
    {
        if (Input.GetMouseButton(1))
        {
            isAiming = true;
            if (scopeImage != null) scopeImage.SetActive(true);
            if ((float) pc.playerHash["cameraFOV"] >= 60) 
                playerCam.fieldOfView = Mathf.Lerp(playerCam.fieldOfView,  (float) pc.playerHash["cameraFOV"] - 50, 0.5f);
        }
        else
        {
            if (scopeImage != null) scopeImage.SetActive(false);
            isAiming = false;
            playerCam.fieldOfView = Mathf.Lerp(playerCam.fieldOfView,  (float) pc.playerHash["cameraFOV"], 0.5f);
        }
    }


    private IEnumerator Reload()
    {
        isReloading = true;
        Debug.Log("Reloading...");
        animator.SetBool(Reloading, true);
        yield return new WaitForSeconds(reloadTime);
        animator.SetBool(Reloading, false);
        currentAmmo = maxAmmo;
        isReloading = false;
    }

    private void Shoot()
    {
        currentAmmo--;
        shootAnimation.Play();
        Ray ray = playerCam.ViewportPointToRay(new Vector3(.5f, .5f));
        ray.origin = playerCam.transform.position;
        Vector3 targetPoint;
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            targetPoint = hit.point;
        }
        else
        {
            targetPoint = ray.GetPoint(75);
        }

        Vector3 directionWithoutSpread = targetPoint - gunTip.position;
        
        float x = Random.Range(-spread, spread);
        float y = Random.Range(-spread, spread);
        
        Vector3 directionWithSpread;

        if (!isAiming) directionWithSpread = directionWithoutSpread + new Vector3(x, y, 0);
        else directionWithSpread = targetPoint - gunTip.position;
        
        
        GameObject newBullet = PhotonNetwork.Instantiate(bulletPrefab.name, 
            gunTip.position, Quaternion.identity);
        newBullet.GetComponent<BulletBehaviour>().bulletGun = this;
        newBullet.SetActive(true);
        newBullet.transform.forward = directionWithSpread.normalized;
        newBullet.GetComponent<Rigidbody>().AddForce(directionWithSpread.normalized * shootForce, ForceMode.Impulse);
        if (newBullet != null && PhotonNetwork.IsMasterClient) StartCoroutine(DestroyBulletDelay(5, newBullet));

    }
}
