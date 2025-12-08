using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

public class BeatManager : MonoBehaviour
{
    public GameObject[] Lanes;
    public GameObject[] notes;
    public float noteSpawnFrequency;
    public float tempo;
    private float currentTime;
    Vector3 spawnoffset;
    Quaternion rotation;
    // Start is called once before the first execution of Update after the M
    void Start()
    {
        tempo=tempo/60f;
    }
    void spawnRandomNote()
    {
        int laneIndex = Random.Range(0, Lanes.Length);
        GameObject lane = Lanes[laneIndex];
        int noteIndex = Random.Range(0, notes.Length);
        GameObject note = notes[noteIndex];
        if(laneIndex == 0)
        {
            spawnoffset=new Vector3(0, -9, 0);
            rotation=Quaternion.identity;
        }
        else if(laneIndex == 1)
        {
            spawnoffset=new Vector3(-9, 0, 0);
            rotation=Quaternion.Euler(0, 0, 90);
        }
        else if(laneIndex == 2)
        {
            spawnoffset=new Vector3(0, 9, 0);
            rotation=Quaternion.identity;
        }
        else if(laneIndex == 3)
        {
            spawnoffset=new Vector3(9, 0, 0);
            rotation=Quaternion.Euler(0, 0, 90);
        }
        GameObject spawnedNote = Instantiate(note, lane.transform.position + spawnoffset, rotation);

        NoteControls noteControl = spawnedNote.GetComponent<NoteControls>();
        if (noteControl != null)
        {
            noteControl.setbeattempo(tempo);
            noteControl.JudgementLine = lane;
        }
    }
    // Update is called once per frame
    void Update()
    {
        currentTime += Time.deltaTime;
        if(currentTime >= noteSpawnFrequency)
        {
            spawnRandomNote();
            currentTime = 0;
        }
    }
}
