using System;
using UnityEngine;

public class Dash : MonoBehaviour
{
    private Rigidbody2D rb;
    public Transform flechePos;
    private bool pressed;
    public Vector3 normale;
    [SerializeField] public bool Touche = false;

    public float force;
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            pressed = true;
        }
    }

    void FixedUpdate()
    {
        if (pressed)
        {
            Debug.Log("Space");
            Vector2 direction = new Vector2(flechePos.position.x - transform.position.x, flechePos.position.y - transform.position.y);
            direction.Normalize();
            rb.AddForce(direction*force, ForceMode2D.Impulse);
            pressed = false;
        }
    }

    public void OnCollisionEnter2D(Collision2D other)
    {
        if (other.contactCount > 0)
        {
            Touche = true;
            Debug.Log("Contact");
            ContactPoint2D contact = other.GetContact(0);
            // Vector2 ContactPoint = contact.point;
            normale = contact.normal;
            Debug.Log(normale);
        }
    }
}
