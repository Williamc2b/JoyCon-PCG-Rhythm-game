using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

[System.Serializable]
public class Hold_Note
{
    public GameObject Head;
    public GameObject Lane;
    public GameObject Tail;
    public float hold_Duration;
    public float bodymultiplier=1f;
}

public class BeatManager : MonoBehaviour
{

    public GameObject[] Lanes;
    public GameObject[] notes;
    public Hold_Note[] hold_Notes;
    public float noteSpawnFrequency;
    public float tempo;
    private float currentTime;
    private Vector3 spawnoffset;
    private Quaternion rotation;
    // Start is called once before the first execution of Update after the M
    void Start()
    {
        tempo=tempo/60f;
    }
    void Setspawnoffset(int laneIndex)
    {
        if(laneIndex == 0)
        {
            spawnoffset=new Vector3(0, -11, 0);
            rotation=Quaternion.identity;
        }
        else if(laneIndex == 1)
        {
            spawnoffset=new Vector3(-11, 0, 0);
            rotation=Quaternion.Euler(0, 0, 90);
        }
        else if(laneIndex == 2)
        {
            spawnoffset=new Vector3(0, 11, 0);
            rotation=Quaternion.identity;
        }
        else if(laneIndex == 3)
        {
            spawnoffset=new Vector3(11, 0, 0);
            rotation=Quaternion.Euler(0, 0, 90);
        }
    }
    void spawnRandomNote()
    {
        int laneIndex = Random.Range(0, Lanes.Length);
        GameObject lane = Lanes[laneIndex];
        int noteIndex = Random.Range(0, notes.Length);
        GameObject note = notes[noteIndex];
        Setspawnoffset(laneIndex);
        GameObject spawnedNote = Instantiate(note, lane.transform.position + spawnoffset, rotation);

        NoteControls noteControl = spawnedNote.GetComponent<NoteControls>();
        if (noteControl != null)
        {
            noteControl.setbeattempo(tempo);
            noteControl.JudgementLine = lane;
        }
    }
    void SpawnHoldNote()
    {
        //set which lane and colour of note to spawn
        int laneIndex = Random.Range(0, Lanes.Length);
        GameObject lane = Lanes[laneIndex];
        int holdIndex = Random.Range(0, hold_Notes.Length);
        Hold_Note spawn_hold=hold_Notes[holdIndex];
        float hold_duration = Random.Range(1f, 3f); // Random duration between 1 and 3 seconds
        Setspawnoffset(laneIndex);

        StartCoroutine(SpawnHoldNoteCoroutine(lane, spawn_hold, hold_duration,laneIndex));
    
    }
    IEnumerator SpawnHoldNoteCoroutine(GameObject lane, Hold_Note spawn_hold, float hold_duration, int laneIndex)
    {
        GameObject holdNote = new GameObject("HoldNote");
        holdNote.transform.position = lane.transform.position + spawnoffset;
        holdNote.transform.rotation = rotation;
        
        //head
        GameObject head= Instantiate(spawn_hold.Head, holdNote.transform);
        head.transform.localPosition = Vector3.zero;
        head.transform.localRotation = Quaternion.identity;
        
        //body
        GameObject body = Instantiate(spawn_hold.Lane, holdNote.transform);
        body.transform.localPosition = Vector3.zero;
        body.transform.localRotation = Quaternion.identity;
        float travelSpeed = 11f / noteSpawnFrequency;
        float bodyLength = spawn_hold.hold_Duration * travelSpeed* spawn_hold.bodymultiplier; // Length of body based on duration and multiplier
        Vector3 bodyScale = body.transform.localScale;
        bodyScale.y = bodyLength;
        body.transform.localScale = bodyScale;
        
        //tail
        GameObject spawnedTail = Instantiate(spawn_hold.Tail, holdNote.transform);
        spawnedTail.transform.localPosition = new Vector3(0, -bodyLength, 0);
        spawnedTail.transform.localRotation = Quaternion.identity;
        yield return new WaitForSeconds(hold_duration);

    }
    // Update is called once per frame
    void Update()
    {
        currentTime += Time.deltaTime;
        if(currentTime >= noteSpawnFrequency)
        {
            SpawnHoldNote();
            currentTime = 0;
        }
    }
}
