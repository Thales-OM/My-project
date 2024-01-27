using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Turret : MonoBehaviour
{
    public float length = 1, height = 1, shooting_cooldown = 0.5f, bullet_speed = 4f;
    public bool drawDebug = true;
    public Transform FirePoint;
    public GameObject bulletPrefab;
    private float size_x, size_y, time_since_shot, cooldown_limiter = 0.1f;
    private bool isShooting, isFacingLeft, wasShooting;
    private Vector2 bottom_left, top_right, toPlayer, position2D;
    private SpriteRenderer Main_Renderer;
    private Collider2D Main_Collider;
    private Collider2D[] colliders_detected;
    private Animator animator;
    private GameObject lastBulletFired;
    void Start()
    {
        Main_Renderer = GetComponent<SpriteRenderer>();
        Main_Collider = GetComponent<BoxCollider2D>();
        animator = GetComponent<Animator>();
        size_x = Main_Renderer.bounds.size.x/2;
        size_y = Main_Renderer.bounds.size.y/2;
        isShooting = false;
        wasShooting = false;
        isFacingLeft = true;
        time_since_shot = 0f;
        animator.SetBool("isShooting", false);
    }

    void Update()
    {
        top_right = new Vector2(size_x/2*length, size_y/2*height);
        bottom_left = new Vector2(-size_x/2*length, -size_y/2);
        Player_Detection();
    }
    private void Shoot()
    {
        lastBulletFired = Instantiate(bulletPrefab, FirePoint.position, FirePoint.rotation);
        lastBulletFired.GetComponent<Bullet>().Initiate(Main_Collider, FirePoint.right * bullet_speed);
    }
    private void Player_Detection()
    {
        wasShooting = isShooting;
        isShooting = false;
        //collider_detected = Physics2D.OverlapBox(transform.TransformPoint(Vector2.zero), new Vector2(size_x*length, size_y/2 + size_y/2*height), 0f);
        colliders_detected = Physics2D.OverlapAreaAll(transform.TransformPoint(bottom_left), transform.TransformPoint(top_right)); //NonAlloc
        if(colliders_detected != null)
        {
            foreach(Collider2D collider in colliders_detected){
                if(collider.tag == "Player"){
                    isShooting = true;
                    toPlayer = new Vector2(collider.transform.position.x - transform.position.x, collider.transform.position.y - transform.position.y);
                    if(isFacingLeft && toPlayer.x > 0){Flip();}
                    if(!isFacingLeft && toPlayer.x < 0){Flip();}
                    break;
                }
            }
        }
        if(isShooting)
        {
            time_since_shot += Time.deltaTime;
            if(!wasShooting)
            {
                time_since_shot = 0f;
            }
            if(time_since_shot >= Mathf.Max(shooting_cooldown, cooldown_limiter))
            {
                Shoot();
                time_since_shot = 0f;
            }
        }
        animator.SetBool("isShooting", isShooting);
    }
    private void Flip()
    {
        isFacingLeft = !isFacingLeft;
        transform.Rotate(0f, 180f, 0f);
        Debug.Log(isFacingLeft);
    }

    private void OnDrawGizmos() {
        if(drawDebug){
            Custom_Debug.DrawRectangle(transform.TransformPoint(bottom_left), transform.TransformPoint(top_right));
            if(isShooting){
                position2D = new Vector2(transform.position.x, transform.position.y);
                Gizmos.DrawLine(position2D, position2D + toPlayer);
            }
        }
    }
}
