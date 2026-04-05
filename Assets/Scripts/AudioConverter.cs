using System.Collections;
using UnityEngine;
using System.IO;
using AForge;
using AForge.Math;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
//source: https://www.aforgenet.com/framework/docs/html/namespace_a_forge_1_1_math.html

[System.Serializable]
public class NoteEvent
{
    public float timestamp;
    
}
[System.Serializable]
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
    public GameObject LoadingScreen;
    public Slider ProgressBar;
    public TextMeshProUGUI ProgressText;
    public string songFolder;
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
            float[] flux=null;//perform FFT and get spectral flux
            yield return StartCoroutine(FFT(audio, result => flux = result));
            Debug.Log("Spectral flux calculated for: " + audio.name);
            Debug.Log("Flux length: " + flux.Length);
            SetProgress(0.6f, "Estimating BPM for: " + audio.name);
            yield return null; // 
            float bpm=GetBPM(flux, audio.frequency, 512);//get BPM from spectral flux
            Debug.Log("Estimated BPM: " + bpm);
            SetProgress(0.8f, "Generating beatmap for: " + audio.name);
            Beatmap beatmap=GenerateBeatmap(audio, bpm, audio.name, audio.length, flux);//generate beatmap from BPM and other info
            SaveBeatmap(beatmap, songFolder);//save beatmap to file
            Debug.Log("Beatmap generated and saved for: " + audio.name);
            yield return null;
            SetProgress(1f, "Conversion completed for: " + audio.name);
            Debug.Log("Audio conversion completed for: " + audio.name);

        }
    }
    void SetProgress(float value, string message)//update progress bar and text
    {
        if (ProgressBar)  ProgressBar.value = value;
        if (ProgressText)   ProgressText.text   = message;
    }

    IEnumerator FFT(AudioClip audio, System.Action<float[]> callback)//performs FFT on audio clip
    {
        SetProgress(0.3f, "Performing FFT on: " + audio.name);
        //audio data
        int windowSize = 1024;
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
        float[] flux = new float[(monoSamples.Length - windowSize) / windowslide];
        float[] prevMagnitude = new float[windowSize / 2]; 
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
            yield return null;

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
        callback(flux);
    }

    float GetBPM(float[] flux, int sampleRate,int windowslide)
    {
        SetProgress(0.5f, "Estimating BPM");
        float FPS = (float)sampleRate / windowslide;
        // Estimate BPM by finding the lag with the highest autocorrelation in the spectral flux
        int minBPM= Mathf.RoundToInt(FPS * 60f / 250f);
        int maxBPM= Mathf.Min(Mathf.RoundToInt(FPS * 60f / 60f),flux.Length - 1);
        float bestScore = float.MinValue;
        int   bestLag   = minBPM;
        // Autocorrelation score for each potential BPM
        for (int lag = minBPM; lag <= maxBPM; lag++)
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
        float bpm = BPMCorrection(rawBpm, flux, FPS, bestScore);
        bpm=Mathf.Round(bpm / 5f) * 5f;
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

            // Score the half-lag
            float halfScore = 0f;
            int n = flux.Length - halfLag;
            for (int j = 0; j < n; j++)
            {
                halfScore += flux[j] * flux[j + halfLag];
            }
            halfScore /= n;

            // Only double if the half-lag scores is close to the original
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

    Beatmap GenerateBeatmap(AudioClip audio, float bpm, string mapName, float mapDuration, float[] flux)
    {
        Beatmap Newbeatmap = new Beatmap();
        Newbeatmap.mapName = mapName;
        Newbeatmap.bpm = bpm;
        Newbeatmap.mapDuration = mapDuration;

        float sampleRate = audio.frequency; // Assuming standard audio sample rate
        float windowSlide = 512; //


        float FPS = (float)sampleRate / windowSlide; // frames per second
        float secondsPerBeat = 60f / bpm;
        float framesPerBeat = FPS * secondsPerBeat;

        //calculate the mean, variance and standard deviation of flux to obtain threshold for note detection, any peaks in spectral flux that exceed the threshold will be considered note events and placed at those timestamps
        float mean = 0f;
        foreach (float f in flux)
        {
            mean += f;
        }
        mean /= flux.Length;

        float variance = 0f;
        foreach (float f in flux)
        {
            //variance formula: σ² = (1/N) * Σ(xᵢ - μ)²
            variance += (f - mean) * (f - mean);
        }

        //standard deviation formula: σ = √σ²
        float stdDev = Mathf.Sqrt(variance / flux.Length);

        float tuner = 1.5f;//tuning parameter to adjust sensitivity of note detection
        float threshold = mean + tuner * stdDev;
        float framePos = 0f;

        // Iterate through the spectral flux frames and place note events at peaks that exceed the threshold
        while (framePos < flux.Length)
        {
            int frameIndex = Mathf.RoundToInt(framePos);
            if (frameIndex >= flux.Length) break;


            int searchRadius = Mathf.RoundToInt(framesPerBeat * 0.5f);
            int searchStart  = Mathf.Max(0, frameIndex - searchRadius);
            int searchEnd    = Mathf.Min(flux.Length - 1, frameIndex + searchRadius);

            float peakValue = 0f;
            int   peakFrame = frameIndex;
            for (int i = searchStart; i <= searchEnd; i++)//search for local peak in spectral flux around the expected beat position
            {
                if (flux[i] > peakValue)
                {
                    peakValue = flux[i];
                    peakFrame = i;
                }
            }

            // Only place a note if the peak clears the threshold
            if (peakValue >= threshold)
            {
                float timestamp = peakFrame / FPS; // convert frame → seconds
                Newbeatmap.beatEvents.Add(new NoteEvent { timestamp = timestamp });
            }

            framePos += framesPerBeat; // advance exactly one beat
        }
        return Newbeatmap;
    }
    void SaveBeatmap(Beatmap beatmap, string songFolder)//save beatmap to JSON file in corresponding song folder
    {
        string json = JsonUtility.ToJson(beatmap, prettyPrint: true);
        string path = Path.Combine(songFolder, beatmap.mapName + ".json");
        File.WriteAllText(path, json);
        Debug.Log("Beatmap saved to: " + path);
    }
}