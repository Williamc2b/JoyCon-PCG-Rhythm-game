using System.Runtime.InteropServices;
using UnityEngine;

public class HoldnoteControls : MonoBehaviour
{
    public float speed;
    public Vector3 direction;
    public GameObject head;
    public GameObject tail;
    public SpriteRenderer body;
    public GameObject JudgementLine;
    public Lane lane;
    float offset=0.6f;
    private bool headHit = false;
    private bool isheld = false;
    void Start()
    {
        lane = JudgementLine.GetComponent<Lane>();
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(direction * speed * Time.deltaTime);
        float distanceToLine = Vector3.Distance(transform.position, JudgementLine.transform.position);
        // hold when head it within offset
        if (!headHit && lane.pressedThisFrame && distanceToLine <= offset)
        {
            headHit = true;
            isheld = true;
            Destroy(head);
            Debug.Log("Hit head!");
        }

        // Head passed the line without being hit
        if (!headHit && HasPassedLine())
        {
            Debug.Log("Hold note missed!");
            Destroy(gameObject);
            return;
        }

        // Player released too early
        if (isheld && lane.releasedThisFrame)
        {
            Debug.Log("Released too early!");
            Destroy(gameObject);
            return;
        }

        // Tail reached the line while holding
        if (isheld && tail != null)
        {
            float tailDistance = Vector3.Distance(tail.transform.position, JudgementLine.transform.position);

            // Shrink body to match remaining tail distance
            if (body != null)
            {
                body.size = new Vector2(body.size.x, tailDistance);
            }

            if (tailDistance <= offset)
            {
                Debug.Log("Hold note fully hit!");
                Destroy(gameObject);
                return;
            }
        }
    }

    bool HasPassedLine()
    {
        Vector3 toLine = JudgementLine.transform.position - head.transform.position;
        Debug.Log($"dot: {Vector3.Dot(toLine, direction)}");
        return Vector3.Dot(toLine, direction) < -0.1f;
    }
}
