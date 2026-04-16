using System.Collections;
using UnityEngine;
using System.IO;
using AForge.Math;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using System;
using Unity.VisualScripting;
//source: https://www.aforgenet.com/framework/docs/html/namespace_a_forge_1_1_math.html

[System.Serializable]
public class Notetypes
{
    public string name;
}
[System.Serializable]
public class NoteEvent
{
    public double timestamp;
    public Notetypes type;
    public double duration;
    
}
[System.Serializable]
public class Beatmap
{
    public float bpm;
    public string mapName;
    public double mapDuration;
    public List<NoteEvent> beatEvents = new List<NoteEvent>();
}

public class AudioConverter : MonoBehaviour
{
    public AudioSource audioSource;
    public GameObject LoadingScreen;
    public Slider ProgressBar;
    public TextMeshProUGUI ProgressText;
    public string songFolder;
    public string beatmapJson;

    public void ConvertAudio()
    {  
        AudioClip audio = audioSource.clip;
        if (audio == null)
        {
            Debug.LogError("No audio clip assigned to the AudioSource.");
            return;
        }
        StartCoroutine(ConvertAudioCoroutine(audio));
        
        IEnumerator ConvertAudioCoroutine(AudioClip audio)
        {
            LoadingScreen.SetActive(true);
            SetProgress(0f, "Starting conversion for: " + audio.name);

            float[] flux = null;
            yield return StartCoroutine(FFT(audio, result => flux = result));

            SetProgress(0.6f, "Estimating BPM for: " + audio.name);
            yield return null;

            float bpm = GetBPM(flux, audio.frequency, 512);
            SetProgress(0.8f, "Generating beatmap for: " + audio.name);

            Beatmap Easy   = GenerateBeatmap(audio, bpm, audio.name + " (Easy)",   audio.length, flux, 0.7f, 2.0f);
            Beatmap Medium = GenerateBeatmap(audio, bpm, audio.name + " (Medium)", audio.length, flux, 1.0f, 1.5f);
            Beatmap Hard   = GenerateBeatmap(audio, bpm, audio.name + " (Hard)",   audio.length, flux, 1.6f, 1.0f);
            
            SaveBeatmap(Easy,   songFolder);
            SaveBeatmap(Medium, songFolder);
            SaveBeatmap(Hard,   songFolder);
            yield return null;

            LoadingScreen.SetActive(false);
            SetProgress(1f, "Conversion completed for: " + audio.name);
        }
    }

    void SetProgress(float value, string message)
    {
        if (ProgressBar)  ProgressBar.value = value;
        if (ProgressText) ProgressText.text  = message;
    }

    IEnumerator FFT(AudioClip audio, System.Action<float[]> callback)
    {
        SetProgress(0.3f, "Performing FFT on: " + audio.name);

        int windowSize = 1024;
        int channels   = audio.channels;
        float[] samples = new float[audio.samples * channels];
        audio.GetData(samples, 0);

        // Convert stereo to mono
        float[] monoSamples = new float[audio.samples];
        for (int i = 0; i < audio.samples; i++)
        {
            float sum = 0f;
            for (int j = 0; j < channels; j++)
                sum += samples[i * channels + j];
            monoSamples[i] = sum / channels;
        }

        // Hann window + spectral flux
        // Hanning window formula: w(n) = 0.5 * (1 - cos(2 * pi * n / (N - 1)))
        // source: https://en.wikipedia.org/wiki/Window_function#Hann_and_Hamming_windows
        int windowslide = windowSize / 2;
        int frameCount = 1 + (monoSamples.Length - windowSize) / windowslide;
        if (frameCount < 1) frameCount = 1;
        float[] flux = new float[frameCount];
        float[] prevMagnitude = new float[windowSize / 2];

        for (int i = 0; i + windowSize < monoSamples.Length; i += windowslide)
        {
            Complex[] complexSamples = new Complex[windowSize];
            for (int j = 0; j < windowSize; j++)
            {
                float windowValue = 0.5f * (1 - Mathf.Cos(2 * Mathf.PI * j / (windowSize - 1)));
                complexSamples[j] = new Complex(monoSamples[i + j] * windowValue, 0);
            }

            FourierTransform.FFT(complexSamples, FourierTransform.Direction.Forward);

            int   frameIndex = i / windowslide;
            float frameFlux  = 0f;
            for (int k = 0; k < windowslide/2; k++)
            {
                float magnitude   = Mathf.Sqrt((float)(complexSamples[k].Re * complexSamples[k].Re + complexSamples[k].Im * complexSamples[k].Im));
                float changeinMag = magnitude - prevMagnitude[k];
                if (changeinMag > 0f)
                    frameFlux += changeinMag;
                prevMagnitude[k] = magnitude;
            }
            flux[frameIndex] = frameFlux;
            yield return null;
        }

        // Normalize flux
        float maxflux = 0f;
        foreach (float f in flux)
            if (f > maxflux) maxflux = f;

        if (maxflux > 0f)
            for (int i = 0; i < flux.Length; i++)
                flux[i] /= maxflux;

        callback(flux);
    }

    float GetBPM(float[] flux, int sampleRate, int windowslide)
    {
        SetProgress(0.5f, "Estimating BPM");
        float FPS    = (float)sampleRate / windowslide;
        int   minBPM = Mathf.RoundToInt(FPS * 60f / 250f);
        int   maxBPM = Mathf.Min(Mathf.RoundToInt(FPS * 60f / 60f), flux.Length - 1);

        float bestScore = float.MinValue;
        int   bestLag   = minBPM;

        for (int lag = minBPM; lag <= maxBPM; lag++)
        {
            float score = 0f;
            int   n     = flux.Length - lag;
            for (int j = 0; j < n; j++)
                score += flux[j] * flux[j + lag];
            score /= n;

            if (score > bestScore)
            {
                bestScore = score;
                bestLag   = lag;
            }
        }

        float rawBpm = 60f / (bestLag / FPS);
        float bpm    = BPMCorrection(rawBpm, flux, FPS, bestScore);
        bpm = Mathf.Round(bpm / 5f) * 5f;
        return bpm;
    }

    float BPMCorrection(float bpm, float[] flux, float FPS, float bestScore)
    {
        float maxBpm    = 250f;
        float threshold = 0.85f;
        float doubleBpm = bpm * 2f;

        while (doubleBpm <= maxBpm)
        {
            int halfLag = Mathf.RoundToInt(60f / doubleBpm * FPS);
            if (halfLag < 1 || halfLag >= flux.Length) break;

            float halfScore = 0f;
            int   n         = flux.Length - halfLag;
            for (int j = 0; j < n; j++)
                halfScore += flux[j] * flux[j + halfLag];
            halfScore /= n;

            if (halfScore >= bestScore * threshold)
            {
                bpm       = doubleBpm;
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

    Beatmap GenerateBeatmap(AudioClip audio, float bpm, string mapName, float mapDuration, float[] flux, float notedensity, float tuner)
    {
        Beatmap Newbeatmap    = new Beatmap();
        Newbeatmap.mapName    = mapName;
        Newbeatmap.bpm        = bpm;
        double mapDurationdouble=mapDuration;
        Newbeatmap.mapDuration = Math.Round(mapDurationdouble * 1000) / 1000;
        float sampleRate    = audio.frequency;
        float windowSlide   = 512;
        float FPS           = (float)sampleRate / windowSlide;
        float secondsPerBeat = 60f / bpm;
        float framesPerBeat  = FPS * secondsPerBeat;

        Notetypes tapenote = new Notetypes { name = "Tap Note" };
        Notetypes holdnote = new Notetypes { name = "Hold Note" };

        // Statistical analysis of spectral flux to obtain detection threshold
        // variance formula: σ² = (1/N) * Σ(xᵢ - μ)²
        // standard deviation formula: σ = √σ²
        float mean = 0f;
        foreach (float f in flux) mean += f;
        mean /= flux.Length;

        float variance = 0f;
        foreach (float f in flux)
            variance += (f - mean) * (f - mean);

        float stdDev = Mathf.Sqrt(variance / flux.Length);
        float threshold = mean + tuner * stdDev;

        float framePos = 0f;
        int   lastNoteFrame = -9999;
        int   minSpacing = Mathf.RoundToInt(framesPerBeat * 0.75f);

        while (framePos < flux.Length)
        {
            int frameIndex = Mathf.RoundToInt(framePos);
            if (frameIndex >= flux.Length) break;

            int searchRadius = Mathf.RoundToInt(framesPerBeat * 0.5f);
            int searchStart  = Mathf.Max(0, frameIndex - searchRadius);
            int searchEnd    = Mathf.Min(flux.Length - 1, frameIndex + searchRadius);

            float peakValue = 0f;
            int   peakFrame = frameIndex;
            for (int i = searchStart; i <= searchEnd; i++)
            {
                if (flux[i] > peakValue)
                {
                    peakValue = flux[i];
                    peakFrame = i;
                }
            }

                    if (peakValue >= threshold && peakFrame - lastNoteFrame > minSpacing)
        {
            // density filtering
            if (UnityEngine.Random.value > notedensity * 0.6f)
            {
                framePos += framesPerBeat / notedensity;
                continue;
            }

            double timestamp = Math.Round((peakFrame / FPS) * 1000) / 1000;

            NoteEvent note = new NoteEvent();
            note.timestamp = timestamp;

            float minHoldSeconds = 3f;
            float maxHoldSeconds = 9f;

            int lookAheadMin = Mathf.RoundToInt(minHoldSeconds * FPS);
            int lookAheadMax = Mathf.Min(Mathf.RoundToInt(maxHoldSeconds * FPS), flux.Length - 1 - peakFrame);

            float bestAheadPeak = 0f;
            int bestAheadFrame = -1;

            for (int s = lookAheadMin; s <= lookAheadMax; s++)
            {
                int lookFrame = peakFrame + s;
                if (lookFrame >= flux.Length)
                    break;

                if (flux[lookFrame] > bestAheadPeak && flux[lookFrame] >= threshold)
                {
                    bestAheadPeak = flux[lookFrame];
                    bestAheadFrame = lookFrame;
                }
            }

            if (bestAheadFrame != -1 && UnityEngine.Random.value < 0.15f * notedensity)
            {
                double holdDuration = (bestAheadFrame - peakFrame) / FPS;
                holdDuration = Math.Round(holdDuration * 1000) / 1000;

                note.type = holdnote;
                note.duration = holdDuration;

                framePos = bestAheadFrame + framesPerBeat;
            }
            else
            {
                note.type = tapenote;
                note.duration = 0;
            }

            Newbeatmap.beatEvents.Add(note);
            lastNoteFrame = peakFrame;
        }

        framePos += framesPerBeat / notedensity;
    }

    return Newbeatmap;
    }

    void SaveBeatmap(Beatmap beatmap, string songFolder)
    {
        string json = JsonUtility.ToJson(beatmap, prettyPrint: true);
        beatmapJson = json;
        string path = Path.Combine(songFolder, beatmap.mapName + ".json");
        File.WriteAllText(path, json);
        Debug.Log("Beatmap saved to: " + path);
    }
}