using System.Collections;
using UnityEngine;
using AForge;
using AForge.Math;
using System.Collections.Generic;//Plus library for complex numbers and FFT

public class NoteEvent
{
    public float timestamp;
}
public class Beatmap
{
    public float bpm;
    public string mapName;
    public float mapDuration;
    public List<NoteEvent> beatEvents= new List<NoteEvent>();//List of note events with timestamps

}

public class AudioConverter : MonoBehaviour
{
    public AudioSource audioSource;
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
            Debug.Log("Spectral flux calculated for: " + audio.name);
            Debug.Log("Flux length: " + flux.Length);
            float bpm=GetBPM(flux, audio.frequency, 512);//get BPM from spectral flux
            Debug.Log("Estimated BPM: " + bpm);

            //Beatmap beatmap=GenerateBeatmap(bpm, audio.name, audio.length, flux);//generate beatmap from BPM and other info
            //yield return beatmap;
            yield return null; // Placeholder for beatmap generation

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
            //source: https://en.wikipedia.org/wiki/Window_function#Hann_and_Hamming_windows
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
        return flux; 
    }

    float GetBPM(float[] flux, int sampleRate,int windowslide)
    {
        float FPS = (float)sampleRate / windowslide;
        // Estimate BPM by finding the lag with the highest autocorrelation in the spectral flux
        int minLag= Mathf.RoundToInt(FPS * 60f / 250f);
        int maxLag= Mathf.Min(Mathf.RoundToInt(FPS * 60f / 60f),flux.Length - 1);
        float bestScore = float.MinValue;
        int   bestLag   = minLag;
        // Autocorrelation to find periodicity in the flux
        for (int lag = minLag; lag <= maxLag; lag++)
        {
            float score = 0f;
            int n= flux.Length - lag;
            for (int j = 0; j < n; j++)
            {
                score += flux[j] * flux[j + lag];
            }
            score /= n;
            if (score > bestScore) 
            { 
                bestScore = score; 
                bestLag = lag; 
            }
        }
        float rawBpm = 60f / (bestLag / FPS);
        float bpm = CorrectTempo(rawBpm, flux, FPS, bestScore);
        bpm=Mathf.Round(bpm / 5f) * 5f;
        return bpm;
    }
    float CorrectTempo(float bpm, float[] flux, float FPS, float bestScore)
    {
        float maxBpm    = 250f;
        float threshold = 0.85f;
        float doubleBpm = bpm * 2f;
        while (doubleBpm <= maxBpm)
        {
            int halfLag = Mathf.RoundToInt(60f / doubleBpm * FPS);
            if (halfLag < 1 || halfLag >= flux.Length) break;

            // Score the half-lag
            float halfScore = 0f;
            int n = flux.Length - halfLag;
            for (int j = 0; j < n; j++)
            {
                halfScore += flux[j] * flux[j + halfLag];
            }
            halfScore /= n;

            // Only double if the half-lag scores VERY close to the original
            // A genuine slow song will score much worse at double tempo
            // A misdetected fast song will score almost identically at double tempo
            if (halfScore >= bestScore * threshold)
            {
                bpm = doubleBpm;
                doubleBpm = bpm * 2f;
                bestScore = halfScore;
            }
            else
            {
                break;
            }
        }
        return bpm;
    }

    Beatmap GenerateBeatmap(float bpm, string mapName, float mapDuration,float[] flux)
    {
        Beatmap beatmap = new Beatmap();
        beatmap.mapName = mapName;
        beatmap.bpm = bpm;
        beatmap.mapDuration = mapDuration;
        List<NoteEvent> noteSpawnEvents = new List<NoteEvent>();

        //TODO: Use the spectral flux to determine where to place note events in the beatmap. This is a complex task that may involve setting a threshold for flux peaks and spacing out note events based on the BPM and timing of the music. For now, we will just create some placeholder note events at regular intervals based on the BPM.



        return beatmap;
    }

}