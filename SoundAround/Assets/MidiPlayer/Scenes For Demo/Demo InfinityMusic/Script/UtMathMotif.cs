using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MidiPlayerTK;
using System.IO;
using System.Xml.Serialization;
using System;

namespace InfinityMusic
{
    public enum Mode
    {
        Random = 0,
        Up = 1,
        Down = 2,
        Fixed = 3,
        CircleUp = 4,
        CircleDown = 5
    }

    public class UtMathMotif : UtComponent
    {

        public bool IsEnabled = true;

        public UtCadence CurrentCadence = null;

        /// <summary>
        /// Length of the motif in mesure 
        /// </summary>
        [Range(1, 8)]
        public int MeasureCount = 2;

        [Range(0, 10)]
        public int OctaveMin = 5;

        [Range(0, 10)]
        public int OctaveMax = 6;

        [Range(0, 85)]
        public int ScaleIndex;

        [Range(0, 127)]
        public int PatchIndex;

        [Range(-48, 48)]
        public int Transpose;

        public bool DrumKit;

        public Mode SelectedAlgo;

        [Range(1, 10)]
        public int StepInScale;

        [Range(-500, 500)]
        public int RotationSpeed;

        [Range(0, 100)]
        public int RepeatRate;

        [Range(0, 100)]
        public int Accentuation;

        [Range(0, 127)]
        public int Velocity;

        [HideInInspector]
        public int CurrentIndexNotePlayed;
        public MathMotifNote LastNotePlayed = null;

        private const int MAX_NOTE_PER_MEASURE = 16;

        /// <summary>
        /// List of note, saved
        /// </summary>
        public List<MathMotifNote> Notes;

        /// <summary>
        /// Associate note + length, not saved
        /// </summary>
        public MathMotifNote[] Score;

        /// <summary>
        /// Call when default inspector value has changed
        /// </summary>
        void OnValidate()
        {
            //Debug.Log("OnValidate");
            if (Application.isPlaying)
            {
                //IniMotif();
                Generate(false);
            }
        }

        public override void DefaultValue()
        {
            Debug.Log("UtMathMotif : Load default value");
            MeasureCount = 2;
            OctaveMin = 5;
            OctaveMax = 6;
            ScaleIndex = 5;
            PatchIndex = 0;
            DrumKit = false;
            SelectedAlgo = Mode.Random;
            StepInScale = 1;
            RotationSpeed = 100;
            Accentuation = 20;
            Velocity = 100;
            RepeatRate = 0;
        }

        public override void Generate(bool genRandom = false)
        {
            Debug.Log("GenerateMotif Selected Algo:" + SelectedAlgo);
            if (!Application.isPlaying)
                return;

            if (ScaleDefinition.Scales == null)
                return;

            if (OctaveMin > OctaveMax) OctaveMax = OctaveMin;

            // Generate seed notes from the selected scale (octave start to octave end i.e. the ambitus)
            List<int> scaleNotes = new List<int>();
            int step = StepInScale < 1 ? 1 : StepInScale;
            for (int scale = OctaveMin; scale <= OctaveMax; scale++)
            {
                for (int delta = 0; delta < ScaleDefinition.Scales[ScaleIndex].Ecart.Length; delta += step)
                {
                    scaleNotes.Add(scale * 12 + ScaleDefinition.Scales[ScaleIndex].Ecart[delta]);
                }
            }
            if (scaleNotes.Count == 0)
            {
                Debug.LogWarning("No scale defined");
                return;
            }

            int noteIndex = 0;
            if (SelectedAlgo == Mode.Down)// || SelectedMode == Mode.CircleDown)
            {
                noteIndex = scaleNotes.Count - 1;
            }

            Notes = new List<MathMotifNote>();

            for (int measure = 0; measure < MeasureCount; measure++)
            {
                // Generate NotePerMeasure (16) sixteeth by measure. It's the maximum of note.
                for (int notepos = 0; notepos < MAX_NOTE_PER_MEASURE; notepos++)
                {

                    MathMotifNote note = new MathMotifNote
                    {
                        Note = -1
                    };

                    switch (SelectedAlgo)
                    {
                        case Mode.Random:
                            note.Note = RepeatLast();
                            if (note.Note < 0 && scaleNotes.Count > 0)
                                note.Note = scaleNotes[UnityEngine.Random.Range(0, scaleNotes.Count)];
                            break;

                        case Mode.Up:
                            note.Note = RepeatLast();
                            if (note.Note < 0)
                            {
                                if (noteIndex >= scaleNotes.Count)
                                    noteIndex = 0;
                                note.Note = scaleNotes[noteIndex];
                                noteIndex++;
                            }
                            break;

                        case Mode.Down:
                            note.Note = RepeatLast();
                            if (note.Note < 0)
                            {
                                if (noteIndex < 0)
                                    noteIndex = scaleNotes.Count - 1;
                                note.Note = scaleNotes[noteIndex];
                                noteIndex--;
                            }
                            break;

                        case Mode.Fixed:
                            note.Note = scaleNotes[0];
                            break;

                        case Mode.CircleDown:
                        case Mode.CircleUp:
                            {
                                float angle = Mathf.Lerp(0, Mathf.PI / 2f, ((float)noteIndex) / scaleNotes.Count);
                                float sin;
                                angle *= (RotationSpeed / 100f);
                                if (SelectedAlgo == Mode.CircleUp)
                                    // Entre 0 et 1;
                                    sin = Mathf.Sin(angle);
                                else
                                    // Entre 1 et 0;
                                    sin = Mathf.Cos(angle);

                                int iseed = Mathf.RoundToInt(Mathf.Lerp(0, scaleNotes.Count - 1, sin));
                                if (iseed < 0)
                                    iseed = 0;
                                else if (iseed >= scaleNotes.Count)
                                    iseed = scaleNotes.Count - 1;
                                note.Note = scaleNotes[iseed];
                                Debug.Log(noteIndex + " " + sin + " " + note.Note);
                                noteIndex++;
                            }
                            break;
                    }

                    // Add note with only gamme index + delta from first note in gamme (C)
                    // Only these information are saved, others are rebuild with buildscore
                    Notes.Add(note);
                }
            }
            LastCadenceApplied = DateTime.MinValue;
        }

        /// <summary>
        /// Ramdomly repeat last note with a rate defined with RepeatRate
        /// </summary>
        /// <returns></returns>
        private int RepeatLast()
        {
            int midi = -1;
            if (Notes.Count > 0 && RepeatRate > 0f)
                if (UnityEngine.Random.Range(0, 100) < RepeatRate)
                    midi = Notes[Notes.Count - 1].Note;
            return midi;
        }

        DateTime LastCadenceApplied = DateTime.MinValue;

        /// <summary>
        /// Add cadence to note and transpose
        /// </summary>
        public void BuildScore()
        {
            if (CurrentCadence != null)
            {
                if (CurrentCadence.LastCadenceGenerated < LastCadenceApplied)
                    // Cadence already applied
                    return;
            }
            else
                if (LastCadenceApplied > DateTime.MinValue)
                return;

            // Define in Score 16 notes per measure. Note in Score can be null
            Score = new MathMotifNote[MeasureCount * MAX_NOTE_PER_MEASURE];
            int posNoteInScore = 0;
            int posCadence = 0;

            // Assign each notes to the right position in the score
            foreach (MathMotifNote note in Notes)
            {
                if (posNoteInScore >= Score.Length)
                    break;
                note.CadenceDuration = new Cadence();

                // Calculate note midi
                //if (note.Note < 0)
                //    note.Note = note.GammeIndex * 12 + note.Ecart + (Transpose - 1);
                //else

                Score[posNoteInScore] = note;

                // If the cadence component don't exist
                if (CurrentCadence == null || CurrentCadence.Durations == null || CurrentCadence.Durations.Count == 0)
                {
                    // create a standard one, 4 Quarter per measure, 
                    Score[posNoteInScore].CadenceDuration.enDuration = Cadence.Duration.Quarter;
                    posNoteInScore += 4;
                }
                else
                {
                    if (posCadence >= CurrentCadence.Durations.Count)
                        posCadence = 0;
                    Score[posNoteInScore].CadenceDuration = CurrentCadence.Durations[posCadence];
                    posNoteInScore += Score[posNoteInScore].NbrOfSixteen();
                    posCadence++;
                }
            }

            LastCadenceApplied = DateTime.Now;
            //DebugScore();
        }

        void DebugScore()
        {
            int index = 0;
            foreach (MathMotifNote note in Score)
            {
                string info = string.Format("Index:{0,3:000}/{1,3:000} ", index, Score.Length - 1);
                if (note != null)
                {
                    info += string.Format(" Midi:{0,3} Velocity:{1,3} Duration:{2,10} ", note.Note, note.Velocity, note.CadenceDuration.enDuration);
                }
                else
                    info += " - ";
                index++;
                Debug.Log(info);
            }
        }

        /// <summary>
        /// Get note to played, call for every sixteenth but not always return a note
        /// </summary>
        /// <returns></returns>
        public MathMotifNote Calculate()
        {
            LastNotePlayed = null;

            // Add cadence to note
            if (Notes != null && Notes.Count > 0)
            {
                BuildScore();

                if (Score != null && Score.Length > 0)
                {
                    CurrentIndexNotePlayed = (InfinityMusic.instance.IndexMeasure % MeasureCount) * MAX_NOTE_PER_MEASURE + InfinityMusic.instance.IndexSixteenthMeasure;

                    while (CurrentIndexNotePlayed >= Score.Length)
                        CurrentIndexNotePlayed = CurrentIndexNotePlayed - Score.Length;
                    if (CurrentIndexNotePlayed < 0)
                        CurrentIndexNotePlayed = 0;

                    //Debug.Log("Score[" + ((Global.instance.IndexMeasure % MeasureCount) * NotePerMeasure + Global.instance.IndexSixteenthMeasure) + "] " + CurrentIndexNotePlayed);


                    //Debug.Log("last=" + last + " sendNote:" + sendNote);
                    if (Score[CurrentIndexNotePlayed] != null)
                    {
                        if (!Score[CurrentIndexNotePlayed].CadenceDuration.Silence)
                        {
                            //Debug.Log("CurrentIndexNotePlayed:" + CurrentIndexNotePlayed + "/" + Score.Length + " IndexMeasure:" + Global.instance.IndexMeasure % MeasureCount);
                            LastNotePlayed = new MathMotifNote()
                            {
                                Note = Score[CurrentIndexNotePlayed].Note + Transpose,
                                CadenceDuration = Score[CurrentIndexNotePlayed].CadenceDuration,
                                Duration = InfinityMusic.instance.SixteenthDurationMs * Score[CurrentIndexNotePlayed].NbrOfSixteen(),
                            };

                            LastNotePlayed.Velocity = Velocity;
                            if (InfinityMusic.instance.IndexSixteenthMeasure == 0)
                            {
                                // First note in measure, apply accentuation
                                LastNotePlayed.Velocity += (int)((float)Velocity * (float)Accentuation);
                                if (LastNotePlayed.Velocity > 127) LastNotePlayed.Velocity = 127;
                            }

                            if (!DrumKit)
                                LastNotePlayed.Patch = PatchIndex;
                            else
                                LastNotePlayed.Patch = 0;

                            LastNotePlayed.Drum = DrumKit;
                        }
                        //Debug.Log("Score[" + CurrentIndexNotePlayed + "] " + LastNotePlayed.Note + " " + LastNotePlayed.Duration + " " + LastNotePlayed.Velocity);
                    }
                    //else
                    //    Debug.Log("Score[" + CurrentIndexNotePlayed + "] null");
                }
            }
            return LastNotePlayed;
        }
    }
}