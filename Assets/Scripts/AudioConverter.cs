using System.Collections;
using UnityEngine;
using AForge;
using AForge.Math;//Plus library for complex numbers and FFT

public class Beatmap
{
    public float bpm;
    public string mapName;
    public float mapDuration;

}

public class AudioConverter : MonoBehaviour
{
    public AudioSource audioSource;
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
            float[] flux=FFT(audio);//perform FFT and get spectral flux
            float bpm=GetBPM(flux, audio.frequency, 1024, 512);//get BPM from spectral flux
            Debug.Log("Estimated BPM: " + bpm);
            Beatmap beatmap=GenerateBeatmap(bpm, audio.name, audio.length, flux);//generate beatmap from BPM and other info
            yield return beatmap;
            Debug.Log("Audio conversion completed for: " + audio.name);

        }
    }
    float[] FFT(AudioClip audio)//performs FFT on audio clip
    {
        //audio data
        int windowSize = 1024;
        int sampleRate = audio.frequency;
        int channels = audio.channels;//L and R channels
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

        //Implement hann window to reduce spectral leakage and smooth out the signal window
        int windowslide= windowSize / 2;
        float[] flux = new float[monoSamples.Length-windowSize/windowslide];
        float[] prevMagnitude = new float[windowslide];
        for (int i = 0; i+windowSize < monoSamples.Length; i += windowslide)
        {
            //Hanning window formula: w(n) = 0.5 * (1 - cos(2 * pi * n / (N - 1)))
            Complex[] complexSamples = new Complex[windowSize];
            for (int j = 0; j < windowSize; j++)
            {
                float windowValue = 0.5f * (1 - Mathf.Cos(2 * Mathf.PI * j / (windowSize - 1)));//Hann window
                complexSamples[j] = new Complex(monoSamples[i+j] * windowValue, 0);//apply window to samples
            }

            FourierTransform.FFT(complexSamples, FourierTransform.Direction.Forward);
            int   frameIndex = i / windowslide;
            float frameFlux  = 0f;
            for (int k = 0; k < windowslide; k++)
            {
                float magnitude=Mathf.Sqrt((float)(complexSamples[k].Re * complexSamples[k].Re + complexSamples[k].Im * complexSamples[k].Im));

                float changeinMag = magnitude-prevMagnitude[k];
                if(changeinMag > 0f)
                {
                    frameFlux += changeinMag;
                }
                prevMagnitude[k] = magnitude;
            }
            flux[frameIndex] = frameFlux;
        }
        float maxflux=0f;
        foreach(float f in flux)
        {
            if(f>maxflux)
            {
                maxflux=f;
            }
        }
        if(maxflux>0f)
        {
            for(int i=0;i<flux.Length;i++)
            {
                flux[i] /= maxflux;
            }
        }
        return flux; // return the normalized flux values
    }
    float GetBPM(float[] flux, int sampleRate, int windowSize, int windowslide)
    {

        return 120f; // Example BPM value
    }
    Beatmap GenerateBeatmap(float bpm, string mapName, float mapDuration,float[] flux)
    {
        Beatmap beatmap = new Beatmap();
        beatmap.bpm = bpm;
        beatmap.mapName = mapName;
        beatmap.mapDuration = mapDuration;
        return beatmap;
    }

}