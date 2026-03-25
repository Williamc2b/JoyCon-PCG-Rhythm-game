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
    public float holdnoteDuration;
    float offset = 0.6f;
    public float releaseToleranceWindow = 0.15f;

    private bool headHit = false;
    private bool isHeld = false;
    private bool shrinking = false;
    private bool releasedEarly = false;
    private bool noteCompleted = false;
    private bool tooLong = false;
    private float holdTimer = 0f;
    private float releaseHeldTimer = 0f;
    private float originalBodySize = 0f;
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
                bodySizeSign = Mathf.Sign(body.size.y); // *** ADD ***
                if (bodySizeSign == 0f) bodySizeSign = 1f;
                originalBodySize = Mathf.Abs(body.size.y);
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

        // Tolerance window for early release
        if (shrinking && !noteCompleted && !isHeld && !releasedEarly)
        {
            releaseHeldTimer += Time.deltaTime;

            if (releaseHeldTimer >= releaseToleranceWindow)
            {
                releasedEarly = true;
            }
        }

        // Reset tolerance timer if player re-presses within window
        if (shrinking && isHeld)
        {
            releaseHeldTimer = 0f;
        }

        if (releasedEarly)
        {
            Debug.Log("Released too early!");
            if (body != null) Destroy(body.gameObject);
            Destroy(gameObject);
            return;
        }

        // Count hold duration and shrink body
        if (shrinking && !noteCompleted)
        {
            holdTimer += Time.deltaTime;

            if (body != null)
            {
                float t = Mathf.Clamp01(holdTimer / holdnoteDuration);
                float newSize = Mathf.Lerp(originalBodySize, 0, t);
                body.size = new Vector2(body.size.x, bodySizeSign * newSize); // *** CHANGE ***
                body.transform.position = JudgementLine.transform.position;
            }

            if (holdTimer >= holdnoteDuration)
            {
                noteCompleted = true;
                if (body != null) Destroy(body.gameObject);
            }
        }
        if (tooLong)
        {
            Debug.Log("Hold note failed due to holding too long!");
            Destroy(gameObject);
            return;
        }
        // Wait for release after note completes
        if (noteCompleted)
        {
            if (!isHeld)
            {
                Debug.Log("Hold note fully hit!");
                Destroy(gameObject);
                return;
            }
            else
            {
                tooLong = true;
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