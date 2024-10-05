using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    [SerializeField]
    private Transform navigationTarget;
    private NavMeshPath path;
    private int waypointIndex = 0;
    private bool navigationActive = true;
    private float navigationUpdateInterval = 0.2f;
    private float waypointDistanceCheckEpsilon = 0.1f;

    private Rigidbody rb;
    private float moveSpeed = 4.0f;
    private float targetRange = 0.0f;
    private Animator anim;

    [SerializeField]
    private float maxHealth = 5;
    private float health = 5;

    [SerializeField]
    private GameObject dieEffect;
    private GameObject trooper;
    private bool dead = false;
    private float turnSpeed = 720;

    private GameObject player;

    private EnemyState state;

    private int lookForPlayerMask;
    private float playerVisibility = 0;

    [SerializeField]
    private Gun gun;

    [SerializeField]
    private Bullet bulletPrefab;

    private float shootTimer = 0;

    [SerializeField]
    private Transform muzzle;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        anim = GetComponentInChildren<Animator>();
        EnableNavigation();
        navigationTarget.parent = null;
        health = maxHealth;
        trooper = anim.gameObject;
        player = GameObject.FindWithTag("Player");
        lookForPlayerMask = LayerMask.GetMask("Player", "Wall", "Default");
    }

    // Update is called once per frame
    void Update()
    {
        handlePlayerVisibility();

        var targetDir = rb.linearVelocity.normalized;
        targetDir.y = 0.0f;
        if (targetDir.magnitude > 0.5f) {
            var targetRotation = Quaternion.LookRotation(targetDir, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, Time.deltaTime * turnSpeed);
        }

        switch(state) {
            case EnemyState.IDLE:
                handleIdle();
                break;
            case EnemyState.PATROL:
                handlePatrol();
                break;
            case EnemyState.ATTACK:
                handleAttack();
                break;
        }
    }

    private void handlePlayerVisibility() {
        float visibility = 0.0f;
        var playerDir = player.transform.position - transform.position;
        var angle = Vector3.Angle(transform.forward, playerDir);
        if (angle < 60 || playerDir.magnitude < 1.5f) {
            RaycastHit hit;
            if (Physics.Raycast(transform.position + Vector3.up * 0.5f, playerDir, out hit, 5.0f, lookForPlayerMask)) {
                if (hit.rigidbody != null && hit.rigidbody.gameObject == player) {
                    visibility = 10.0f;
                }
            }
        }
        if (playerVisibility < visibility) {
            playerVisibility = visibility;
        } else {
            playerVisibility -= Time.deltaTime * 1.0f;
        }
    }

    public void handleIdle() {
        if (playerVisibility >= 1.0f) {
            state = EnemyState.ATTACK;
        }
    }

    public void handlePatrol() {
        
    }

    public void handleAttack() {
        navigationTarget = player.transform;
        targetRange = 3.0f;

        if (playerVisibility >= 1.0f) {
            shoot();
        }
    }

    private void shoot() {
        if (shootTimer <= Time.time) {
            shootTimer = Time.time + 60.0f / gun.FireRate;
            var bullet = Instantiate(bulletPrefab);
            bullet.transform.position = muzzle.position;
            bullet.transform.forward = transform.forward;
            bullet.Init(gun);
        }
    }

    public void Hurt(float damage) {
        playerVisibility = 10.0f;
        health -= damage;
        if (health <= 0) {
            Die();
        }
    }

    public void Die() {
        if (dead) return;
        DisableNavigation();
        var fx = Instantiate(dieEffect);
        fx.transform.parent = null;
        fx.transform.position = transform.position;
        Destroy(gameObject);
        dead = true;
    }

    void FixedUpdate()
    {
        if (!navigationActive)
        {
            return;
        }

        runTowardsWaypoint();

        if (isFinalWaypoint() && GetDistanceToTarget() < targetRange)
        {
            rb.linearVelocity = Vector3.zero;
        }

        if (isFinalWaypoint() && GetDistanceToTarget() < 0.1f)
        {
            rb.linearVelocity = Vector3.zero;
        }

        if (rb.linearVelocity.magnitude > 0.1f) {
            anim.SetBool("run", true);
        } else {
            anim.SetBool("run", false);
        }
    }

    public float GetDistanceToTarget()
    {
        if (path == null || path.corners == null || path.corners.Length == 0)
        {
            return float.MaxValue;
        }
        var distanceSum = 0.0f;
        var nextWaypoint = waypointIndex;
        distanceSum += Vector3.Distance(transform.position, path.corners[nextWaypoint]);
        nextWaypoint++;
        while (nextWaypoint < path.corners.Length)
        {
            distanceSum += Vector3.Distance(path.corners[nextWaypoint - 1], path.corners[nextWaypoint]);
            nextWaypoint++;
        }
        return distanceSum;
    }
    
    private void updatePathing()
    {
        NavMeshPath newPath = new NavMeshPath();
        var sourcePos = transform.position;
        var targetPos = navigationTarget.position;
        var success = NavMesh.CalculatePath(sourcePos, targetPos, NavMesh.AllAreas, newPath);
        if (success)
        {
            path = newPath;
            waypointIndex = 0;
        }

        if (navigationActive)
        {
            Invoke("updatePathing", navigationUpdateInterval);
        }
    }

    public void EnableNavigation()
    {
        navigationActive = true;
        updatePathing();
    }

    public void DisableNavigation()
    {
        navigationActive = false;
    }

    private void runTowardsWaypoint()
    {
        if (path == null || path.corners == null || path.corners.Length == 0)
        {
            return;
        }
        updateWayPoint();

        var targetPosition = path.corners[waypointIndex];
        if (targetPosition != null)
        {
            rb.linearVelocity = (targetPosition - transform.position).normalized * moveSpeed;
        }
        else
        {
            rb.linearVelocity = Vector3.zero;
        }
    }

    private void updateWayPoint()
    {
        var targetPosition = path.corners[waypointIndex];
        if (targetPosition != null)
        {
            if (waypointReached() && !isFinalWaypoint())
            {
                waypointIndex++;
            }
        }
    }

    private bool waypointReached()
    {
        var targetPosition = path.corners[waypointIndex];
        return Vector2.Distance(transform.position, targetPosition) < waypointDistanceCheckEpsilon;
    }

    private bool isFinalWaypoint()
    {
        return waypointIndex == path.corners.Length - 1;
    }

    public bool HasLOSToTarget()
    {
        if (path == null || path.corners == null || path.corners.Length == 0)
        {
            return false;
        }
        return isFinalWaypoint();
    }
}

public enum EnemyState {
    IDLE,
    PATROL,
    ATTACK
}
