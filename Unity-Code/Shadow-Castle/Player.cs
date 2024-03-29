using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Debug = System.Diagnostics.Debug;

public class Player : MonoBehaviour, IDamageable
{
    [SerializeField] private Camera playerCam;
    private InputAction _moveAction;
    [SerializeField] private float moveSpeed = 10f;
    private Rigidbody2D _rigidbody2D;
    [SerializeField] private Spell spellSlot1;
    [SerializeField] private Spell spellSlot2;
    private bool _canFire = true;
    private float _cooldownTimer = 0f;
    public InputAction castPrimaryAction;
    public InputAction castSecondaryAction;
    [SerializeField] private int health;
    [SerializeField] private int maxHealth;
    [SerializeField] private float damageFlashTime = .5f;
    private float _flashTimeRemaining = 0f;
    private SpriteRenderer _spriteRenderer;
    [SerializeField] private PlayerHealthBar healthBar;

    private int Health
    {
        get => this.health;
        set
        {
            this.health = Math.Clamp(value, 0, maxHealth);
            healthBar.UpdateHealth(this.health);
        }
    }

    public Spell SpellSlot1 => this.spellSlot1;
    public Spell SpellSlot2 => this.spellSlot2;
    private float MoveSpeed => this.moveSpeed;
    
    private void OnEnable()
    {
        castPrimaryAction.Enable();
        castSecondaryAction.Enable();
    }

    private void OnDisable()
    {
        castPrimaryAction.Disable();
        castPrimaryAction.Disable();
    }

    private void Start()
    {
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        Debug.Assert(playerCam != null, "Camera.main != null");
        healthBar.UpdateHealth(health);
        _moveAction = InputSystem.actions.FindAction("Move");
    }

    private void Update()
    {
        var position = transform.position;
        var lookRot = Quaternion.LookRotation(
            playerCam.ScreenToWorldPoint(Mouse.current.position.ReadValue()) - position,
            new Vector3(0,0,1));
        lookRot.x = 0;
        lookRot.y = 0;
        transform.rotation = lookRot;
        
        var moveValue = _moveAction.ReadValue<Vector2>();
        _rigidbody2D.velocity = moveValue * MoveSpeed;

        playerCam.transform.position = new Vector3(position.x, position.y, -10);

        if (_cooldownTimer > 0f)
            _cooldownTimer -= Time.deltaTime;
        else
            _canFire = true;

        if (_canFire)
        {
            if (castPrimaryAction.IsPressed())
                CastSpell(SpellSlot1);
            else if (castSecondaryAction.IsPressed())
                CastSpell(SpellSlot2);
        }
        
        if (_flashTimeRemaining > 0)
            _flashTimeRemaining -= Time.deltaTime;
        float colorVal = 1 - _flashTimeRemaining / damageFlashTime;
        _spriteRenderer.color = new Color(1, colorVal, colorVal);
    }

    private void CastSpell(Spell spell)
    {
        spell.CastSpell(transform.position, transform.rotation * Quaternion.Euler(0, 0, 180f));
        _cooldownTimer = spell.cooldown;
        _canFire = false;
    }

    public void Damage(int damage)
    {
        Health -= damage;
        _flashTimeRemaining = damageFlashTime;
    }

    public void AddHeartContainer()
    {
        healthBar.AddHeart();
        maxHealth += 4;
        health += 4;
    }
}
