using UnityEngine;

public class HoldnoteControls : MonoBehaviour
{
    public float speed;
    public Vector3 direction = Vector3.down;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private bool isHolding = false;
    private bool fullyHeld = false;
    private bool isMissed = false;
    private bool isReleased = false;
    private GameObject head;
    private GameObject tail;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        transform.Translate(direction * speed * Time.deltaTime);
    }
}
