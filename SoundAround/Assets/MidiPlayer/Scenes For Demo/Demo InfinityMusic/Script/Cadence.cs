using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace InfinityMusic
{
    public class Cadence
    {
        public Cadence()
        {
            Silence = false;
            enDuration = Duration.Quarter;
        }

        public bool Silence;
        public Duration enDuration;
        public enum Duration
        {
            /// <summary>
            /// ronde = 2 blanche ou = 4 noire
            /// </summary>
            Whole = 0,

            /// <summary>
            /// blanche = 2 noire
            /// </summary>
            Half = 1,

            /// <summary>
            /// Noire = 2 Eigth
            /// </summary>
            Quarter = 2,

            /// <summary>
            /// Croche = 2 double croche
            /// </summary>
            Eighth = 3,

            /// <summary>
            /// Double Croche, 4 Double Croche = 1 noire
            /// </summary>
            Sixteenth = 4,

            /// <summary>
            /// Double Croche, 4 Double Croche = 1 noire
            /// </summary>
            NotDefined = 5
        }
    }
}