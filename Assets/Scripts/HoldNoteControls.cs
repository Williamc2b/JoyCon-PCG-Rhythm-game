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
    public bool stopGrowing = false;
    float offset = 0.6f;
    private bool headHit = false;
    private bool isheld = false;
    private bool shrinking = false;
    private float bodySizeSign = 1f;

    void Start()
    {
        lane = JudgementLine.GetComponent<Lane>();
    }

    void Update()
    {
        if (!shrinking)
        {
            transform.Translate(direction * speed * Time.deltaTime);
        }

        float distanceToLine = Vector3.Distance(transform.position, JudgementLine.transform.position);

        // Hit head within offset window
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

        // Released too early (before shrinking starts)
        if (isheld && lane.releasedThisFrame &&(body.size.y)>1f)
        {
            Debug.Log("Released too early!");
            if (body != null) Destroy(body.gameObject);
            Destroy(gameObject);
            return;
        }

        // Switch to shrink mode once head reaches judgement line
        if (isheld && !shrinking && headHit && distanceToLine <= offset)
        {
            shrinking = true;
            stopGrowing = true;
            bodySizeSign = Mathf.Sign(body.size.y) == 0 ? 1f : Mathf.Sign(body.size.y);
            if (body != null)
            {
                body.transform.SetParent(null);
                body.transform.position = JudgementLine.transform.position;
            }
        }

        if (shrinking && body != null)
        {
            float currentSize = Mathf.Abs(body.size.y);
            float newSize = currentSize - Mathf.Abs(speed) * Time.deltaTime;
            Debug.Log($"{currentSize}");
            if (currentSize<offset && isheld==false)
            {
                Debug.Log("Hold note fully hit!");
                Destroy(body.gameObject);
                Destroy(gameObject);
                return;
            }
            else if (currentSize==0&& isheld==true)
            {
                Debug.Log("Released too late");
                Destroy(body.gameObject);
                Destroy(gameObject);
                return;
            }

            // Preserve sign so lanes 1 & 2 keep rendering correctly
            body.size = new Vector2(body.size.x, bodySizeSign * newSize);
            body.transform.position = JudgementLine.transform.position;
        }
    }

    bool HasPassedLine()
    {
        if (head == null) return false;
        Vector3 toLine = JudgementLine.transform.position - head.transform.position;
        return Vector3.Dot(toLine, direction) < -0.1f;
    }
}