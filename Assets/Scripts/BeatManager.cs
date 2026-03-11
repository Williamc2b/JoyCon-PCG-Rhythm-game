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
            rotation=Quaternion.identity;
        }
        else if(laneIndex == 2)
        {
            spawnoffset=new Vector3(0, 11, 0);
            rotation=Quaternion.identity;
        }
        else if(laneIndex == 3)
        {
            spawnoffset=new Vector3(11, 0, 0);
            rotation=Quaternion.identity;
        }
    }

    void spawnRandomNote()
    {
        int laneIndex = Random.Range(0, Lanes.Length);
        GameObject lane = Lanes[laneIndex];
        GameObject note = notes[laneIndex];
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
        Hold_Note spawn_hold = hold_Notes[laneIndex];
        Setspawnoffset(laneIndex);

        float travelSpeed = 11f/noteSpawnFrequency;
        float bodyLength = Random.Range(2f, 10f) * spawn_hold.bodymultiplier;
        float hold_duration = bodyLength / travelSpeed;

        StartCoroutine(SpawnHoldNoteCoroutine(lane, spawn_hold, hold_duration, bodyLength, laneIndex));
    }

    Vector3 setDirection(int laneIndex)
    {
        if(laneIndex == 0)
        {
            return Vector3.up;
        }
        else if(laneIndex == 1)
        {
            return Vector3.right;
        }
        else if(laneIndex == 2)
        {
            return Vector3.down;
        }
        else if(laneIndex == 3)
        {
            return Vector3.left;
        }
        return Vector3.zero;
    }

    IEnumerator SpawnHoldNoteCoroutine(GameObject lane, Hold_Note spawn_hold, float hold_duration, float bodyLength, int laneIndex)
    {
        GameObject holdNote = new GameObject("HoldNote");
        holdNote.transform.position = lane.transform.position + spawnoffset;
        holdNote.transform.rotation = rotation;

        float travelSpeed = 11f/noteSpawnFrequency;

        // Head
        GameObject head = Instantiate(spawn_hold.Head, holdNote.transform);
        head.transform.localPosition = Vector3.zero;

        // Body
        GameObject body = Instantiate(spawn_hold.Lane, holdNote.transform);
        body.transform.localPosition = Vector3.zero;
        SpriteRenderer bodySR = body.GetComponent<SpriteRenderer>();
        bodySR.drawMode = SpriteDrawMode.Tiled;
        bodySR.size = new Vector2(bodySR.size.x, 0);

        HoldnoteControls holdNoteControl = holdNote.AddComponent<HoldnoteControls>();
        holdNoteControl.speed = travelSpeed;
        holdNoteControl.direction = setDirection(laneIndex);
        holdNoteControl.JudgementLine = lane;
        holdNoteControl.head = head;
        holdNoteControl.body = bodySR;
        holdNoteControl.laneIndex = laneIndex;

        if (laneIndex == 1 || laneIndex == 2)
        {
            bodyLength = bodyLength * -1;
        }
        if(laneIndex == 1 || laneIndex == 3)
        {
            bodySR.transform.rotation = Quaternion.Euler(0, 0, 90);
        }
        float elapsed = 0f;
        while (elapsed < hold_duration)
        {
            if (bodySR == null) yield break;
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / hold_duration);
            bodySR.size = new Vector2(bodySR.size.x, Mathf.Lerp(0, bodyLength, t));
            yield return null;
        }

        bodySR.size = new Vector2(bodySR.size.x, bodyLength);

        // // Tail spawned only after body is fully grown
        // GameObject tail = Instantiate(spawn_hold.Tail, holdNote.transform);
        // if (laneIndex == 1 || laneIndex == 3)
        // {
        //     // Horizontal lanes - offset along x axis
        //     tail.transform.localPosition = new Vector3((bodyLength / 2f) - tailoffset, 0, 0);
        // }
        // else
        // {
        //     // Vertical lanes - offset along y axis
        //     tail.transform.localPosition = new Vector3(0, -(bodyLength / 2f) + tailoffset, 0);
        // }

        // holdNoteControl.tail = tail;
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