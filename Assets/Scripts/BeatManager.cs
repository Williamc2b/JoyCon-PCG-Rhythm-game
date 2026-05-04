using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;
using System.IO;
using System.Collections.Generic;

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

    public float tempo;
    private float currentTime;
    private Vector3 spawnoffset;
    private Quaternion rotation;

    private string mapFolderPath;
    private AudioClip musicClip;
    private Beatmap beatmap;
    private List<NoteEvent> pendingNotes;
    private AudioSource musicSource;
    private int currentNoteIndex;


    public void setMapFolderPath(string path)
    {
        mapFolderPath = path;
        Debug.Log("Map folder path set to: " + mapFolderPath);
    }
    public void setMusicClip(AudioClip clip)
    {
        musicClip = clip;
        Debug.Log("Music clip set to: " + musicClip.name);
    }

    void Start()
    {
        musicSource = GetComponent<AudioSource>();
        tempo=tempo/60f;
        string path = GameManager.Instance.selectedMapPath;
        AudioClip clip = GameManager.Instance.selectedMusicClip;
        Debug.Log("Loaded map path: " + path);
        setMapFolderPath(path);
        setMusicClip(clip);
        musicSource.clip = musicClip;
        LoadBeatmap();
    }
    void LoadBeatmap()
    {
        // Read and deserialise the JSON file
        string json = File.ReadAllText(mapFolderPath);
        beatmap = JsonUtility.FromJson<Beatmap>(json);
        pendingNotes = new List<NoteEvent>(beatmap.beatEvents);
        // Start playing
        StartCoroutine(PlayBeatmap());
    }

    IEnumerator PlayBeatmap()
    {
        // Wait for audio to start
        musicSource.Play();
        currentNoteIndex = 0;

        float travelTime = CalculateTravelTime();
        Debug.Log("Travel time: " + travelTime);

        while (currentNoteIndex < pendingNotes.Count)
        {
            NoteEvent noteEvent = pendingNotes[currentNoteIndex];

            float spawnTime = (float)noteEvent.timestamp - travelTime;

            while (musicSource.time < spawnTime)
            {
                yield return null;
            }

            // Spawn the note
            SpawnNoteFromEvent(noteEvent);
            currentNoteIndex++;
        }
        Debug.Log("All notes spawned");
    }   
    float CalculateTravelTime()
    {
        // Distance from spawn point to judgement line
        float distance = 11f;
        // Speed the note travels
        float speed = distance / tempo;
        return speed;
    }
    void SpawnNoteFromEvent(NoteEvent noteEvent)
    {
        if (noteEvent.type.name == "Tap Note")
        {
            spawnTapNote();
        }
        else if (noteEvent.type.name == "Hold Note")
        {
            SpawnHoldNote((float)noteEvent.duration);
        }
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

    void spawnTapNote()
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

    void SpawnHoldNote(float duration)
    {
        int laneIndex = Random.Range(0, Lanes.Length);//random change for real implementation

        GameObject lane = Lanes[laneIndex];
        Hold_Note spawn_hold = hold_Notes[laneIndex];
        Setspawnoffset(laneIndex);

        float travelSpeed = tempo;
        float bodyLength = duration * spawn_hold.bodymultiplier;
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

        float travelSpeed = tempo;

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
        holdNoteControl.holdnoteDuration = hold_duration;

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

    }
}