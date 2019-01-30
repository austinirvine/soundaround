
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace InfinityMusic
{
    abstract public class UtComponent : MonoBehaviour
    {
        public enum Component
        {
            Audio = 0,
            Motif = 1,
            Cadence = 2,
            Modifier = 3,
            Drum = 4,
            MidiMotif = 5,
            Activator = 6,
            Chorder = 7,
            None = 8,
        }

        public int UtId;
        public bool UtIsEnabled = true;
        public Component ComponantType;


        public void Awake()
        {
            //Debug.Log("Awake " + name + "  id:" + UtId);
            //UtGlobal.SongIsModified = true;
        }

        public void Start()
        {
            Debug.Log("Start " + name + "  id:" + UtId);
            if (UtId == 0 || IsIdExist())
            {
                UtId = GetUniqId();
                Debug.Log("Id already exist or 0, new id:" + UtId);
                if (this is UtMathMotif)
                {
                    name = "MathMotif_" + UtId;
                }
                else if (this is UtCadence)
                {
                    name = "Cadence_" + UtId;
                }
            }
        }
        public virtual void DefaultValue() { }
        public virtual void Generate(bool genRandom = false) { }


        static public void UtNew(UtComponent.Component component)
        {
            UtComponent ut = null;

            switch (component)
            {
                case UtComponent.Component.Motif:
                    ut = (UtMathMotif)Instantiate(InfinityMusic.instance.TemplateMathMotif, Vector3.zero, Quaternion.identity);
                    break;
                case UtComponent.Component.Cadence:
                    ut = (UtCadence)Instantiate(InfinityMusic.instance.TemplateCadence, Vector3.zero, Quaternion.identity);
                    break;
            }
            ut.ComponantType = component;
            ut.transform.parent = InfinityMusic.instance.transform;
            ut.DefaultValue();
            ut.Generate();

        }
        public bool IsIdExist()
        {
            bool exist = false;
            UtComponent[] components = GameObject.FindObjectsOfType<UtComponent>();

            foreach (UtComponent ut in components)
            {
                if (ut.UtId == UtId && this != ut)
                {
                    exist = true;
                    break;
                }
            }
            return exist;
        }
        public int GetUniqId()
        {
            int id = 1;
            UtComponent[] components = GameObject.FindObjectsOfType<UtComponent>();
            foreach (UtComponent ut in components)
            {
                if (ut.UtId >= id)
                    id = ut.UtId + 1;
            }
            return id;
        }

    }
}

