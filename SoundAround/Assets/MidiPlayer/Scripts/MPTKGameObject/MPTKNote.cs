using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace MidiPlayerTK
{

    public class MPTKNote
    {
        /// <summary>
        /// Note between 0 and 127
        /// </summary>
        public int Note;

        /// <summary>
        /// Velocity between 0 and 127
        /// </summary>
        public int Velocity;

        /// <summary>
        /// Midi info : pan left right
        /// </summary>
        public int Pan=-1;

        /// <summary>
        /// Duration of the note in millisecond
        /// </summary>
        public double Duration;

        /// <summary>
        /// Delay before playing the note in millisecond
        /// Used to short delay inferior to 1 second. From 0 to 1000
        /// </summary>
        public float Delay;

        /// <summary>
        /// Patch (preset) to play with this note. Between 0 and 127.   
        /// </summary>
        public int Patch;

        /// <summary>
        /// True if not is a drum hit
        /// </summary>
        public bool Drum;

        private MidiNote midinote;

        /// <summary>
        /// Convert a MPTKNote to a Midi Note - [New 1.7]
        /// </summary>
        /// <returns></returns>
        public MidiNote ToMidiNote()
        {
            midinote = new MidiNote()
            {
                Midi = Note <= 0 ? 0 : (Note > 127 ? 127 : Note),
                Delay = Delay <= 0 ? 0f : (Delay > 1000f ? 1000f : Delay),
                Patch = Drum ? 0 : Patch <= 0 ? 0 : (Patch > 127 ? 127 : Patch),
                Drum = Drum,
                Pan = Pan,
                Duration = Duration <= 0 ? 200f : (Duration > 100000f ? 200f : Duration),
                Velocity = Velocity,
            };
            return midinote;
        }

        /// <summary>
        /// Play a note which is stoppable. A release delay (typically 0.1 second) will be apply before the stop of the sound. No effect on Drum Kit sound.  - [New 1.7]
        /// </summary>
        /// <param name="streamPlayer">A MidiStreamPlayer component</param>
        public void Play(MidiStreamPlayer streamPlayer)
        {
            streamPlayer.MPTK_PlayNote(ToMidiNote());
        }

        /// <summary>
        /// Stop the note. A release delay (typically 0.1 second) will be apply before the stop of the sound. No effect on Drum Kit sound.  - [New 1.7]
        /// </summary>
        public void Stop()
        {
            if (midinote != null)
            {
                midinote.Delay = 0f;
                midinote.Duration = 0f;
            }
        }
    }
}
