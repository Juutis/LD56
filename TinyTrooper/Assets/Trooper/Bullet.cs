using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    private Rigidbody rb;
    private float speed = 20.0f;
    private Gun gun;
    private int penetration = 1;

    [SerializeField]
    private LayerMask mask;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = false;
        rb.useGravity = false;
        rb.position = transform.position;
        var additionalComponent = gun.BulletGravity > 0.001f ? Vector3.up : Vector3.zero;
        rb.linearVelocity = transform.forward * gun.BulletSpeed + additionalComponent;
        penetration = gun.Penetration;
        Invoke("Kill", 10.0f);
    }

    public void Init(Gun gun) {
        this.gun = gun;
    }

    // Update is called once per frame
    void Update()
    {
        if (transform.position.y < -1.0f) {
            Kill();
        }

        var result = Physics.RaycastAll(transform.position, rb.linearVelocity, rb.linearVelocity.magnitude * Time.deltaTime, mask);
        foreach(var hit in result) {
            ProcessHit(hit);
        }
        
    }

    void FixedUpdate() {
        rb.AddForce(Physics.gravity * gun.BulletGravity, ForceMode.Acceleration);
    }

    void ProcessHit(RaycastHit hit) {
        var other = hit.collider;
        DestroyableObject obj = other.GetComponentInParent<DestroyableObject>();
        if (obj != null) {
            obj.Hurt(getDamage());
            penetration -= obj.Hardness;
        } else {
            penetration = 0;
        }
        if (penetration <= 0) {
            Kill();
        }
    }

    private float getDamage() {
        return gun.Damage * penetration / gun.Penetration;
    }

    public void Kill() {
        Destroy(gameObject);
    }
}
