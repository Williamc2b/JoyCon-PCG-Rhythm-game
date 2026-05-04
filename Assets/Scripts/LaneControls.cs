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

    public InputActionReference buttonAction; // hold note
    public bool pressedThisFrame = false;
    public bool isHeld = false;
    public bool releasedThisFrame = false;
    void Start()
    {
        stickAction.action.Enable();
        buttonAction.action.Enable();
    }
    
    private bool flickDetected = false;

    void Update()
    {
        Vector2 currentStick = stickAction.action.ReadValue<Vector2>();
        // Check cooldown
        bool canFlick = Time.time - lastFlickTime > flickCooldown;
        if (canFlick && lastStickInput.magnitude < 0.3f && currentStick.magnitude > 0.5f)
        {
            flickDetected = true;
            flickedThisFrame = true;
            lastFlickTime = Time.time;
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

        // Hold note
        pressedThisFrame = buttonAction.action.WasPressedThisFrame();
        releasedThisFrame = buttonAction.action.WasReleasedThisFrame();
        isHeld = buttonAction.action.IsPressed();
    }
}