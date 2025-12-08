using UnityEngine;

public class NoteControls : MonoBehaviour
{
    private float beattempo;
    public GameObject JudgementLine;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, JudgementLine.transform.position, beattempo * Time.deltaTime);
    }
    public void setbeattempo(float tempo)
    {
        beattempo = tempo;
    }  
}
