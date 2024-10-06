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

    private Vector3 spawnPoint;

    [SerializeField]
    private bool patrol = true;

    [SerializeField]
    private bool followPlayer = true;
    [SerializeField]
    private bool shootBursts = false;
    private int burstCounter = 0;

    private Vector3 initialForward;

    private AudioSource audioSource;

    [SerializeField]
    private bool isObjective;

    [SerializeField]
    private GameObject minimapIcon;

    [SerializeField]
    private GameObject ingameIcon;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        rb = GetComponent<Rigidbody>();
        anim = GetComponentInChildren<Animator>();
        EnableNavigation();
        navigationTarget.parent = null;
        health = maxHealth;
        trooper = anim.gameObject;
        player = GameObject.FindWithTag("Player");
        lookForPlayerMask = LayerMask.GetMask("Player", "Wall", "Default");
        spawnPoint = transform.position;
        if (patrol) {
            state = EnemyState.PATROL;
            RandomizePatrol();
        }
        if (!followPlayer) {
            DisableNavigation();
        }
        initialForward = transform.forward;
        burstCounter = Random.Range(3, 6);

        if (isObjective) {
            GameManager.Instance.QuestEnemiesTotal++;
            minimapIcon.SetActive(true);
            ingameIcon.SetActive(true);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (GameManager.Instance.BriefingActive) return;

        handlePlayerVisibility();

        var targetDir = Vector3.zero;
        if ((patrol || followPlayer) && rb.linearVelocity.magnitude > 0.1f) {
            targetDir = rb.linearVelocity;
        }
        if (targetDir.magnitude < 0.1f) {
            if (state == EnemyState.ATTACK) {
                var dirToPlayer = player.transform.position - transform.position;
                var angleToPlayer = Vector3.SignedAngle(initialForward, dirToPlayer, Vector3.up);
                if (followPlayer) {
                    targetDir = dirToPlayer;
                } else {
                    if (angleToPlayer > -60f && angleToPlayer < 60f) {
                        targetDir = dirToPlayer;
                    } else {
                        targetDir = Quaternion.AngleAxis(Mathf.Sign(angleToPlayer) * 60f, Vector3.up) * initialForward;
                    }
                }
            }
        }

        targetDir.y = 0.0f;
        if (targetDir.magnitude > 0.1f) {
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
        if (angle < 75 || playerDir.magnitude < 1.5f) {
            RaycastHit hit;
            if (Physics.Raycast(transform.position + Vector3.up * 0.75f, playerDir, out hit, 6.0f, lookForPlayerMask)) {
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
        moveSpeed = 2.0f;
        if (playerVisibility >= 1.0f) {
            state = EnemyState.ATTACK;
        }
    }

    public void RandomizePatrol() {
        if (state != EnemyState.PATROL) return;
        var randomPoint = Random.insideUnitCircle;
        var offset = new Vector3(randomPoint.x, spawnPoint.y, randomPoint.y);
        navigationTarget.position = spawnPoint + offset * 10.0f;
        Invoke("RandomizePatrol", Random.Range(3.0f, 6.0f));
    }

    public void handleAttack() {
        moveSpeed = 4.0f;
        if (followPlayer) {
            navigationTarget = player.transform;
        }
        targetRange = 3.0f;

        var angleToPlayer = Vector3.Angle(transform.forward, player.transform.position - transform.position);
        if (playerVisibility >= 1.0f && angleToPlayer < 15.0f) {
            shoot();
        }
    }

    private void shoot() {
        if (shootTimer <= Time.time) {
            var fireRate = shootBursts ? gun.FireRate : Random.Range(gun.FireRate * 0.8f, gun.FireRate);
            shootTimer = Time.time + 60.0f / fireRate;
            var bullet = Instantiate(bulletPrefab);
            bullet.transform.position = muzzle.position;
            var inaccuracy = new Vector3(Random.Range(-1.0f, 1.0f), Random.Range(-0.5f, 0.5f), Random.Range(-1.0f, 1.0f));
            bullet.transform.forward = transform.forward * 10 + inaccuracy;
            bullet.Init(gun);
            audioSource.PlayOneShot(gun.Sound);
            if (shootBursts) {
                burstCounter--;
                if (burstCounter <= 0) {
                    shootTimer = Time.time + Random.Range(0.5f, 1.0f);
                    burstCounter = Random.Range(3, 6);
                }
            }
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
        if (isObjective) {
            GameManager.Instance.QuestEnemiesKilled++;
            player.GetComponent<Player>().ObjectiveCompleted();
        }
        DisableNavigation();
        var fx = Instantiate(dieEffect);
        fx.transform.parent = null;
        fx.transform.position = transform.position;
        Destroy(gameObject);
        dead = true;
    }

    void FixedUpdate()
    {
        if (rb.linearVelocity.magnitude > 0.1f) {
            anim.SetBool("run", true);
        } else {
            anim.SetBool("run", false);
        }

        if (GameManager.Instance.BriefingActive) return;

        if (!navigationActive)
        {
            anim.SetBool("run", false);
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
        if (path == null) return true;
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
