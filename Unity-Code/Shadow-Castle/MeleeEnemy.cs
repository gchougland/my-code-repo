using UnityEngine;

public class MeleeEnemy : Enemy
{
    [SerializeField] private int damage = 1;
    private bool _isHittingPlayer = false;
    private Player _playerComponent;
    
    public override void Update()
    {
        base.Update();
        if (_isHittingPlayer && attackTimer <= 0)
        {
            attackTimer = attackSpeed;
            _playerComponent.Damage(damage);
        }
    }
    
    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.collider.CompareTag("Player"))
        {
            _isHittingPlayer = true;
            _playerComponent = other.gameObject.GetComponent<Player>();
        }
    }

    private void OnCollisionExit2D(Collision2D other)
    {
        if (other.collider.CompareTag("Player"))
        {
            _isHittingPlayer = false;
        }
    }
}