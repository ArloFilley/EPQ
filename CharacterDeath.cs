using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CharacterDeath : MonoBehaviour
{
    public float maxHealth;
    public float health;

    public Transform Player;

    public PlayerMovement pm;
    public WallRunning wallrun;
    public Sliding slide;
    public PlayerCam cam;
    
    public void Start()
    {
        health = maxHealth;
    }

    // Update is called once per frame
    void Update()
    {   
        if (health <= 0) {
            pm.enabled = false;
            wallrun.enabled = false;
            slide.enabled = false;
            cam.enabled = false;
        }
    }

    void Restart()
    {
        // Reset Player Positions
        Player.position = new Vector3(0,0,0);

        // Enable movement Scripts
        pm.enabled = true;
        wallrun.enabled = true;
        slide.enabled = true;
        cam.enabled = true;

        health = maxHealth;
    }

    public void Damage(float amount)
    {
        health -= amount;
    }
}
