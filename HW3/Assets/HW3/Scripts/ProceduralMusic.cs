using UnityEngine;

/// <summary>
/// Fallback background music generated in code (used only if no AudioClip is
/// assigned to the GameManager's AudioSource). A short looping Am-F-C-G arpeggio.
/// </summary>
public static class ProceduralMusic
{
    static float Midi(int n) => 440f * Mathf.Pow(2f, (n - 69) / 12f);

    public static AudioClip Create()
    {
        const int rate = 44100;
        const float bpm = 96f;
        const int steps = 64;                       // eighth notes
        float stepDur = 60f / bpm / 2f;
        int stepSamples = (int)(stepDur * rate);
        int total = stepSamples * steps;
        var data = new float[total];

        int[] bass = { 45, 41, 48, 43 };            // A2 F2 C3 G2
        int[][] arps =
        {
            new[] { 57, 60, 64, 69 },               // Am
            new[] { 53, 57, 60, 65 },               // F
            new[] { 60, 64, 67, 72 },               // C
            new[] { 55, 59, 62, 67 },               // G
        };
        int[] pattern = { 0, 1, 2, 3, 2, 1, 2, 3 };
        const float twoPi = Mathf.PI * 2f;

        for (int s = 0; s < steps; s++)
        {
            int chord = (s / 16) % 4;
            float f = Midi(arps[chord][pattern[s % 8]]);
            int offset = s * stepSamples;

            for (int i = 0; i < stepSamples; i++)
            {
                float t = (float)i / rate;
                float env = Mathf.Exp(-t * 6f) * Mathf.Min(1f, t * 300f);
                float v = Mathf.Sin(twoPi * f * t) * 0.22f + Mathf.Sin(twoPi * 2f * f * t) * 0.05f;
                data[offset + i] += v * env;
            }

            if (s % 4 == 0)
            {
                float bf = Midi(bass[chord]);
                int len = stepSamples * 4;
                for (int i = 0; i < len && offset + i < total; i++)
                {
                    float t = (float)i / rate;
                    float env = Mathf.Exp(-t * 1.5f) * Mathf.Min(1f, t * 300f);
                    data[offset + i] += Mathf.Sin(twoPi * bf * t) * 0.3f * env;
                }
            }
        }

        for (int i = 0; i < total; i++) data[i] = Mathf.Clamp(data[i], -1f, 1f);

        var clip = AudioClip.Create("ProceduralMusic", total, 1, rate, false);
        clip.SetData(data, 0);
        return clip;
    }
}
