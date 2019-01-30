using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Xml.Serialization;
using System.IO;
using MidiPlayerTK;

namespace InfinityMusic
{
     public class UtCadence : UtComponent
    {
        /// <summary>
        /// Length of the motif in mesure 
        /// </summary>
        [Range(1, 8)]
        public int MeasureCount = 2;

        [Range(0, 100)]
        public int PctSilence = 0;
        [Range(0, 100)]
        public int RatioWhole;
        [Range(0, 100)]
        public int RatioHalf;
        [Range(0, 100)]
        public int RatioQuarter;
        [Range(0, 100)]
        public int RatioEighth;
        [Range(0, 100)]
        public int RatioSixteen;

        public List<Cadence> Durations;
        public DateTime LastCadenceGenerated = DateTime.MaxValue;

        public override void DefaultValue()
        {
            Debug.Log("UtCadence : Load default value");
            MeasureCount = 1;
            RatioHalf = 50;
            RatioEighth = 50;
        }

        public override void Generate(bool fake)
        {
            try
            {
                int indexCadence = 0;
                Durations = new List<Cadence>();
                Debug.Log("Generate Cadence");
                for (int measure = 0; measure < MeasureCount; measure++)
                {
                    int countSixteen = 16; // Always the max else : Global.instance.countSixteenthMeasure;

                    // Generate for the maximum : 4 measures
                    while (countSixteen > 0)
                    {
                        Cadence.Duration enDuration = Cadence.Duration.NotDefined;

                        float randDuration = UnityEngine.Random.Range(1f, 100f);

                        if (randDuration < RatioWhole) enDuration = Cadence.Duration.Whole;
                        if (randDuration < RatioHalf) enDuration = Cadence.Duration.Half;
                        if (randDuration < RatioQuarter) enDuration = Cadence.Duration.Quarter;
                        if (randDuration < RatioEighth) enDuration = Cadence.Duration.Eighth;
                        if (randDuration < RatioSixteen) enDuration = Cadence.Duration.Sixteenth;
                        if (enDuration == Cadence.Duration.NotDefined) enDuration = Cadence.Duration.Quarter;

                        countSixteen -= MathMotifNote.NbrOfSixteen(enDuration);

                        float randSilence = UnityEngine.Random.Range(1f, 100f);

                        Cadence cadence = new Cadence() { enDuration = enDuration, Silence = randSilence >= PctSilence ? false : true };
                        //Debug.Log("   cadence:" + indexCadence + " measure:" + measure + " countSixteen:" + countSixteen + " enDuration:" + cadence.enDuration + " Silence:" + cadence.Silence);
                        Durations.Add(cadence);
                        indexCadence++;
                    }
                }

                LastCadenceGenerated = DateTime.Now;
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
        }
    }
}