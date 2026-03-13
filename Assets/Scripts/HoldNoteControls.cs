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
    public int laneIndex;

    float offset = 0.6f;

    private bool headHit = false;
    private bool isHeld = false;
    private bool shrinking = false;
    private float bodySizeSign = 1f;

    void Start()
    {
        lane = JudgementLine.GetComponent<Lane>();

        if (body != null)
        {
            bodySizeSign = Mathf.Sign(body.size.y);
            if (bodySizeSign == 0) bodySizeSign = 1f;
        }
    }

    void Update()
    {
        if (!shrinking)
        {
            transform.Translate(direction * speed * Time.deltaTime);
        }

        float distanceToLine = Vector3.Distance(transform.position, JudgementLine.transform.position);

        // Hit head
        if (!headHit && lane.pressedThisFrame && distanceToLine <= offset)
        {
            headHit = true;
            isHeld = true;
            shrinking = true;

            Destroy(head);

            if (body != null)
            {
                body.transform.SetParent(null);
                body.transform.position = JudgementLine.transform.position;
            }

            Debug.Log("Hit head!");
        }

        // Missed note
        if (!headHit && HasPassedLine())
        {
            Debug.Log("Hold note missed!");
            Destroy(gameObject);
            return;
        }

        if (headHit)
        {
            isHeld = lane.isHeld;
        }

        if (shrinking && !isHeld)
        {
            Debug.Log("Released too early!");
            if (body != null) Destroy(body.gameObject);
            Destroy(gameObject);
            return;
        }

        // Shrink hold body
        if (shrinking && body != null)
        {
            float currentSize = Mathf.Abs(body.size.y);
            float newSize = currentSize - Mathf.Abs(speed) * Time.deltaTime;

            newSize = Mathf.Max(0, newSize);

            body.size = new Vector2(body.size.x, bodySizeSign * newSize);
            body.transform.position = JudgementLine.transform.position;

            if (newSize <= 0.05f)
            {
                Debug.Log("Hold note fully hit!");
                Destroy(body.gameObject);
                Destroy(gameObject);
                return;
            }
        }
    }

    bool HasPassedLine()
    {
        if (head == null) return false;

        Vector3 toLine = JudgementLine.transform.position - head.transform.position;
        return Vector3.Dot(toLine, direction) < -0.1f;
    }
}