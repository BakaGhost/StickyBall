using UnityEngine;

public class GuideNormale : MonoBehaviour
{
    public Dash DashScript;

    // Update is called once per frame
    void Update()
    {
        transform.up = DashScript.normale;
    }
}
