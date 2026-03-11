using System.Windows.Forms;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;


public class Beatmap
{
    public float bpm;
    public string mapName;
    public float mapDuration;

}
public class AudioConverter : MonoBehaviour
{
    public AudioSource audioSource;
    
    public void ConvertAudio()
    {        // Example: Convert the audio clip to a different format (e.g., WAV)
        AudioClip audio = audioSource.clip;
        if (audio == null)
        {
            Debug.LogError("No audio clip assigned to the AudioSource.");
            return;
        }
        audioSource.GetSpectrumData(new float[1024], 0, FFTWindow.Rectangular);
        Debug.Log("Converting audio clip: " + audio.name);
    }
}
