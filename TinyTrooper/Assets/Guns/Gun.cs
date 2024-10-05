using UnityEngine;

[CreateAssetMenu(fileName = "Gun", menuName = "Scriptable Objects/Gun")]
public class Gun : ScriptableObject
{
    public float BulletSpeed = 20.0f;
    public float BulletGravity = 1.0f;
    public float FireRate = 100;
    public float Damage = 1;
    public float MaterialDamage = 1;
    public int Penetration = 2;
    public AudioClip Sound;
    public bool InfiniteAmmo = false;
    public AmmoType ammoType = AmmoType.PISTOL;
}

public enum AmmoType {
    PISTOL,
    RIFLE,
    SNIPER
}
