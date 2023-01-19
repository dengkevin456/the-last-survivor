using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
public class ExplosiveBarrel : MonoBehaviour
{
    public Rigidbody rb;
    public float explosionRange;
    public float explosionForce;
    public float upwardsModifier;
    public LayerMask bulletMask;
    public Volume globalVolume;
    private Vignette vignette;
    public static bool setSlowmotion;
    private void Awake()
    {
        globalVolume.sharedProfile.TryGet(out vignette);
        vignette.intensity.value = 0f;
    }

    private void BarrelExplosion()
    {
        Collider[] enemies = Physics.OverlapSphere(transform.position, explosionRange);
        for (int i = 0; i < enemies.Length; i++)
        {
            if (enemies[i].GetComponent<Rigidbody>())
            {
                enemies[i].GetComponent<Rigidbody>().AddExplosionForce(explosionForce,
                    transform.position, explosionRange, upwardsModifier, ForceMode.Impulse);
            }

            else if (enemies[i].GetComponentInParent<Rigidbody>())
            {
                Debug.Log("Added the player!");
                enemies[i].GetComponentInParent<Rigidbody>().AddExplosionForce(explosionForce,
                    transform.position, explosionRange, upwardsModifier, ForceMode.Impulse);
            }
        }
    }
    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.tag == "Grenade")
        {
            setSlowmotion = true;
            Debug.Log("Yes explosion!");
            BarrelExplosion();
        }
    }

    private void ResetSlowmotion()
    {
        setSlowmotion = false;
        if (vignette != null)
        {
            vignette.intensity.SetValue(new NoInterpFloatParameter(0f));
        }
        Time.timeScale = 1f;
    }

    private void Update()
    {
        if (PauseMenu.gameIsPaused) return;
        if (setSlowmotion)
        {
            Time.timeScale = 0.4f;
            vignette.intensity.SetValue(new NoInterpFloatParameter(0.5f));
            Invoke(nameof(ResetSlowmotion), 5f);
        }
    }
}
