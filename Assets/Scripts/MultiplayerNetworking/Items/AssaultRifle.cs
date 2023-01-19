using UnityEngine;
using System.Collections;
using Photon.Pun;
public class AssaultRifle : MultiplayerGun
{
    public int maxAmmo = 10;
    private int currentAmmo;
    public float reloadTime = 1f;
    private float nextTimeToFire = 0f;
    public float spread = 1f;
    public float fireRate = 15f;
    private bool isReloading = false;
    private AudioSource assaultAudioSource;
    public Animator animator;
    private static readonly int Reloading = Animator.StringToHash("Reloading");
    private static readonly int Shooting = Animator.StringToHash("Shooting");
    private void Start()
    {
        assaultAudioSource = GetComponent<AudioSource>();
        if (assaultAudioSource != null)
        {
            assaultAudioSource.playOnAwake = false;
            assaultAudioSource.loop = false;
        }
        
        if (currentAmmo == -1)
            currentAmmo = maxAmmo;
        
    }

    private void OnEnable()
    {
        isReloading = false;
        animator.SetBool(Reloading, false);
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
        if (PhotonNetwork.IsMasterClient) StartCoroutine(DestroyBulletDelay(7, newBullet));
    }

    public override void Use()
    {
        if (isReloading) return;
        if (currentAmmo <= 0)
        {
            StartCoroutine(Reload());
            return;
        }
        if (Time.time >= nextTimeToFire && Input.GetButton("Fire1"))
        {
            if (assaultAudioSource != null) assaultAudioSource.Play();
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
            playerCam.fieldOfView = Mathf.Lerp(playerCam.fieldOfView,  (float) pc.playerHash["cameraFOV"] - 40, 0.5f);
        }
        else
        {
            isAiming = false;
            playerCam.fieldOfView = Mathf.Lerp(playerCam.fieldOfView,  (float) pc.playerHash["cameraFOV"], 0.5f);
        }
    }

    private IEnumerator Reload()
    {
        isReloading = true;
        animator.SetBool(Reloading, true);
        yield return new WaitForSeconds(reloadTime);
        animator.SetBool(Reloading, false);
        currentAmmo = maxAmmo;
        isReloading = false;
    }

}