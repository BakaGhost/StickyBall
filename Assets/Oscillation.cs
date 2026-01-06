using UnityEngine;

public class Oscillation : MonoBehaviour
{

    public float RotationSpeed;
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
        }        
        if (angleDifference > 60)
        {
            droite = true;
        }
        if (droite)
        {
            transform.Rotate(Vector3.forward, -RotationSpeed * Time.deltaTime);
        }
        else
        {
            transform.Rotate(Vector3.forward, RotationSpeed * Time.deltaTime);
        }
        Debug.DrawRay(transform.position, ScriptDash.normale * 2f, Color.red);
    }
}
