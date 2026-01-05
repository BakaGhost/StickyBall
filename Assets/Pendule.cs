using UnityEngine;

public class Pendule : MonoBehaviour
{
    [Header("Réglages Oscillation")]
    public float vitesse = 10f;    // Vitesse du balancement
    public float amplitude = 15f;  // Angle max (ex: 15 degrés)
    public float vitesseRetour = 5f; // Vitesse pour revenir droit quand on lâche

    private bool estAuPlafond = false;

    void Update()
    {
        if (estAuPlafond)
        {
            // Calcul de l'oscillation (Math Sinusoïdal)
            float angleZ = Mathf.Sin(Time.time * vitesse) * amplitude;
            
            // On applique la rotation LOCALE (par rapport au parent)
            transform.localRotation = Quaternion.Euler(0, 0, angleZ);
        }
        else
        {
            // Quand on n'est plus au plafond, on revient doucement à la position normale (0,0,0)
            transform.localRotation = Quaternion.Lerp(transform.localRotation, Quaternion.identity, Time.deltaTime * vitesseRetour);
        }
    }

    // Fonction appelée par le script Dash du parent
    public void Activer(bool etat)
    {
        estAuPlafond = etat;
    }
}
