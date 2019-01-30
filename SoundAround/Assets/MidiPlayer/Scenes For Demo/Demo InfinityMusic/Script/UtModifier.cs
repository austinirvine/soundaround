using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Xml.Serialization;
using System.IO;
using MidiPlayerTK;

namespace InfinityMusic
{
    /// <summary>
    /// Useful to change on live some parameters of others components
    /// Work in progress
    /// </summary>
    public class UtModifier : UtComponent
    {
        /// <summary>
        /// Length of the motif in mesure 
        /// </summary>
        [Range(1, 8)]
        public int MeasureCount = 2;

        public List<UtComponent> Components;

        public override void DefaultValue()
        {
            Debug.Log("UtModifier : Load default value");
            MeasureCount = 1;
        }

        public override void Generate(bool fake)
        {
            try
            {

            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
        }
    }
}