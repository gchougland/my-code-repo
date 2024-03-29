
using UnityEngine;

[CreateAssetMenu(fileName = "NewSpell", menuName = "Spells/Basic")]
public class BasicSpell : Spell
{
    public override void CastSpell(Vector3 pos, Quaternion dir)
    {
        Vector3 spawnPos = pos + dir*new Vector3(0,1,0);
        Instantiate(projectile, spawnPos, dir);
    }
}