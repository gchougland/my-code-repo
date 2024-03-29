using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class Projectile : MonoBehaviour
{
    [SerializeField] private float speed = 100f;
    [SerializeField] private float lifetime = 1f;
    [SerializeField] private int damage = 1;
    [SerializeField] private bool playerProjectile = true;
    private float _destroyTimer;
    private int _layerMask;
    
    void Start()
    {
        GetComponent<Rigidbody2D>().AddForce(transform.up*speed);
        _destroyTimer = lifetime;
        _layerMask = LayerMask.NameToLayer("Solid");
    }

    private void Update()
    {
        _destroyTimer -= Time.deltaTime;
        if (_destroyTimer <= 0)
        {
            OnProjectileHit();
        }
    }

    protected virtual void OnProjectileHit()
    {
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy") && playerProjectile)
        {
            other.GetComponent<Enemy>().Damage(damage);
            OnProjectileHit();
        }
        else if (other.CompareTag("Player") && !playerProjectile)
        {
            other.GetComponent<Player>().Damage(damage);
            OnProjectileHit();
        }
        else if(other.gameObject.layer == _layerMask)
        {
            OnProjectileHit();
        }
    }
}
