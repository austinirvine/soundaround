
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.IO;
using System;
using UnityEngine.Events;

namespace MidiPlayerTK
{
    /// <summary>
    /// Play a list of notes according the preset
    /// </summary>
    public class MidiPlayer : MonoBehaviour
    {
        /// <summary>
        /// Log information on selected wave for each notes
        /// </summary>
        public bool MPTK_LogWaves = false;

        /// <summary>
        /// Transpose note from -24 to 24
        /// </summary>
        public virtual int MPTK_Transpose
        {
            get { return transpose; }
            set
            {
                if (value >= -24 && value <= 24f)
                    transpose = value;
                else
                    Debug.LogWarning("MidiFilePlayer - Set Transpose value not valid : " + value);
            }
        }

        /// <summary>
        /// MaxDistance to use for PauseOnDistance
        /// </summary>
        public virtual float MPTK_MaxDistance
        {
            get
            {
                try
                {
                    return AudioSourceTemplate.maxDistance;
                }
                catch (System.Exception ex)
                {
                    MidiPlayerGlobal.ErrorDetail(ex);
                }
                return 0;
            }
            set
            {
                try
                {
                    AudioSourceTemplate.maxDistance = value;
                    if (audiosources == null) audiosources = new List<AudioSource>();
                    foreach (AudioSource audio in audiosources)
                        audio.maxDistance = value;
                }
                catch (System.Exception ex)
                {
                    MidiPlayerGlobal.ErrorDetail(ex);
                }
            }
        }

        /// <summary>
        /// Value updated only when playing in Unity (for inspector refresh)
        /// </summary>
        public float distanceEditorModeOnly;


        [SerializeField]
        [HideInInspector]
        public int transpose = 0;

        /// <summary>
        /// Should the Midi playing must be paused if distance between AudioListener and MidiFilePlayer is greater than MaxDistance
        /// </summary>
        public virtual bool MPTK_PauseOnDistance { get { return pauseOnDistance; } set { pauseOnDistance = value; } }

        /// <summary>
        /// Volume of midi playing. 
        /// Must be >=0 and <= 1
        /// </summary>
        public virtual float MPTK_Volume
        {
            get { return volume; }
            set
            {
                if (volume >= 0f && volume <= 1f)
                    volume = value;
                else
                    Debug.LogWarning("MidiFilePlayer - Set Volume value not valid : " + value);
            }
        }

        [SerializeField]
        [HideInInspector]
        private float volume = 0.5f;

        [SerializeField]
        [HideInInspector]
        private bool pauseOnDistance = false;


        /// <summary>
        /// Template for creating all AudioSource
        /// </summary>
        public AudioSource AudioSourceTemplate;

        static protected float _ratioHalfTone = 1.0594630943592952645618252949463f;
        /*protected*/
        public List<AudioSource> audiosources;

        private float timeToRelease = 0.1f;
        /// <summary>
        /// Time in second to stop a sound after the Duration time. No effect on sound with no loop as Drum. From 0.05f to 1f.
        /// </summary>
        public virtual float MPTK_TimeToRelease
        {
            get { return timeToRelease; }
            set
            {
                if (value >= 0.05f && value <= 1f)
                    timeToRelease = value;
                else
                    Debug.LogWarning("MidiFilePlayer - TimeToRelease value not valid : " + value + " must in the range 0.05 to 1");
            }
        }

        protected virtual void Awake()
        {
            try
            {
                HelperNoteLabel.Init();
                //MidiPlayerGlobal.Init();
                audiosources = new List<AudioSource>();
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
        }

        protected virtual void Start()
        {
            try
            {
                AudioSourceTemplate = GetComponentInChildren<AudioSource>();
                if (AudioSourceTemplate == null)
                    Debug.LogWarning("No AudioSource found.");
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
        }

        /// <summary>
        /// Clear all sounds
        /// </summary>
        public IEnumerator MPTK_ClearAllSound(bool destroyAudioSource = false, float p_timeToRelease = -1f)
        {
            float f_timeToRelease = p_timeToRelease < 0f ? timeToRelease : p_timeToRelease;
            if (f_timeToRelease > 1f) f_timeToRelease = 1f;

            if (AudioSourceTemplate.isPlaying)
                yield return StartCoroutine(Release(AudioSourceTemplate, f_timeToRelease));

            yield return StartCoroutine(ReleaseAll(f_timeToRelease));

            if (destroyAudioSource)
            {
                yield return StartCoroutine(WaitAllStop());
                yield return StartCoroutine(DestroyAllAudioSource());
            }
            yield return 0;
        }

        /// <summary>
        /// Cut the sound gradually
        /// </summary>
        /// <returns></returns>
        private IEnumerator ReleaseAll(float p_timeToRelease)
        {
            for (int i = 0; i < audiosources.Count; i++)
            {
                AudioSource audio = audiosources[i];
                if (audio.isPlaying)
                    yield return StartCoroutine(Release(audio, p_timeToRelease));
            }
        }

        /// <summary>
        /// Wait all audio source not playing with time out of 2 seconds
        /// </summary>
        /// <returns></returns>
        private IEnumerator WaitAllStop()
        {
            int countplaying = 999999;
            DateTime timeout = DateTime.Now + TimeSpan.FromSeconds(2);
            while (countplaying > 0 && timeout > DateTime.Now)
            {
                countplaying = 0;
                foreach (AudioSource audio in audiosources)
                    if (audio.isPlaying)
                    {
                        countplaying++;
                        audio.Stop();
                    }
                //Debug.Log("   " + countplaying + " ");
            }
            yield return 0;
        }

        /// <summary>
        /// Remove AudioSource not playing
        /// </summary>
        protected IEnumerator DestroyAllAudioSource()
        {
            try
            {
                AudioSource[] audios = GetComponentsInChildren<AudioSource>();

                if (audios != null)
                {
                    //int i = 0;
                    foreach (AudioSource audio in audios)
                        try
                        {
                            //Debug.Log("Destroy " + i++ + " " + audio.name + " " + (audio.clip != null ? audio.clip.name : "no clip"));
                            // Don't delete audio source template, the only one without a clip
                            if (audio.clip != null)
                                Destroy(audio.gameObject);
                        }
                        catch (System.Exception ex)
                        {
                            MidiPlayerGlobal.ErrorDetail(ex);
                        }
                }
                audiosources = new List<AudioSource>();
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
            yield return 0;

        }

        /// <summary>
        /// Play a list of notes
        /// </summary>
        /// <param name="notes"></param>
        public void MPTK_PlayNotes(List<MidiNote> notes)
        {
            if (MidiPlayerGlobal.SoundFontLoaded == false)
                return;

            if (notes != null)
            {
                foreach (MidiNote note in notes)
                {
                    if (note.Velocity != 0)
                        MPTK_PlayNote(note);
                }
            }
        }

        /// <summary>
        /// Play one note - [New 1.7]
        /// </summary>
        /// <param name="note"></param>
        public void MPTK_PlayNote(MidiNote note)
        {
            try
            {
                // Search sample associated to the preset, midi note and velocity
                int selectedBank = note.Drum ? MidiPlayerGlobal.CurrentMidiSet.ActiveSounFontInfo.DrumKitBankNumber : selectedBank = MidiPlayerGlobal.CurrentMidiSet.ActiveSounFontInfo.DefaultBankNumber;
                int noteMidi = note.Midi;
                if (!note.Drum)
                    noteMidi += MPTK_Transpose;

                //ImSample smpl = MidiPlayerGlobal.GetImSample(selectedBank, note.Patch, noteMidi, note.Velocity);
                //if (smpl != null)
                {
                    List<ImSample> samples = MidiPlayerGlobal.GetImMultiSample(selectedBank, note.Patch, noteMidi, note.Velocity);
                    //LogInfoSample(note, null, " Found " + samples.Count + " samples");
                    foreach (ImSample smpl in samples)
                    {
                        note.Pitch = Mathf.Pow(_ratioHalfTone, (float)(noteMidi - smpl.OriginalPitch + smpl.CoarseTune) + (float)smpl.FineTune / 100f);
                        // Load wave from audioclip
                        AudioClip clip = DicAudioClip.Get(smpl.WaveFile);
                        if (clip != null && clip.loadState == AudioDataLoadState.Loaded)
                        {
                            if (MPTK_LogWaves)
                                LogInfoSample(note, smpl);

                            AudioSource audioSelected = null;

                            // Search audioclip not playing with the same wave
                            try
                            {
                                foreach (AudioSource audio in audiosources)
                                {
                                    //Debug.Log(audio.isPlaying + " " + audio.clip.name + " " + clip.name);
                                    if (!audio.isPlaying && audio.clip.name == clip.name)
                                    {
                                        audioSelected = audio;
                                        break;
                                    }
                                }
                            }
                            catch (System.Exception ex)
                            {
                                MidiPlayerGlobal.ErrorDetail(ex);
                            }

                            if (audioSelected == null)
                            {
                                // No audiosource available, create a new audiosource
                                audioSelected = Instantiate<AudioSource>(AudioSourceTemplate);
                                audioSelected.Stop();
                                audioSelected.transform.position = AudioSourceTemplate.transform.position;
                                audioSelected.transform.SetParent(this.transform);
                                audiosources.Add(audioSelected);
                                // Assign sound to audioclip
                                audioSelected.clip = clip;
                            }

                            // Play note
                            StartCoroutine(PlayNote(audioSelected, note.Drum, smpl, note, timeToRelease));

                        }
                        else
                        {
                            if (MPTK_LogWaves)
                                LogInfoSample(note, null, smpl.WaveFile + "         ******** Clip not ready to play or not found ******");
                        }
                    }
                    //else
                    if (samples.Count == 0)
                    {
                        if (MPTK_LogWaves)
                            LogInfoSample(note, null, "               ********* Sample not found *********");
                    }
                }
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
        }

        /// <summary>
        /// Play one note with this AudioSource
        /// </summary>
        /// <param name="audio">AudioSource</param>
        /// <param name="loop">Sound with loop</param>
        /// <param name="note"></param>
        /// <returns></returns>
        protected IEnumerator PlayNote(AudioSource audio, bool drum, ImSample sample, MidiNote note, float vTransitionTime)
        {
            if (note.Delay > 0f)
            {
                float endDelay = Time.realtimeSinceStartup + note.Delay / 1000f;
                while (Time.realtimeSinceStartup < endDelay && note.Delay > 0f)
                    yield return new WaitForSeconds(0.01f);
            }

            try
            {
                audio.pitch = note.Pitch;
                if (drum)
                    audio.loop = false;
                else
                    audio.loop = sample.IsLoop;
                // Attenuation removed from current version
                //audio.volume = Mathf.Lerp(0f, 1f, note.Velocity * sample.Attenuation / 127f) * MPTK_Volume;
                audio.volume = Mathf.Lerp(0f, 1f, note.Velocity / 127f) * MPTK_Volume;

                // Pan from the SoundFont
                if (MidiPlayerGlobal.CurrentMidiSet.ActiveSounFontInfo.Panoramic)
                    audio.panStereo = Mathf.Lerp(-1f, 1f, (sample.Pan + 500) / 1000f);
                else
                    audio.panStereo = 0f;

                // Pan from the midi file or from a midi generated event
                if (note.Pan >= 0)
                {
                    audio.panStereo = Mathf.Lerp(-1f, 1f, note.Pan / 127f);
                }
                //Debug.Log(string.Format("   Pan  - note:{0,3} sample:{1,3} --> audio pan:{2,3}" , note.Pan , sample.Pan,Math.Round( audio.panStereo,2)));
                //Debug.Log(string.Format("   Vel  - note:{0,3} sample:{1,3} --> audio vel:{2,3}", note.Velocity , Math.Round(sample.Attenuation, 2), Math.Round(audio.volume, 2)));
                //Debug.Log(string.Format("   Loop - drum:{0} sample:{1}     --> audio loop:{2}" , drum , sample.IsLoop , audio.loop));

                audio.Play();
            }
            catch (Exception)
            {
            }

            // Attack & Decay taken in account by the wave, for drum (loop=false) play the wave one shot (no duration)
            if (audio.loop)
            {
                // Sustain phase until key release, constant amplitude
                float end = Time.realtimeSinceStartup + (float)(note.Duration / 1000d);
                while (note.Duration > 0f)
                {
                    try
                    {
                        if (Time.realtimeSinceStartup >= end || !audio.isPlaying)
                            break;
                    }
                    catch (Exception)
                    {
                    }
                    yield return new WaitForSeconds(0.01f);
                }
                //Debug.Log("stop " + sample.WaveFile + " " + note.Duration);
                // Release phase
                if (vTransitionTime > 0f)
                {
                    float dtime = 0f;
                    float volume = 0;

                    try
                    {
                        volume = audio.volume;
                        end = Time.realtimeSinceStartup + vTransitionTime;
                    }
                    catch (Exception)
                    {
                    }

                    do
                    {
                        dtime = end - Time.realtimeSinceStartup;
                        try
                        {
                            audio.volume = Mathf.Lerp(0f, volume, dtime / vTransitionTime);
                            if (dtime < 0f || !audio.isPlaying)
                                break;

                        }
                        catch (Exception)
                        {
                            break;
                        }
                        yield return new WaitForSeconds(0.01f);
                    }
                    while (true);
                }

                try
                {
                    audio.Stop();
                }
                catch (Exception)
                {
                }
            }
            //else
            //{
            //    // play with no loop (drum)
            //    while (audio.isPlaying)
            //    {
            //        yield return new WaitForSeconds(0.01f);
            //    }
            //}
        }

        /// <summary>
        /// Release the sound
        /// </summary>
        /// <param name="audio"></param>
        /// <param name="delay"></param>
        /// <returns></returns>
        protected IEnumerator Release(AudioSource audio, float vTransitionTime)
        {
            float dtime = 0f;
            float volume = 0f;
            float end = 0f;
            //Debug.Log(vTransitionTime);
            if (vTransitionTime > 0f)
            {
                try
                {
                    dtime = 0f;
                    volume = audio.volume;
                    end = Time.realtimeSinceStartup + vTransitionTime;
                }
                catch (Exception)
                {
                }

                do
                {
                    dtime = end - Time.realtimeSinceStartup;
                    try
                    {
                        audio.volume = Mathf.Lerp(0f, volume, dtime / vTransitionTime);
                        if (dtime < 0f || !audio.isPlaying)
                            break;
                    }
                    catch (Exception)
                    {
                        break;
                    }

                    yield return new WaitForSeconds(0.01f);
                }
                while (true);
            }

            try
            {
                //Debug.Log("Stop " + audio.clip.name);
                audio.Stop();
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
            }

        }

        private void LogInfoSample(MidiNote note, ImSample sample, string info = null)
        {
            //TimeSpan playtime = TimeSpan.Zero;
            //if (miditoplay != null) playtime = miditoplay.CurrentMidiTime();

            //string time = string.Format("[{0:00}:{1:00}:{2:00}:{3:000}]", playtime.Hours, playtime.Minutes, playtime.Seconds, playtime.Milliseconds);
            string time = "";
            if (sample != null)
#if DEBUGPITCHNOTE
                Debug.Log(string.Format("{0} C:{1,2} P:{2,3:000} D:{3} Note:{4,3:000} OriginalPitch:{5} PitchCorrection:{6} FineTune:{7} CoarseTune:{8} Duration:{9,4} sec. Velocity:{10} Wave:{11}",
                    time, note.Chanel, note.Patch, note.Drum, note.Midi,
                    sample.OriginalPitch, sample.PitchCorrection, sample.FineTune, sample.CoarseTune, Math.Round(note.Duration / 1000d, 2), note.Velocity, sample.WaveFile));
#else
                Debug.Log(string.Format("{0} C:{1,2} P:{2,3:000} D:{3} Note:{4,3:000} {5,-3} Duration:{6,4} sec. Velocity:{7} Pan:{8} Wave:{9}",
                    time, note.Channel, note.Patch, note.Drum, note.Midi, HelperNoteLabel.LabelFromMidi(note.Midi),
                    Math.Round(note.Duration / 1000d, 2), note.Velocity, sample.Pan, sample.WaveFile));
#endif
            else
                Debug.Log(string.Format("{0} C:{1,2} P:{2,3:000} D:{3} Note:{4,3:000} {5,-5} Duration:{6,4} sec. Velocity:{7} {8}",
                    time, note.Channel, note.Patch, note.Drum, note.Midi, HelperNoteLabel.LabelFromMidi(note.Midi), Math.Round(note.Duration / 1000d, 2), note.Velocity, info));

        }
    }
}

