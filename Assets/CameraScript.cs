using UnityEngine;

public class CameraScript : MonoBehaviour
{
    [Header("Réglages Dynamiques")]
    [Tooltip("Multiplicateur pour convertir la vitesse en force de secousse")]
    public float multiplicateurVitesse = 0.5f;

    public float forceMax = 50f;
    
    [Header("Réglages Élastique (Spring)")]
    [Tooltip("La rigidité du ressort. Plus c'est haut, plus le retour est sec.")]
    public float raideur = 150f; 
    [Tooltip("L'amortissement. Plus c'est haut, moins ça oscille longtemps.")]
    public float amortissement = 15f;
    
    [Header("Effet Dash (Lag & Zoom)")]
    [Tooltip("La force avec laquelle la caméra est coincée en arrière lors du dash")]
    public float dureeBlocage = 0.15f;
    public float intensiteZoomDash = 1.5f;
    public float forceDezoomImpact = 5f;
    public float MultiSpeedZoom = 1f;
    public float MultiSpeedDeZoom = 1f;
    private float timerBlocage;
    
    private float forceMin = 2f;
    private Vector3 positionCibleLocale;
    private Vector3 offsetActuel;        
    private Vector3 velocitySpring;     
    private Vector3 dernierePosParent;
    private Camera cam;
    private float tailleDeBase;
    private float offsetZoomActuel; // Le décalage du zoom (élastique)
    private float velocityZoomSpring; // La vitesse du ressort de zoom
    private float targetZoom = 0f;

    void Start()
    {
        positionCibleLocale = transform.localPosition;
        cam = GetComponentInChildren<Camera>();
        tailleDeBase = cam.orthographicSize;
    }

    void LateUpdate()
    {
        Vector3 deplacementParent = Vector3.zero;
        if (transform.parent != null)
        {
            deplacementParent = transform.parent.position - dernierePosParent;
            dernierePosParent = transform.parent.position;
        }
        if (timerBlocage > 0)
        {
            timerBlocage -= Time.deltaTime;
            offsetActuel -= deplacementParent;
            velocitySpring = Vector3.zero; 
        }
        else
        {
            Vector3 forceRappel = -offsetActuel * raideur;
            Vector3 forceAmortissement = -velocitySpring * amortissement;
            
            Vector3 acceleration = forceRappel + forceAmortissement;
            
            velocitySpring += acceleration * Time.deltaTime;
            offsetActuel += velocitySpring * Time.deltaTime;
        }
        transform.localPosition = positionCibleLocale + offsetActuel;

        float ecartParRapportCible = offsetZoomActuel - targetZoom;

        float forceRappelZoom = -ecartParRapportCible * raideur;
        float forceAmortissementZoom = -velocityZoomSpring * amortissement;
        float accelerationZoom = forceRappelZoom + forceAmortissementZoom;

        velocityZoomSpring += accelerationZoom * Time.deltaTime * MultiSpeedZoom;
        offsetZoomActuel += velocityZoomSpring * Time.deltaTime*MultiSpeedDeZoom;

        cam.orthographicSize = tailleDeBase + offsetZoomActuel;
    }
    public void DeclencherImpact(Vector2 normaleCollision, Vector2 vitesseImpact)
    {
        float intensite = vitesseImpact.magnitude * multiplicateurVitesse;
        intensite = Mathf.Max(intensite, forceMin);
        Debug.Log(intensite);
        intensite = Mathf.Clamp(intensite, forceMin, forceMax);
        velocitySpring = new Vector3(-normaleCollision.x, -normaleCollision.y, 0) * intensite;
        targetZoom = 0f;
        velocityZoomSpring = -forceDezoomImpact;
    }
    public void DeclencherLagDash(Vector2 directionDash)
    {
        timerBlocage = dureeBlocage;
        targetZoom = -intensiteZoomDash;    }
}