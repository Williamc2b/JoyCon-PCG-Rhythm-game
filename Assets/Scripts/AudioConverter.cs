using System.Collections;
using UnityEngine;
using AForge;
using AForge.Math;


public class Beatmap
{
    public float bpm;
    public string mapName;
    public float mapDuration;

}
public class AudioConverter : MonoBehaviour
{
    public AudioSource audioSource;
    
    //beat detection algorithm adapted from https://www.youtube.com/watch?v=VbXkYHjK8sE&t=1s
    public void getBPM()
    {
        
    }
    public void ConvertAudio()
    {  
        AudioClip audio = audioSource.clip;
        if (audio == null)
        {
            Debug.LogError("No audio clip assigned to the AudioSource.");
            return;
        }
        Debug.Log("Converting audio clip: " + audio.name);

        StartCoroutine(ConvertAudioCoroutine(audio));

        IEnumerator ConvertAudioCoroutine(AudioClip audio)
        {
            // Simulate a time-consuming conversion process
            float bpm=FFT(audio);
            yield return new WaitForSeconds(2f); // Simulate conversion time
            Debug.Log("Audio conversion completed for: " + audio.name);

        }
    }
    float FFT(AudioClip audio)
    {
        //audio data
        int windowSize = 1024;
        int sampleRate = audio.frequency;
        int channels = audio.channels;
        float[] samples = new float[audio.samples * channels];
        audio.GetData(samples, 0);//get audio data into samples array

        //convert stereo audio to mono to allow FFT to function
        float[] monoSamples = new float[audio.samples];
        for (int i = 0; i < audio.samples; i++)
        {
            float sum = 0f;
            for (int j = 0; j < channels; j++)
            {
                sum += samples[i * channels + j];
            }
            monoSamples[i] = sum / channels;
        }
        //Implement han window to reduce spectral leakage
        int windowslide= (int)(windowSize / 2);
        for (int i = 0; i+windowSize < monoSamples.Length; i += windowslide)
        {
            //Hanning window formula: w(n) = 0.5 * (1 - cos(2 * pi * n / (N - 1)))
            Complex[] complexSamples = new Complex[(int)windowSize];
            for (int j = 0; j < windowSize; j++)
            {
                float windowValue = 0.5f * (1 - Mathf.Cos(2 * Mathf.PI * j / (windowSize - 1)));//Hanning window
                complexSamples[j] = new Complex(monoSamples[j] * windowValue, 0);
            }

            FourierTransform.FFT(complexSamples, FourierTransform.Direction.Forward);
        }


        return 120f; // Example BPM
    }
}
