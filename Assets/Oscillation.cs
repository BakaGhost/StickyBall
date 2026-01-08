using UnityEngine;

public class Oscillation : MonoBehaviour
{
    [Header("Audio")]
    public AudioSource sourceAudio;
    public AudioClip Tic;
    public float RotationSpeed;
    public float RotationUltraSpeed;
    private float angleDifference;
    public bool droite;
    public Dash ScriptDash;
    public float MyAngle;
    void Update()
    {
        if (ScriptDash.Touche == true)
        {
            MyAngle = Mathf.Atan2(ScriptDash.normale.y, ScriptDash.normale.x) * Mathf.Rad2Deg;
            angleDifference = Mathf.DeltaAngle(MyAngle, transform.eulerAngles.z);
            transform.rotation = Quaternion.Euler(0,0,MyAngle);
            ScriptDash.Touche = false;
        }
        
        MyAngle = Mathf.Atan2(ScriptDash.normale.y, ScriptDash.normale.x) * Mathf.Rad2Deg;
        angleDifference = Mathf.DeltaAngle(MyAngle, transform.eulerAngles.z);

        if (angleDifference < -60)
        {
            droite = false;
            sourceAudio.Stop();
            sourceAudio.clip = Tic;
            sourceAudio.volume = 1f;
            sourceAudio.Play();
        }        
        if (angleDifference > 60)
        {
            droite = true;
            sourceAudio.Stop();
            sourceAudio.clip = Tic;
            sourceAudio.volume = 1f;
            sourceAudio.Play();
        }
        if (droite)
        {
            if (angleDifference > 30)
            {
                transform.Rotate(Vector3.forward, -RotationSpeed * Time.deltaTime);
            }
            else
            {
                transform.Rotate(Vector3.forward, -RotationUltraSpeed * Time.deltaTime);
            }
        }
        else
        {
            if (angleDifference < -30)
            {
                transform.Rotate(Vector3.forward, RotationSpeed * Time.deltaTime);
            }
            else
            {
                transform.Rotate(Vector3.forward, RotationUltraSpeed * Time.deltaTime);
            }
        }
        Debug.DrawRay(transform.position, ScriptDash.normale * 2f, Color.red);
    }
}
