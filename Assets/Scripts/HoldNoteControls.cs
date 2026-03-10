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
    float offset = 0.6f;
    private bool headHit = false;
    private bool isheld = false;
    private bool tailPassed = false;
    private int tailPassedFrame = -1;

    void Start()
    {
        lane = JudgementLine.GetComponent<Lane>();
    }

    void Update()
    {
        if (!tailPassed)
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
        // Released too early
        if (isheld && lane.releasedThisFrame && !tailPassed)
        {
            Debug.Log("Released too early!");
            if (tail != null) Destroy(tail);
            if (body != null) Destroy(body.gameObject);
            Destroy(gameObject);
            return;
        }
        if(tail==null && headHit)
        {
            return;
        }
        if (isheld&&headHit==true&&tailPassed==false)
        {

            float tailDistance = Vector3.Distance(tail.transform.position, JudgementLine.transform.position);

                if (body != null)
                {
                    // Mathf.Abs handles negative bodyLength from lanes 1 & 2
                    body.transform.Translate(direction * 0* Time.deltaTime);
                    body.size = new Vector2(body.size.x,tailDistance);

                    // // Anchor body midpoint between judgement line and tail
                    // body.transform.position = JudgementLine.transform.position +
                    //     (tail.transform.position - JudgementLine.transform.position) / 2f;
                }

                if (tailDistance <= offset && Time.frameCount > tailPassedFrame)
                {
                    tailPassed = true;
                    tailPassedFrame = Time.frameCount;
                }
        }
    }

    bool HasPassedLine()
    {
        if (head == null) return false;
        Vector3 toLine = JudgementLine.transform.position - head.transform.position;
        return Vector3.Dot(toLine, direction) < -0.1f;
    }

    bool TailHasPassedOrReachedLine()
    {
        if (tail == null) return false;
        Vector3 toLine = JudgementLine.transform.position - tail.transform.position;
        return Vector3.Dot(toLine, direction) < -0.1f;
    }
}