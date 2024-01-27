using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{   
    public float lifetime = 10f;
    private bool initiated = false;
    private float current_life;
    private Collider2D shooter_collider;
    private Vector2 current_velocity_2D;
    private Vector3 current_velocity;
    public void Initiate(Collider2D collider, Vector2 velocity) // one-time initiation method for a new instance
    {
        if(!initiated){
            current_velocity_2D = velocity;
            current_velocity = new Vector3(current_velocity_2D.x, current_velocity_2D.y, 0);
            shooter_collider = collider;
        }
        initiated = true;
    }
    void Start()
    {
        if(!initiated){
            current_velocity_2D = new Vector2(transform.right.x, transform.right.y);
            current_velocity = new Vector3(current_velocity_2D.x, current_velocity_2D.y, 0);
        }
        current_life = 0f;
    }
    public void Change_Direction(Vector2 velocity)
    {
        transform.Rotate(Quaternion.FromToRotation(current_velocity, new Vector3(velocity.x, velocity.y, 0)).eulerAngles);
        current_velocity_2D = velocity;
        current_velocity = new Vector3(current_velocity_2D.x, current_velocity_2D.y, 0);
    }
    void Update()
    {
        flight();
        lifetime_check();
    }
    private void flight()
    {
        transform.position += Time.deltaTime * current_velocity;
    }

    private void OnTriggerEnter2D(Collider2D collider) {
        if(collider.tag != "Enemy" && collider != shooter_collider)
        {
            Debug.Log("Bullet hit " + collider.name);
            Explode();
        }
    }
    private void Explode()
    {
        Destroy(gameObject);
    }
    private void lifetime_check()
    {
        current_life += Time.deltaTime;
        if(current_life >= lifetime)
        {
            Explode();
        }
    }
    
}
