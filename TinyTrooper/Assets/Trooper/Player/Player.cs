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
        var horiz = Input.GetAxis("Horizontal");
        var vert = Input.GetAxis("Vertical");
        input = new Vector2(horiz, vert);

        transform.Rotate(Vector3.up, horiz * Time.deltaTime * 720);

        if(Input.GetKey(KeyCode.C) && readyToFire()) {
            var bullet = Instantiate(bulletPrefab);
            bullet.transform.position = muzzle.position;
            bullet.transform.forward = transform.forward;
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

    void FixedUpdate()
    {
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
        Debug.Log("OUCH!");
        if (Health <= 0.0f) {
            Debug.Log("DIE DIE DIE !");
        }
    }
}
