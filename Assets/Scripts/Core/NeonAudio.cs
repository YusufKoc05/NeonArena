using UnityEngine;

namespace NeonArena
{
    /// <summary>
    /// Synthesizes crisp retro-cyberpunk sound effects procedurally at runtime
    /// (zero external audio files needed).
    /// </summary>
    public class NeonAudio : MonoBehaviour
    {
        public static NeonAudio Instance { get; private set; }

        private AudioSource _sfx;
        private AudioSource _bgm;
        private AudioClip _synthwaveLoop;
        private AudioClip _laserClip;
        private AudioClip _hitClip;
        private AudioClip _explosionClip;
        private AudioClip _xpClip;
        private AudioClip _levelUpClip;
        private AudioClip _dashClip;
        private AudioClip _bossWarningClip;
        private AudioClip _bossDefeatClip;

        private const string MusicPrefKey = "NeonArena_MusicOn";
        private const string SfxPrefKey = "NeonArena_SfxOn";

        public bool IsMusicEnabled
        {
            get => PlayerPrefs.GetInt(MusicPrefKey, 1) == 1;
            set
            {
                PlayerPrefs.SetInt(MusicPrefKey, value ? 1 : 0);
                PlayerPrefs.Save();
                if (_bgm != null) _bgm.mute = !value;
            }
        }

        public bool IsSfxEnabled
        {
            get => PlayerPrefs.GetInt(SfxPrefKey, 1) == 1;
            set
            {
                PlayerPrefs.SetInt(SfxPrefKey, value ? 1 : 0);
                PlayerPrefs.Save();
                if (_sfx != null) _sfx.mute = !value;
            }
        }

        private float _lastLaserTime;
        private float _lastXpTime;
        private float _lastHitTime;

        public static NeonAudio Create()
        {
            var go = new GameObject("NeonAudio");
            var audio = go.AddComponent<NeonAudio>();
            return audio;
        }

        private void Awake()
        {
            Instance = this;
            _sfx = gameObject.AddComponent<AudioSource>();
            _sfx.playOnAwake = false;
            _sfx.spatialBlend = 0f;
            _sfx.mute = !IsSfxEnabled;

            _bgm = gameObject.AddComponent<AudioSource>();
            _bgm.playOnAwake = false;
            _bgm.loop = true;
            _bgm.spatialBlend = 0f;
            _bgm.volume = 0.34f;
            _bgm.mute = !IsMusicEnabled;

            _laserClip = BuildSweepClip("Laser", 0.085f, 1150f, 320f, 0.22f, true);
            _hitClip = BuildNoisePopClip("Hit", 0.05f, 520f, 0.25f);
            _explosionClip = BuildNoisePopClip("Explosion", 0.22f, 140f, 0.42f);
            _xpClip = BuildSweepClip("Xp", 0.09f, 760f, 1320f, 0.20f, false);
            _dashClip = BuildSweepClip("Dash", 0.14f, 260f, 920f, 0.30f, false);
            _levelUpClip = BuildChordClip("LevelUp", 0.42f, new[] { 523.25f, 659.25f, 783.99f, 1046.5f }, 0.30f);
            _bossWarningClip = BuildSirenClip("BossWarn", 0.75f, 220f, 440f, 0.38f);
            _bossDefeatClip = BuildChordClip("BossDefeat", 0.65f, new[] { 392f, 493.88f, 587.33f, 783.99f }, 0.40f);

            _synthwaveLoop = BuildSynthwaveLoop();
            _bgm.clip = _synthwaveLoop;
            _bgm.Play();
        }

        /// <summary>
        /// Generates a rich 8-bar 122 BPM Cyberpunk Synthwave / Outrun track (Am - F - C - G - Am - F - Dm - E)
        /// with warm detuned synth pads, rolling filtered sub-bass, catchy lead melody with echo,
        /// punchy kick, gated snare, and crisp 16th hi-hats.
        /// </summary>
        private static AudioClip BuildSynthwaveLoop()
        {
            const int sr = 32000;
            const float bpm = 122f;
            const float beatDur = 60f / bpm;
            const int totalBeats = 32; // 8 full bars (~15.7s loop)
            float totalDur = beatDur * totalBeats;
            int samples = Mathf.RoundToInt(sr * totalDur);
            float[] data = new float[samples];

            // 8-bar dark cyberpunk progression: Am - F - C - G - Am - F - Dm - E
            float[] bassRoots = { 55.00f, 43.65f, 65.41f, 49.00f, 55.00f, 43.65f, 73.42f, 82.41f };
            float[][] padChords =
            {
                new[] { 220.00f, 261.63f, 329.63f }, // Am
                new[] { 174.61f, 220.00f, 261.63f }, // F
                new[] { 261.63f, 329.63f, 392.00f }, // C
                new[] { 196.00f, 246.94f, 293.66f }, // G
                new[] { 220.00f, 261.63f, 329.63f }, // Am
                new[] { 174.61f, 220.00f, 261.63f }, // F
                new[] { 293.66f, 349.23f, 440.00f }, // Dm
                new[] { 164.81f, 207.65f, 246.94f }  // E (dramatic tension turnaround!)
            };

            // 32-step (1/4-beat / eighth-note feel) lead melody across 8 bars (0 = rest)
            float[] melody =
            {
                // Bar 1 (Am)       Bar 2 (F)
                440f, 0f, 523.25f, 659.25f,   587.33f, 523.25f, 440f, 349.23f,
                // Bar 3 (C)        Bar 4 (G)
                523.25f, 0f, 659.25f, 783.99f, 783.99f, 659.25f, 587.33f, 0f,
                // Bar 5 (Am)       Bar 6 (F)
                880f, 0f, 783.99f, 659.25f,   523.25f, 0f, 440f, 523.25f,
                // Bar 7 (Dm)       Bar 8 (E)
                587.33f, 659.25f, 587.33f, 523.25f, 493.88f, 0f, 415.30f, 493.88f
            };

            var rng = new System.Random(2147);
            float bassPhase = 0f;
            float bassFilter = 0f;
            float leadPhase = 0f;
            float[] padPhases = new float[6];
            int delaySamples = Mathf.RoundToInt(sr * (beatDur * 0.75f)); // Dotted-eighth synthwave echo!
            float[] leadBuf = new float[samples];

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / sr;
                float beatPos = t / beatDur;
                int barIdx = Mathf.Clamp((int)(beatPos / 4f), 0, 7);
                int beatInBar = (int)beatPos % 4;
                float beatFrac = beatPos - Mathf.Floor(beatPos);

                // Sidechain ducking curve on every beat (authentic synthwave pump!)
                float sidechain = 0.35f + 0.65f * Mathf.Clamp01(beatFrac * 3.2f);

                // 1. Punchy Analog Kick
                float kickEnv = Mathf.Exp(-beatFrac * 13f);
                float kickFreq = 145f * Mathf.Exp(-beatFrac * 22f) + 44f;
                float kick = Mathf.Sin(2f * Mathf.PI * kickFreq * (beatFrac * beatDur)) * kickEnv * 0.42f;

                // 2. Crisp 80s Gated Snare on beats 2 & 4
                float snare = 0f;
                if (beatInBar == 1 || beatInBar == 3)
                {
                    float sEnv = Mathf.Exp(-beatFrac * 10f);
                    float n = (float)(rng.NextDouble() * 2.0 - 1.0);
                    snare = (n * 0.68f + Mathf.Sin(2f * Mathf.PI * 185f * t) * 0.32f) * sEnv * 0.24f;
                }

                // 3. Crisp 16th-note Hi-Hats (off-beat accent)
                int sixteenth = (int)(beatPos * 4f);
                float sixFrac = (beatPos * 4f) - Mathf.Floor(beatPos * 4f);
                float hatAccent = (sixteenth % 2 == 1) ? 0.085f : 0.035f;
                float hatNoise = (float)(rng.NextDouble() * 2.0 - 1.0);
                float hihat = hatNoise * Mathf.Exp(-sixFrac * 26f) * hatAccent;

                // 4. Rolling Filtered Outrun Bass (16th notes + deep sub)
                float bassFreq = bassRoots[barIdx];
                bassPhase += bassFreq / sr;
                if (bassPhase > 1f) bassPhase -= 1f;
                float rawSaw = (bassPhase * 2f - 1f) + 0.6f * Mathf.Sin(2f * Mathf.PI * bassPhase);
                // Simple warm low-pass filter
                float cutoff = 0.14f + 0.18f * Mathf.Exp(-sixFrac * 6f);
                bassFilter += (rawSaw - bassFilter) * cutoff;
                float bassEnv = 0.45f + 0.55f * Mathf.Exp(-sixFrac * 4.5f);
                float bass = bassFilter * bassEnv * sidechain * 0.30f;

                // 5. Lush Detuned Supersaw Pad (3 notes x 2 detuned oscillators)
                float padSum = 0f;
                var chord = padChords[barIdx];
                for (int n = 0; n < 3; n++)
                {
                    float fA = chord[n] * 0.996f;
                    float fB = chord[n] * 1.004f;
                    padPhases[n * 2] += 2f * Mathf.PI * fA / sr;
                    padPhases[n * 2 + 1] += 2f * Mathf.PI * fB / sr;
                    padSum += Mathf.Sin(padPhases[n * 2]) + Mathf.Sin(padPhases[n * 2 + 1]);
                }
                float pad = (padSum / 6f) * sidechain * 0.14f;

                // 6. Expressive Lead Synth Melody (with subtle vibrato + dotted-8th echo)
                int melIdx = Mathf.Clamp((int)(beatPos * 1f), 0, melody.Length - 1);
                float melFrac = beatPos - Mathf.Floor(beatPos);
                float melFreq = melody[melIdx];
                float lead = 0f;
                if (melFreq > 0f)
                {
                    float vib = 1f + 0.004f * Mathf.Sin(2f * Mathf.PI * 5.5f * t);
                    leadPhase += 2f * Mathf.PI * (melFreq * vib) / sr;
                    float lEnv = Mathf.Min(1f, melFrac * 18f) * Mathf.Exp(-melFrac * 2.4f);
                    lead = (Mathf.Sin(leadPhase) * 0.65f + Mathf.Sin(leadPhase * 2f) * 0.25f + Mathf.Sin(leadPhase * 3f) * 0.1f) * lEnv * 0.18f;
                }
                leadBuf[i] = lead;
                float echo = (i >= delaySamples) ? leadBuf[i - delaySamples] * 0.42f : 0f;

                // Master mix with warm analog soft-clipping (tanh)
                float mix = kick + snare + hihat + bass + pad + lead + echo;
                data[i] = (float)System.Math.Tanh(mix * 1.15f) * 0.88f;
            }

            var clip = AudioClip.Create("NeonSynthwaveBGM", samples, 1, sr, false);
            clip.SetData(data, 0);
            return clip;
        }

        public void PlayLaser()
        {
            if (Time.unscaledTime - _lastLaserTime < 0.055f) return;
            _lastLaserTime = Time.unscaledTime;
            _sfx.pitch = Random.Range(0.94f, 1.08f);
            _sfx.PlayOneShot(_laserClip, 0.55f);
        }

        public void PlayHit()
        {
            if (Time.unscaledTime - _lastHitTime < 0.045f) return;
            _lastHitTime = Time.unscaledTime;
            _sfx.pitch = Random.Range(0.9f, 1.15f);
            _sfx.PlayOneShot(_hitClip, 0.5f);
        }

        public void PlayExplosion(float volume = 0.7f)
        {
            _sfx.pitch = Random.Range(0.85f, 1.1f);
            _sfx.PlayOneShot(_explosionClip, volume);
        }

        public void PlayXp()
        {
            if (Time.unscaledTime - _lastXpTime < 0.05f) return;
            _lastXpTime = Time.unscaledTime;
            _sfx.pitch = Random.Range(0.96f, 1.14f);
            _sfx.PlayOneShot(_xpClip, 0.45f);
        }

        public void PlayDash()
        {
            _sfx.pitch = 1f;
            _sfx.PlayOneShot(_dashClip, 0.75f);
        }

        public void PlayLevelUp()
        {
            _sfx.pitch = 1f;
            _sfx.PlayOneShot(_levelUpClip, 0.85f);
        }

        public void PlayBossWarning()
        {
            _sfx.pitch = 1f;
            _sfx.PlayOneShot(_bossWarningClip, 0.9f);
        }

        public void PlayBossDefeat()
        {
            _sfx.pitch = 1f;
            _sfx.PlayOneShot(_bossDefeatClip, 0.95f);
        }

        private static AudioClip BuildSweepClip(string name, float duration, float startHz, float endHz, float vol, bool square)
        {
            const int sr = 44100;
            int samples = Mathf.Max(1, Mathf.RoundToInt(sr * duration));
            float[] data = new float[samples];
            float phase = 0f;

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / samples;
                float freq = Mathf.Lerp(startHz, endHz, t);
                phase += 2f * Mathf.PI * freq / sr;
                float wave = square ? Mathf.Sign(Mathf.Sin(phase)) * 0.6f + Mathf.Sin(phase) * 0.4f : Mathf.Sin(phase);
                float env = (1f - t) * (1f - t);
                data[i] = wave * env * vol;
            }

            var clip = AudioClip.Create(name, samples, 1, sr, false);
            clip.SetData(data, 0);
            return clip;
        }

        private static AudioClip BuildNoisePopClip(string name, float duration, float toneHz, float vol)
        {
            const int sr = 44100;
            int samples = Mathf.Max(1, Mathf.RoundToInt(sr * duration));
            float[] data = new float[samples];
            var rng = new System.Random(name.GetHashCode());
            float phase = 0f;

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / samples;
                float freq = toneHz * (1f - t * 0.65f);
                phase += 2f * Mathf.PI * freq / sr;
                float noise = (float)(rng.NextDouble() * 2.0 - 1.0);
                float env = Mathf.Pow(1f - t, 1.8f);
                data[i] = (noise * 0.55f + Mathf.Sin(phase) * 0.45f) * env * vol;
            }

            var clip = AudioClip.Create(name, samples, 1, sr, false);
            clip.SetData(data, 0);
            return clip;
        }

        private static AudioClip BuildChordClip(string name, float duration, float[] freqs, float vol)
        {
            const int sr = 44100;
            int samples = Mathf.Max(1, Mathf.RoundToInt(sr * duration));
            float[] data = new float[samples];

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / samples;
                float sum = 0f;
                for (int f = 0; f < freqs.Length; f++)
                {
                    float startOffset = f * 0.08f;
                    if (t < startOffset) continue;
                    float localT = (t - startOffset) / (1f - startOffset);
                    sum += Mathf.Sin(2f * Mathf.PI * freqs[f] * i / sr) * (1f - localT * 0.7f);
                }
                float env = Mathf.Sin(Mathf.Min(1f, t * 8f) * Mathf.PI * 0.5f) * (1f - t);
                data[i] = (sum / freqs.Length) * env * vol;
            }

            var clip = AudioClip.Create(name, samples, 1, sr, false);
            clip.SetData(data, 0);
            return clip;
        }

        private static AudioClip BuildSirenClip(string name, float duration, float lowHz, float highHz, float vol)
        {
            const int sr = 44100;
            int samples = Mathf.Max(1, Mathf.RoundToInt(sr * duration));
            float[] data = new float[samples];
            float phase = 0f;

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / samples;
                float lfo = 0.5f + 0.5f * Mathf.Sin(t * Mathf.PI * 6f);
                float freq = Mathf.Lerp(lowHz, highHz, lfo);
                phase += 2f * Mathf.PI * freq / sr;
                float env = Mathf.Sin(t * Mathf.PI);
                data[i] = (Mathf.Sin(phase) * 0.7f + Mathf.Sign(Mathf.Sin(phase)) * 0.3f) * env * vol;
            }

            var clip = AudioClip.Create(name, samples, 1, sr, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
