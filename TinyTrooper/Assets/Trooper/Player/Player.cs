using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Player : MonoBehaviour
{
    private Rigidbody rb;
    private Vector2 input;

    private float MAX_SPEED = 5.0f;

    [SerializeField]
    private Bullet bulletPrefab;

    [SerializeField]
    private Transform muzzle;

    private int currentGun;
    
    [SerializeField]
    private Gun[] guns;

    private float gunTimer = 0;
    private Animator anim;

    public float Health = 100;
    public float maxHealth = 100;

    [SerializeField]
    private AudioSource gunSounds;

    [SerializeField]
    private AudioSource otherSounds;
    private bool dead = false;

    [SerializeField]
    private AudioClip pickup;
    [SerializeField]
    private AudioClip changeWeapon;
    [SerializeField]
    private AudioClip medkit;
    [SerializeField]
    private AudioClip objective;

    private bool clickPlayed = false;
    

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        anim = GetComponentInChildren<Animator>();
        currentGun = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (dead) return;
        if (GameManager.Instance.BriefingActive) return;

        var horiz = Input.GetAxis("Horizontal");
        var vert = Input.GetAxis("Vertical");
        input = new Vector2(horiz, vert);

        transform.Rotate(Vector3.up, horiz * Time.deltaTime * 720);
        rb.rotation = transform.rotation;

        if(Input.GetKey(KeyCode.X) || Input.GetKey(KeyCode.Comma)) {
            if (readyToFire()) {
                var moa = guns[currentGun].Accuracy;
                var inaccuracy = new Vector3(Random.Range(-moa, moa), Random.Range(moa / 10.0f, moa / 10.0f), Random.Range(-moa, moa));
                var bullet = Instantiate(bulletPrefab);
                bullet.transform.position = muzzle.position;
                bullet.transform.forward = transform.forward * 10 + inaccuracy;
                bullet.Init(guns[currentGun]);
                gunTimer = Time.time + (60.0f / guns[currentGun].FireRate);
                gunSounds.PlayOneShot(guns[currentGun].Sound);
                if (!guns[currentGun].InfiniteAmmo) {
                    GameManager.Instance.PlayerAmmo[guns[currentGun].ammoType]--;
                }
            } else if (gunTimer < Time.time && !clickPlayed) {
                gunSounds.PlayOneShot(guns[currentGun].EmptySound);
                clickPlayed = true;
            }
        } else {
            clickPlayed = false;
        }

        if (rb.linearVelocity.magnitude > 0.01f) {
            anim.SetBool("run", true);
        } else {
            anim.SetBool("run", false);
        }

        if (Input.GetKeyDown(KeyCode.Alpha1)) {
            selectGun(0);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2)) {
            selectGun(1);
        }
        if (Input.GetKeyDown(KeyCode.Alpha3)) {
            selectGun(2);
        }

        if (Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.Period)) {
            if (currentGun < 2) {
                selectGun(currentGun+1);
            } else {
                selectGun(0);
            }
        }
    }

    public void ObjectiveCompleted() {
        otherSounds.PlayOneShot(objective);
    }

    public bool PickUp(PickupType type) {
        if (type == PickupType.HEALTH) {
            if (Health < maxHealth) {
                Health += 25;
                if (Health > maxHealth) {
                    Health = maxHealth;
                }
                otherSounds.PlayOneShot(medkit);
                return true;
            }
            return false;
        }
        if (type == PickupType.RIFLE_AMMO) {
            GameManager.Instance.PlayerAmmo[AmmoType.RIFLE] += 20;
            otherSounds.PlayOneShot(pickup);
            return true;
        }
        if (type == PickupType.SNIPER_AMMO) {
            GameManager.Instance.PlayerAmmo[AmmoType.SNIPER] += 3;
            otherSounds.PlayOneShot(pickup);
            return true;
        }
        if (type == PickupType.QUEST) {
            GameManager.Instance.QuestItemsFound++;
            otherSounds.PlayOneShot(objective);
            return true;
        }
        return false;
    }

    void FixedUpdate()
    {
        if (GameManager.Instance.BriefingActive) return;
        if (dead)
        {
            rb.linearVelocity = Vector3.zero;
            return;
        }
        rb.AddForce(transform.forward * input.y * 100.0f, ForceMode.Acceleration);
        if (rb.linearVelocity.magnitude > MAX_SPEED) {
            rb.linearVelocity = rb.linearVelocity.normalized * MAX_SPEED;
        }
    }

    private bool readyToFire() {
        var ammo = GameManager.Instance.PlayerAmmo[guns[currentGun].ammoType];
        return gunTimer < Time.time && ammo > 0;
    }

    private void selectGun(int gun) {
        if (currentGun == gun) return;
        currentGun = gun;
        GameManager.Instance.SelectGun(gun);
        otherSounds.PlayOneShot(changeWeapon);
    }

    public void Hurt(float damage) {
        Health -= damage;
        if (Health <= 0.0f) {
            anim.SetBool("dead", true);
            dead = true;
            GameManager.Instance.Die();
        }
    }
}
