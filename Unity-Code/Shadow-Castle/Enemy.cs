
using System;
using UnityEngine;

public class Enemy : MonoBehaviour, IDamageable
{
    [SerializeField] private int health = 10;
    [SerializeField] private int maxHealth = 10;
    [SerializeField] private float damageFlashTime = .5f;
    [SerializeField] private float sightRange = 5f;
    [SerializeField] private float sightLoseRange = 10f;
    [SerializeField] private float timeToForget = 5f;
    [SerializeField] protected float attackSpeed = 1f;
    
    [SerializeField] private float moveSpeed = 10f;
    private float _flashTimeRemaining = 0f;
    private SpriteRenderer _spriteRenderer;
    private Transform _playerTransform;
    private bool _canSeePlayer = false;
    [SerializeField] private bool debugAI = false;
    private CircleCollider2D _circleCollider2D;
    private float _timeSinceDamage = 0f;
    protected float attackTimer = 0f;


    private void Start()
    {
        _circleCollider2D = GetComponent<CircleCollider2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        Health = maxHealth;
        _playerTransform = GameObject.FindWithTag("Player").transform;
    }

    private int Health
    {
        get => this.health;
        set => this.health = Math.Clamp(value, 0, maxHealth);
    }

    public virtual void Update()
    {
        if (attackTimer > 0)
            attackTimer -= Time.deltaTime;
        if (_timeSinceDamage < timeToForget)
            _timeSinceDamage += Time.deltaTime;
        if (_flashTimeRemaining > 0)
            _flashTimeRemaining -= Time.deltaTime;
        float colorVal = 1 - _flashTimeRemaining / damageFlashTime;
        _spriteRenderer.color = new Color(1, colorVal, colorVal);

        int layerMask = 1 << 3 | 1 << 0;
        Vector3 direction = (_playerTransform.position - transform.position).normalized;
        float agentRadius = _circleCollider2D.radius/2f;
        if(debugAI)
            Debug.DrawRay(transform.position + direction*agentRadius, direction*sightRange);
        if (!_canSeePlayer)
        {
            RaycastHit2D hit = Physics2D.Raycast(transform.position + direction * agentRadius, direction, sightRange,
                layerMask);
            if (hit.collider != null && hit.collider.CompareTag("Player"))
            {
                if (debugAI)
                    _flashTimeRemaining = damageFlashTime;
                _canSeePlayer = true;
            }
        }
        else
        {
            if (debugAI)
                _flashTimeRemaining = damageFlashTime;
            // Out of sightLoseRange and has Forgotten damage
            if (Vector2.Distance(_playerTransform.position, transform.position) >= sightLoseRange && 
                _timeSinceDamage >= timeToForget)
                _canSeePlayer = false;
        }

        if (_canSeePlayer)
        {
            transform.Translate(direction * (moveSpeed * Time.deltaTime));
        }
    }


    public void Damage(int damage)
    {
        Health -= damage;
        _timeSinceDamage = 0;
        _canSeePlayer = true;
        _flashTimeRemaining = damageFlashTime;
        if (Health <= 0)
        {
            Destroy(gameObject);
        }
    }
}