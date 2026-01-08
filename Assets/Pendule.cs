using UnityEngine;

public class Pendule : MonoBehaviour
{
    [Header("Réglages Oscillation")]
    public float vitesse = 10f;    // Vitesse du balancement
    public float amplitude = 15f;  // Angle max (ex: 15 degrés)
    public float vitesseRetour = 5f; // Vitesse pour revenir droit quand on lâche
    
    [Header("Réglages Ressort Scale Y")]
    [Tooltip("Vitesse de l'effet d'étirement/écrasement (souvent plus rapide que la rotation)")]
    public float vitesseRessortY = 15f; 
    [Tooltip("Force de l'étirement. 0.2 signifie +/- 20% de la taille Y.")]
    public float forceRessortY = 0.2f;
    
    private Vector3 scaleInitial;

    public Dash DashScript;
    
    void Start()
    {
        // On sauvegarde la taille de départ (ex: 1,1,1)
        scaleInitial = transform.localScale;
    }
    
    void Update()
    {
        if (DashScript.normale == Vector3.zero) return;
        Quaternion rotationMur = Quaternion.FromToRotation(Vector3.up, DashScript.normale);
        float angleZ = 0f;
        if (DashScript.AuPlafond)
        {
            // ROTATION
            angleZ = Mathf.Sin(Time.time * vitesse) * amplitude;

            // --- 2. Gestion du Scale Y (NOUVEAU CODE ICI) ---
            
            // On crée une onde sinusoïdale dédiée au scale (souvent avec une vitesse différente)
            // Elle va de -1 à 1
            float ondeRessort = Mathf.Sin(Time.time * vitesseRessortY);

            // On calcule le montant du changement (ex: si force est 0.2, ça va de -0.2 à 0.2)
            float changementY = ondeRessort * forceRessortY;

            // On part du scale initial pour être propre
            Vector3 nouveauScale = scaleInitial;
            
            // On applique le changement UNIQUEMENT sur l'axe Y
            // Si ondeRessort est positif, ça s'étire. Négatif, ça s'écrase.
            nouveauScale.y = scaleInitial.y + changementY;

            // On applique le nouveau scale à l'objet
            transform.localScale = nouveauScale;
            Debug.Log("Je Pendouille");
        }

        // 4. On crée la rotation du balancement
        Quaternion rotationOscillation = Quaternion.Euler(0, 0, angleZ);

        // 5. MATHÉMATIQUES MAGIQUES : On multiplie les deux rotations
        // En Quaternions, multiplier = "Ajouter la rotation B à la rotation A"
        transform.rotation = rotationMur * rotationOscillation;
    }
}
