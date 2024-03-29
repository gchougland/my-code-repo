
using UnityEngine;

public abstract class Spell : ScriptableObject
{
    [Tooltip("Time between shots (seconds)")]
    public float cooldown;
    [SerializeField] protected GameObject projectile;

    public abstract void CastSpell(Vector3 pos, Quaternion dir);
}
