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

    private AudioSource audioSource;
    private bool dead = false;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        anim = GetComponentInChildren<Animator>();
        audioSource = GetComponent<AudioSource>();
        currentGun = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (dead) return;
        var horiz = Input.GetAxis("Horizontal");
        var vert = Input.GetAxis("Vertical");
        input = new Vector2(horiz, vert);

        transform.Rotate(Vector3.up, horiz * Time.deltaTime * 720);
        rb.rotation = transform.rotation;

        if(Input.GetKey(KeyCode.C) && readyToFire()) {
            var moa = guns[currentGun].Accuracy;
            var inaccuracy = new Vector3(Random.Range(-moa, moa), Random.Range(moa / 10.0f, moa / 10.0f), Random.Range(-moa, moa));
            var bullet = Instantiate(bulletPrefab);
            bullet.transform.position = muzzle.position;
            bullet.transform.forward = transform.forward * 10 + inaccuracy;
            bullet.Init(guns[currentGun]);
            gunTimer = Time.time + (60.0f / guns[currentGun].FireRate);
            audioSource.PlayOneShot(guns[currentGun].Sound);
            if (!guns[currentGun].InfiniteAmmo) {
                GameManager.Instance.PlayerAmmo[guns[currentGun].ammoType]--;
            }
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
    }

    public bool PickUp(PickupType type) {
        if (type == PickupType.HEALTH) {
            if (Health < maxHealth) {
                Health += 25;
                if (Health > maxHealth) {
                    Health = maxHealth;
                }
                return true;
            }
            return false;
        }
        if (type == PickupType.RIFLE_AMMO) {
            GameManager.Instance.PlayerAmmo[AmmoType.RIFLE] += 20;
            return true;
        }
        if (type == PickupType.SNIPER_AMMO) {
            GameManager.Instance.PlayerAmmo[AmmoType.SNIPER] += 3;
            return true;
        }
        if (type == PickupType.QUEST) {
            GameManager.Instance.QuestItemsFound++;
            return true;
        }
        return false;
    }

    void FixedUpdate()
    {
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
        currentGun = gun;
    }

    public void Hurt(float damage) {
        Health -= damage;
        if (Health <= 0.0f) {
            anim.SetBool("dead", true);
            dead = true;
        }
    }
}
