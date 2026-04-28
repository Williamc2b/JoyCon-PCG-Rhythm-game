using UnityEngine;

public class NoteControls : MonoBehaviour
{
    private float beattempo;
    public GameObject JudgementLine;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public Lane lane;
    public ScoreManager scoreManager;
    void Start()
    {
        lane = JudgementLine.GetComponent<Lane>();
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, JudgementLine.transform.position, beattempo * Time.deltaTime);
        bool isflicked = lane.flickedThisFrame;
        if (isflicked)
        {
            float distanceToLine = Vector3.Distance(transform.position, JudgementLine.transform.position);
            if (distanceToLine < 0.6f)
            {
                Destroy(gameObject);
                Debug.Log("Note Hit!");
                if (scoreManager != null)
                {
                    scoreManager.notehit(100);
                }
            }
        }
        else
        {
            if (Vector3.Distance(transform.position, JudgementLine.transform.position) < 0.1f)
            {
                Destroy(gameObject);
                Debug.Log("Missed Note!");
                if (scoreManager != null)
                {
                    scoreManager.notemiss();
                }
            }
        }

    }
    public void setbeattempo(float tempo)
    {
        beattempo = tempo;
    }  
}
