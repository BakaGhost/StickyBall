using System;
using System.Collections.Generic;
using Unity.VisualScripting;
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
    
    public GameObject RealGuide;
    public GameObject triangle;
    public Sprite PetiteF;
    public Sprite GrandF;

    [Header("Audio")]
    public AudioSource sourceAudio; 
    public AudioClip sonDash;     
    public AudioClip sonImpact;
    public AudioClip sonGlissade;
    
    // --- VARIABLES ---
    private Rigidbody2D rb;
    private Collider2D col;
    // private PhysicsMaterial2D matPhysique;
    private float tempsEcoule;
    private float timerImmune;
    private bool pressed;
    private bool Dashing = false;
    
    public CameraScript cameraEffect;
    public ParticleSystem Particule;
    [Header("Réglages Dash")]
    public Transform flechePos;
    public float force;
    
    [Header("Adhérence")]
    public float forceCollage = 20f;
    private float forceAdherence;    
    private float vitesseMaxGlissade;
    [Range(0f, 10f)] public float DurréeChute;
    private float TimerChute;
    
    // [Header("Effet Pendouille")]
    // public float vitessePendouille = 5f;
    // public float anglePendouille = 10f; 

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
    
    private bool AuSol;
    [HideInInspector]
    public bool AuPlafond;
    [System.NonSerialized]
    public bool Enter = false;
    private bool CanJump = false;
    
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        rb.sleepMode = RigidbodySleepMode2D.NeverSleep;

        // matPhysique = new PhysicsMaterial2D("MatJoueur");
        // matPhysique.friction = 0f;
        // col.sharedMaterial = matPhysique;

        if (modeAutomatique) GenererCourbe();
        Gravity = rb.gravityScale;
        forceAdherence = forceCollage;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
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
    }

    void FixedUpdate()
    {
        if (timerImmune > 0)
        {
            timerImmune -= Time.deltaTime;
        }

        if (pressed && CanJump)
        {
            timerImmune = 0.1f;
            Vector2 direction = new Vector2(flechePos.position.x - transform.position.x,
                flechePos.position.y - transform.position.y);
            direction.Normalize();
            forceAdherence = 0f;
            // matPhysique.friction = 0f;
            rb.gravityScale = Gravity;
            StartCoroutine(DesactiverCollider());
            rb.AddForce(direction * force, ForceMode2D.Impulse);
            pressed = false;
            if (cameraEffect != null)
            {
                // On envoie la direction du dash pour que la caméra aille à l'opposé
                cameraEffect.DeclencherLagDash(direction);
            }
            
            RealGuide.SetActive(false);
            
            sourceAudio.Stop();
            sourceAudio.clip = sonDash;
            sourceAudio.volume = 1f;
            sourceAudio.Play();
            CanJump = false;
        }
    }

    public void OnCollisionEnter2D(Collision2D other)
    {
        CanJump = true;
        Enter = true;
        ContactPoint2D contact;
        if (other.contactCount > 0)
        {
            Touche = true;
            tempsEcoule = 0f;
            
            rb.linearVelocity = Vector2.zero; 
            
            contact = other.GetContact(0);
            normale = contact.normal;
            if (cameraEffect != null)
            {
                cameraEffect.DeclencherImpact(normale, other.relativeVelocity);
            }
        }
        contact = other.GetContact(0);
        Particule.transform.position = contact.point;
        Particule.transform.forward = normale;
        var Emission =  Particule.emission;
        Emission.enabled = true;
        Particule.Play();
        TimerChute = DurréeChute;
        forceAdherence = forceCollage;
        Debug.Log("Impact");
        
        SpriteRenderer TriangleRender = triangle.GetComponent<SpriteRenderer>();
        TriangleRender.sprite = GrandF;
        
        sourceAudio.Stop();
        sourceAudio.clip = sonImpact;
        sourceAudio.volume = 1f;
        sourceAudio.Play();
        RealGuide.SetActive(true);
    }

    public void OnCollisionStay2D(Collision2D other)
    {
        if (other.contactCount > 0)
        {
            if (Dashing)
            {
                forceAdherence = 0f;
                return;
            }
            ContactPoint2D contact = other.GetContact(0);
            normale = contact.normal;

            float signedAngle = Vector2.SignedAngle(Vector2.right, normale);
            float angle = (signedAngle < 0) ? signedAngle + 360 : signedAngle;
            
            
            AuSol = (angle >= angleMinSol && angle <= angleMaxSol);
            AuPlafond = (angle > 260 && angle < 280);

            if (AuPlafond)
            {
                // 1. On diminue le timer privé (pas la variable publique !)
                TimerChute -= Time.fixedDeltaTime; 
                Debug.Log(TimerChute);

                if (TimerChute > 0)
                {
                    rb.gravityScale = 0f;
                    rb.AddForce(-normale * forceCollage); 
                    rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);
                }
                else
                {
                    // TEMPS ÉCOULÉ :
                    rb.gravityScale = 1f;
                }
            }
            else if (AuSol)
            {
                tempsEcoule = 0f;
                rb.gravityScale = Gravity;
                // matPhysique.friction = frictionInitiale; 
                forceAdherence = forceCollage;
            }
            else 
            {
                rb.gravityScale = 0f; 
                SensGlissade = (normale.x < 0) ? -1f : 1f;
                Glisser();
                Debug.Log(angle);
            }
            if (!sourceAudio.isPlaying && !AuSol)
            {
                sourceAudio.clip = sonGlissade;
                sourceAudio.volume = 1f;
                sourceAudio.Play();
            }
        }
    }
    void Glisser()
    {
        tempsEcoule += Time.fixedDeltaTime;
        float valeurCourbe = courbeFriction.Evaluate(tempsEcoule);
        
        Vector2 tangente = new Vector2(normale.y, -normale.x);

        Vector2 VitesseGlissade = tangente * SensGlissade * valeurCourbe;
        Vector2 VitesseCollage = -normale * forceAdherence;
        transform.Translate((VitesseCollage+VitesseGlissade)*Time.deltaTime, Space.World);


        Debug.DrawRay(transform.position, tangente * SensGlissade * 2f, Color.green);
        Debug.DrawRay(transform.position, -normale * 2f, Color.blue);
    }
    public void OnCollisionExit2D(Collision2D other)
    {
        TimerChute = DurréeChute;
        Touche = false;
        forceAdherence = forceCollage;
        if (sourceAudio.clip == sonGlissade)
        {
            sourceAudio.Stop();
        }
        SpriteRenderer TriangleRender = triangle.GetComponent<SpriteRenderer>();
        TriangleRender.sprite = PetiteF;
    }
    System.Collections.IEnumerator DesactiverCollider()
    {
        Dashing = true;
        col.enabled = false;
        yield return new WaitForSeconds(0.1f);
        col.enabled = true;
        Dashing =  false;
    }
}