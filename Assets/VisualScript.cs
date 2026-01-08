using UnityEngine;

public class VisualScript : MonoBehaviour
{
    public Dash DashScript;
    
    [Header("Réglages")]
    [Tooltip("Vitesse de l'oscillation")]
    public float vitesse = 5f;
    
    [Tooltip("Intensité de la déformation")]
    public float force = 0.1f;
    public float dureeEffet = 1.0f;
    
    private Vector3 scaleInitial;
    private float timerRestant;
    void Start()
    {
        scaleInitial = transform.localScale;
        timerRestant = dureeEffet;
    }

   // Update is called once per frame
    void Update()
    {
        if (DashScript.Enter)
        {
            timerRestant -= Time.deltaTime;

            // On calcule un "Ratio" qui va de 1 (début) à 0 (fin)
            // Mathf.Clamp01 s'assure qu'on ne dépasse pas les bornes
            float ratio = Mathf.Clamp01(timerRestant / dureeEffet);

            // On calcule l'onde sinusoïdale
            float sinus = Mathf.Sin(Time.time * vitesse);
                
            // LA CLÉ EST ICI : On multiplie la force initiale par le ratio.
            // Au début : Force * 1
            // À la fin : Force * 0
            float changement = sinus * (force * ratio);

            Vector3 nouveauScale = scaleInitial;

            nouveauScale.x = scaleInitial.x + changement;
            nouveauScale.y = scaleInitial.y - changement;
            
            transform.localScale = nouveauScale;
            if ( timerRestant <= 0)
            {
                DashScript.Enter = false;
                timerRestant = dureeEffet;
            }
        }
        transform.up = DashScript.normale;
    }
}
