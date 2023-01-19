using UnityEngine;
using UnityEngine.SceneManagement;

public class ObjectCollision : MonoBehaviour
{
    private PlayerMovement pm;

    private void Awake()
    {
        pm = FindObjectOfType<PlayerMovement>();
    }

    public enum ObjectType
    {
        None,
        Lava,
        Portal,
        Arrow,
        Spike
    }

    public ObjectType type = ObjectType.None;

    private void OnTriggerEnter(Collider other)
    {
        if (other.name == "PlayerObj")
        {
            switch (type)
            {
                case ObjectType.Lava:
                    pm.CheckpointRespawn();
                    break;
                case ObjectType.Portal:
                    PauseMenu.levelComplete = true;
                    Time.timeScale = 0f;
                    break;
                case ObjectType.Arrow:
                    SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
                    break;
            }
        }
    }
}
