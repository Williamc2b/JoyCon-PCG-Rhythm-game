using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class Lane : MonoBehaviour
{
    public InputActionReference stickAction;
    public Transform hitLine; // The line where notes should be hit
    public float hitWindow = 0.2f; // Time window for hitting notes
    private Vector2 lastStickInput;
    private float flickCooldown = 0.15f;
    private float lastFlickTime;
    public bool flickedThisFrame = false;
    void Start()
    {
        stickAction.action.Enable();
    }
    
    private bool flickDetected = false;

    void Update()
    {
        Vector2 currentStick = stickAction.action.ReadValue<Vector2>();
        
        // Check if we're still in cooldown
        bool canFlick = Time.time - lastFlickTime > flickCooldown;
        
        if (canFlick && lastStickInput.magnitude < 0.3f && currentStick.magnitude > 0.5f)
        {
            flickDetected = true;
            flickedThisFrame = true;
            lastFlickTime = Time.time;
            Debug.Log($"Flick detected at {Time.time}");
        }
        else
        {
            flickedThisFrame = false;
        }
        
        // Reset flickDetected after processing
        if (flickDetected)
        {
            flickDetected = false;
        }
        
        lastStickInput = currentStick;
    }
}