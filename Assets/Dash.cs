using System;
using System.Collections.Generic;
using UnityEngine;

public class Dash : MonoBehaviour
{
    [System.Serializable]
    public struct PointIntermediaire
    {
        [Range(0f, 1f)] public float positionTemps;
        public float friction;
        public float inclinaison;
    }

    // --- VARIABLES ---
    private Rigidbody2D rb;
    private Collider2D col;
    private PhysicsMaterial2D matPhysique;
    private float tempsEcoule;
    private bool pressed;

    // Timer pour empêcher le mur d'annuler le saut
    private float cooldownDash = 0f; 

    [Header("Réglages Dash")]
    public Transform flechePos;
    public float force;
    
    [Header("Adhérence")]
    public float forceCollage = 20f; // La force max quand on colle
    private float forceAdherence;    // La force actuelle (variable)
    private float vitesseMaxGlissade;
    [Range(0f, 0.1f)] public float DurréeChute;

    [Header("Générateur de Courbe")]
    public bool modeAutomatique = true;

    [Header("Départ")]
    public float frictionInitiale = 10f; 
    public float inclinaisonDepart = 0f;

    [Header("Milieu")]
    public List<PointIntermediaire> pointsSupplementaires; 

    [Header("Fin")]
    public float dureeGlissade = 2f;
    public float inclinaisonFin = 0f;
    
    [Header("Visualisation")]
    public AnimationCurve courbeFriction;

    [Header("Détection Angles")]
    public float angleMinSol = 85f; 
    public float angleMaxSol = 95f;

    public float forceDecollement = 5f; 
    [SerializeField] public bool Touche = false;
    public Vector3 normale;
    private float SensGlissade;
    private float Gravity;
    
    // États simplifiés
    private bool AuSol;
    private bool AuPlafond;
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();

        matPhysique = new PhysicsMaterial2D("MatJoueur");
        matPhysique.friction = 0f;
        col.sharedMaterial = matPhysique;

        if (modeAutomatique) GenererCourbe();
        Gravity = rb.gravityScale;
        forceAdherence = forceCollage;
    }

    void OnValidate() { if (modeAutomatique) GenererCourbe(); }

    void GenererCourbe()
    {
        Keyframe[] cles = new Keyframe[2 + pointsSupplementaires.Count];
        cles[0] = new Keyframe(0, 0f , inclinaisonDepart, inclinaisonDepart);
        for (int i = 0; i < pointsSupplementaires.Count; i++)
        {
            PointIntermediaire p = pointsSupplementaires[i];
            float tempsReel = p.positionTemps * dureeGlissade;
            cles[i + 1] = new Keyframe(tempsReel, p.friction, p.inclinaison, p.inclinaison);
        }
        cles[cles.Length - 1] = new Keyframe(dureeGlissade, frictionInitiale, inclinaisonFin, inclinaisonFin);
        courbeFriction = new AnimationCurve(cles);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)) pressed = true;
        if (cooldownDash > 0) cooldownDash -= Time.deltaTime;
    }

    void FixedUpdate()
    {
        if (pressed)
        {
            Vector2 direction = new Vector2(flechePos.position.x - transform.position.x, flechePos.position.y - transform.position.y);
            direction.Normalize();
            
            matPhysique.friction = 0f;
            col.sharedMaterial = matPhysique;

            cooldownDash = 0.2f; 
            rb.gravityScale = Gravity; 

            rb.AddForce(direction * force, ForceMode2D.Impulse);
            pressed = false;
        }
    }

    public void OnCollisionEnter2D(Collision2D other)
    {
        if (other.contactCount > 0)
        {
            if (cooldownDash > 0) return;

            Touche = true;
            tempsEcoule = 0f;
            
            // Stop net pour coller
            rb.linearVelocity = Vector2.zero; 
            
            ContactPoint2D contact = other.GetContact(0);
            normale = contact.normal;
        }
        // On remet l'adhérence au max à chaque impact
        forceAdherence = forceCollage;
        Debug.Log("Impact");
    }

    public void OnCollisionStay2D(Collision2D other)
    {
        if (cooldownDash > 0) return;

        if (other.contactCount > 0)
        {
            ContactPoint2D contact = other.GetContact(0);
            normale = contact.normal;

            float signedAngle = Vector2.SignedAngle(Vector2.right, normale);
            float angle = (signedAngle < 0) ? signedAngle + 360 : signedAngle;
            Debug.Log(angle);
            
            // On définit le plafond (le haut du cercle)
            AuSol = (angle >= angleMinSol && angle <= angleMaxSol);
            AuPlafond = (angle > 260 && angle < 280);

            if (AuPlafond)
            {
                rb.gravityScale = 0f; 
                forceAdherence = Mathf.Lerp(forceAdherence, 0, DurréeChute);
                SensGlissade = (rb.linearVelocity.x > 0) ? 1f : -1f;
                Vector2 tangente = new Vector2(normale.y, -normale.x);
                rb.AddForce(-normale * forceAdherence);
            }
            else if (AuSol)
            {
                // --- SOL (AJOUTÉ ICI) ---
                // On remet la physique normale pour ne pas glisser
                tempsEcoule = 0f;
                rb.gravityScale = Gravity;
                
                // On met une friction élevée pour s'arrêter net au sol
                matPhysique.friction = frictionInitiale; 
                forceAdherence = forceCollage;
            }
            else 
            {
                // --- MUR (Tout le reste) ---
                // Ni sol, ni plafond = Mur (Pas de trou possible)
                
                forceAdherence = forceCollage; // On colle fort
                rb.gravityScale = 0f;          // Pas de gravité

                // Détection auto du sens : Mur Droite (x<0) -> -1, Mur Gauche (x>0) -> 1
                SensGlissade = (normale.x < 0) ? -1f : 1f;

                Glisser();
            }
        }
    }

    void Glisser()
    {
        tempsEcoule += Time.fixedDeltaTime;
        float valeurCourbe = courbeFriction.Evaluate(tempsEcoule);

        // 1. Calcul Direction
        Vector2 tangente = new Vector2(normale.y, -normale.x);

        // 2. Vitesse (Longitudinal)
        Vector2 VitesseGlissade = tangente * SensGlissade * valeurCourbe;
        Vector2 VitesseCollage = -normale * forceAdherence;
        rb.linearVelocity = VitesseCollage+VitesseGlissade;

        // 3. Adhérence (Transversal)
        // Utilise la variable forceAdherence (qui est 0 au plafond, et 20 sur les murs)
        Debug.DrawRay(transform.position, tangente * SensGlissade * 2f, Color.green);
        Debug.DrawRay(transform.position, -normale * 2f, Color.blue);
    }

    public void OnCollisionExit2D(Collision2D other)
    {
        rb.gravityScale = Gravity;
        Touche = false;
        matPhysique.friction = 0f;
        col.sharedMaterial = matPhysique;
        forceAdherence = forceCollage;
    }
}