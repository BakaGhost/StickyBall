using UnityEngine;

public class Oscillation : MonoBehaviour
{

    public float RotationSpeed;
    public bool droite;
    public Dash ScriptDash;
    public float MyAngle;
    void Update()
    {
        if (transform.eulerAngles.z < MyAngle - 60)
        {
            droite = false;
        }        
        if  (transform.eulerAngles.z > MyAngle + 60)
        {
            droite = true;
        }
        
        if (droite)
        {
            transform.Rotate(Vector3.forward, -RotationSpeed*Time.deltaTime );
        }
        else
        {
            transform.Rotate(Vector3.forward, RotationSpeed*Time.deltaTime );
        }

        if (ScriptDash.Touche == true)
        {
            transform.up = ScriptDash.normale;
            MyAngle = Mathf.Atan2(ScriptDash.normale.y, ScriptDash.normale.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0,0,MyAngle);
            ScriptDash.Touche = false;
        }
        Debug.DrawRay(transform.position, ScriptDash.normale * 2f, Color.red);
    }
}
