using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MidiPlayerTK;
using System.IO;
using System.Xml.Serialization;
using System;

namespace InfinityMusic
{
    public class MathMotifNote : MPTKNote
    {
        public Cadence CadenceDuration;

        public int NbrOfSixteen()
        {
            return NbrOfSixteen(CadenceDuration.enDuration);
        }

        static public int NbrOfSixteen(Cadence.Duration enumDuration)
        {
            int countSixteen = 0;
            switch (enumDuration)
            {
                case Cadence.Duration.Sixteenth: countSixteen = 1; break;
                case Cadence.Duration.Eighth: countSixteen = 2; break;
                case Cadence.Duration.Quarter: countSixteen = 4; break;
                case Cadence.Duration.Half: countSixteen = 8; break;
                case Cadence.Duration.Whole: countSixteen = 16; break;
            }
            return countSixteen;
        }
    }
}