
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
    /// Script for the prefab MidiStreamPlayer. 
    /// Play generated notes 
    /// </summary>
    public class MidiStreamPlayer : MidiPlayer
    {
        new void Awake()
        {
            base.Awake();
        }

        new void Start()
        {
            try
            {
                base.Start();
                DestroyAllAudioSource();
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
        }

        /// <summary>
        /// Play a list of notes with a thread (Coriutine). Return immediately.
        /// If you want be able to stop note, use rather the Methods Play and Stop of MPTKNote class
        /// </summary>
        public virtual void MPTK_Play(List<MPTKNote> notes)
        {
            try
            {
                if (MidiPlayerGlobal.SoundFontLoaded)
                {
                    StartCoroutine(TheadPlay(notes));
                }
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
        }

        private IEnumerator TheadPlay(List<MPTKNote> notes)
        {
            if (notes != null && notes.Count > 0)
            {
                try
                {
                    if (MPTK_PauseOnDistance)
                    {
                        distanceEditorModeOnly = MidiPlayerGlobal.MPTK_DistanceToListener(this.transform);
                        if (distanceEditorModeOnly > AudioSourceTemplate.maxDistance)
                        {
                            notes.Clear();
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    MidiPlayerGlobal.ErrorDetail(ex);
                }

                try
                {
                    List<MidiNote> midinotes = new List<MidiNote>();
                    foreach (MPTKNote note in notes)
                        midinotes.Add(note.ToMidiNote());
                    MPTK_PlayNotes(midinotes);
                    //Debug.Log("---------------- play count:" + notes.Count + " " + timeFromStartMS );
                }
                catch (System.Exception ex)
                {
                    MidiPlayerGlobal.ErrorDetail(ex);
                }
            }
            yield return 0;

        }
    }
}

