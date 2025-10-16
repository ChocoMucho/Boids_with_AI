using System.Collections.Generic;
using UnityEngine;

public class Boid : MonoBehaviour
{
    [SerializeField] private float perceptionRadius = 3.0f;
    [SerializeField] private float maxSpeed = 5.0f;
    [SerializeField] private float maxForce = 0.5f;
    [SerializeField] private float turnSpeed = 6.0f;

    private BoidsManager manager;
    private readonly List<Boid> neighbors = new List<Boid>();
    private Vector3 currentDirection = Vector3.zero;

    public void Initialize(BoidsManager manager)
    {
        this.manager = manager;
        currentDirection = transform.forward;
        Vector3.Normalize(currentDirection);
    }

    private void Start()
    {
        if (manager == null && BoidsManager.Instance != null)
        {
            Initialize(BoidsManager.Instance);
            BoidsManager.Instance.RegisterBoid(this);
        }
        if (currentDirection == Vector3.zero)
        {
            currentDirection = transform.forward;
        }
    }

    private void Update()
    {
        Vector3 separation = ComputeSeparation();
        Vector3 alignment = ComputeAlignment();
        Vector3 cohesion = ComputeCohesion();

        Vector3 desired = CombineSteering(separation, alignment, cohesion);
        ApplySteering(desired);
        UpdateRotation();
        UpdatePosition();
        CheckWrapInSphere();
    }

    public void RefreshNeighbors(IReadOnlyList<Boid> allBoids)
    {
        neighbors.Clear();
        Vector3 myPos = transform.position;
        for (int i = 0; i < allBoids.Count; i++)
        {
            var other = allBoids[i];
            if (other == null || other == this) continue;
            float dist = Vector3.Distance(myPos, other.transform.position);
            if (dist <= perceptionRadius)
            {
                neighbors.Add(other);
            }
        }
    }

    private Vector3 ComputeSeparation()
    {
        if (neighbors.Count == 0) return Vector3.zero;
        Vector3 sum = Vector3.zero;
        Vector3 myPos = transform.position;
        for (int i = 0; i < neighbors.Count; i++)
        {
            Vector3 away = myPos - neighbors[i].transform.position;
            sum += away;
        }
        if (sum != Vector3.zero)
        {
            sum = sum.normalized * maxSpeed - currentDirection * maxSpeed;
            if (sum.magnitude > maxForce)
            {
                sum = sum.normalized * maxForce;
            }
        }
        return sum;
    }

    private Vector3 ComputeAlignment()
    {
        if (neighbors.Count == 0) return Vector3.zero;
        Vector3 sum = Vector3.zero;
        for (int i = 0; i < neighbors.Count; i++)
        {
            sum += neighbors[i].transform.forward;
        }
        Vector3 avg = sum / neighbors.Count;
        if (avg != Vector3.zero)
        {
            avg = avg.normalized * maxSpeed - currentDirection * maxSpeed;
            if (avg.magnitude > maxForce)
            {
                avg = avg.normalized * maxForce;
            }
        }
        return avg;
    }

    private Vector3 ComputeCohesion()
    {
        if (neighbors.Count == 0) return Vector3.zero;
        Vector3 sum = Vector3.zero;
        for (int i = 0; i < neighbors.Count; i++)
        {
            sum += neighbors[i].transform.position;
        }
        Vector3 center = sum / neighbors.Count;
        Vector3 desiredDir = (center - transform.position);
        if (desiredDir != Vector3.zero)
        {
            desiredDir = desiredDir.normalized * maxSpeed - currentDirection * maxSpeed;
            if (desiredDir.magnitude > maxForce)
            {
                desiredDir = desiredDir.normalized * maxForce;
            }
        }
        return desiredDir;
    }

    private Vector3 CombineSteering(Vector3 separation, Vector3 alignment, Vector3 cohesion)
    {
        float wS = manager != null ? manager.SeparationWeight : 1.0f;
        float wA = manager != null ? manager.AlignWeight : 1.0f;
        float wC = manager != null ? manager.CohesionWeight : 1.0f;

        Vector3 steering = separation * wS + alignment * wA + cohesion * wC;
        if (steering == Vector3.zero)
        {
            return currentDirection;
        }
        Vector3 desiredVelocity = currentDirection * maxSpeed + steering;
        if (desiredVelocity != Vector3.zero)
        {
            return desiredVelocity.normalized;
        }
        return currentDirection;
    }

    private void ApplySteering(Vector3 desiredDirection)
    {
        if (desiredDirection == Vector3.zero) return;
        currentDirection = Vector3.Slerp(currentDirection, desiredDirection, Time.deltaTime * turnSpeed);
        Vector3.Normalize(currentDirection);
    }

    private void UpdateRotation()
    {
        if (currentDirection != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(currentDirection);
        }
    }

    private void UpdatePosition()
    {
        transform.position += transform.forward * maxSpeed * Time.deltaTime;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0f, 0.6f, 1f, 0.15f);
        Gizmos.DrawSphere(transform.position, perceptionRadius);
    }

    private void CheckWrapInSphere()
    {
        if (manager == null) return;
        float r = manager.MoveSphereRange;
        if (r <= 0f) return;
        Vector3 p = transform.position;
        if (p.sqrMagnitude > r * r)
        {
            transform.position = -p;
        }
    }
}
