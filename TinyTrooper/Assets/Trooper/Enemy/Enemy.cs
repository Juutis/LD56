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

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        anim = GetComponentInChildren<Animator>();
        EnableNavigation();
        navigationTarget.parent = null;
    }

    // Update is called once per frame
    void Update()
    {
        
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


        if (rb.linearVelocity.magnitude > 0.01f) {
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
}
