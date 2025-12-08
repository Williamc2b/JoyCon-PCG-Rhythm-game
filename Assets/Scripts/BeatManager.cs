using UnityEngine;

public class BeatManager : MonoBehaviour
{
    public GameObject[] Lanes;
    public GameObject[] notes;
    public float noteSpawnFrequency;
    public float tempo;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }
    void spawnRandomNote()
    {
        int laneIndex = Random.Range(0, Lanes.Length);
        GameObject lane = Lanes[laneIndex];
        int noteIndex = Random.Range(0, notes.Length);
        GameObject note = notes[noteIndex];
        Instantiate(note, lane.transform.position, Quaternion.identity);
    }
    // Update is called once per frame
    void Update()
    {
        spawnRandomNote();
    }
}
