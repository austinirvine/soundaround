using MidiPlayerTK;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoteView : MonoBehaviour
{
    public static float Speed = 10f;

    public MidiNote note;
    public MidiFilePlayer midiFilePlayer;
    public bool played = false;
    public Material NewNote;
    public Material WaitingNote;
    public Material ReadyNote;
    public float zOriginal;

    public Rigidbody _body;
    public Vector3 _inputs = Vector3.zero;

    // Use this for initialization
    void Start()
    {
        _body = GetComponent<Rigidbody>();

    }

    // Update is called once per frame
    void Update()
    {
        //float translation = Time.deltaTime * 10;
        //transform.Translate(-translation, 0, 0);
        if (!played && transform.position.x < -45f)
        {
            played = true;
            int delta = Mathf.CeilToInt(zOriginal - transform.position.z + 0.5f);
            note.Midi += delta;
            midiFilePlayer.MPTK_PlayNote(note);

            gameObject.GetComponent<Renderer>().material = ReadyNote;// .color = Color.red;
        }
        if (transform.position.y < -30f)
        {
            Destroy(this.gameObject);
        }
    }

    void FixedUpdate()
    {
        //_inputs = Vector3.zero;
        //_inputs.x = -1;// Input.GetAxis("Horizontal");
        ////_inputs.z = Input.GetAxis("Vertical");
        //_body.MovePosition(_body.position + _inputs * Speed * Time.fixedDeltaTime);

        float translation = Time.fixedDeltaTime * Speed;
        transform.Translate(-translation, 0, 0);

    }

}
