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
        int laneIndex = Random.Range(0, Lanes.Length);
        GameObject lane = Lanes[laneIndex];
        int holdIndex = Random.Range(0, hold_Notes.Length);
        Hold_Note spawn_hold = hold_Notes[holdIndex];
        float hold_duration = Random.Range(1f, 3f);
        Setspawnoffset(laneIndex);

        StartCoroutine(SpawnHoldNoteCoroutine(lane, spawn_hold, hold_duration, laneIndex));
    }
    Vector3 setDirection(int laneIndex)
    {
        if(laneIndex == 0)
        {
            return Vector3.up;
        }
        else if(laneIndex == 1)
        {
            return Vector3.down;
        }
        else if(laneIndex == 2)
        {
            return Vector3.down;
        }
        else if(laneIndex == 3)
        {
            return Vector3.up;
        }
        return Vector3.zero;
    }
    IEnumerator SpawnHoldNoteCoroutine(GameObject lane, Hold_Note spawn_hold, float hold_duration, int laneIndex)
    {
        GameObject holdNote = new GameObject("HoldNote");
        holdNote.transform.position = lane.transform.position + spawnoffset;
        holdNote.transform.rotation = rotation;

        float travelSpeed = 11f / noteSpawnFrequency;
        float bodyLength = hold_duration * travelSpeed * spawn_hold.bodymultiplier;

        // Head
        GameObject head = Instantiate(spawn_hold.Head, holdNote.transform);
        head.transform.localPosition = Vector3.zero;

        // Body
        GameObject body = Instantiate(spawn_hold.Lane, holdNote.transform);
        body.transform.localPosition = Vector3.zero;
        SpriteRenderer bodySR = body.GetComponent<SpriteRenderer>();
        bodySR.drawMode = SpriteDrawMode.Tiled;
        bodySR.size = new Vector2(bodySR.size.x, 0);

        // Add movement immediately
        HoldnoteControls holdNoteControl = holdNote.AddComponent<HoldnoteControls>();
        holdNoteControl.speed = travelSpeed;
        holdNoteControl.direction = setDirection(laneIndex);

        // Grow body over hold_duration
        float elapsed = 0f;
        while (elapsed < hold_duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / hold_duration);
            bodySR.size = new Vector2(bodySR.size.x, Mathf.Lerp(0, bodyLength, t));
            yield return null;
        }

        bodySR.size = new Vector2(bodySR.size.x, bodyLength);

        // Tail spawned only after body is fully grown
        GameObject tail = Instantiate(spawn_hold.Tail, holdNote.transform);
        tail.transform.localPosition = new Vector3(0,  -(bodyLength / 2f)-0.5f, 0);
    }

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