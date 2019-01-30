using UnityEngine;
using System.Collections;

public class FlythroughCameraController : MonoBehaviour
{
    public Transform[] movePath;
    public Transform[] lookPath;
    public Transform lookTarget;
    public float percentage;
    [Range(0,300)]
    public float speed=20;

    void FixedUpdate()
    {
        percentage += Time.deltaTime * speed/800;
        if (percentage >= 1f) percentage = 0f;
    }


    void OnGUI()
    {
        percentage = GUI.VerticalSlider(new Rect(Screen.width - 20, 20, 15, Screen.height - 40), percentage, 1, 0);
        //Debug.Log("OnGUI percentage :" + percentage);

        iTween.PutOnPath(gameObject, movePath, percentage);
        iTween.PutOnPath(lookTarget, lookPath, percentage);
        transform.LookAt(lookTarget);
    }

    void OnDrawGizmos()
    {
        iTween.DrawPath(movePath, Color.magenta);
        iTween.DrawPath(lookPath, Color.cyan);
        Gizmos.color = Color.black;
        Gizmos.DrawLine(transform.position, lookTarget.position);
    }

    void SlideTo(float position)
    {
        iTween.Stop(gameObject);
        iTween.ValueTo(gameObject, iTween.Hash("from", percentage, "to", position, "time", 2, "easetype", iTween.EaseType.easeInOutCubic, "onupdate", "SlidePercentage"));
    }

    void SlidePercentage(float p)
    {
        percentage = p;
    }
}
