using System.Collections.Generic;
using UnityEngine;

public class BoidsManager : MonoBehaviour
{
    public static BoidsManager Instance { get; private set; }

    [SerializeField] private GameObject boidPrefab;
    [SerializeField] private int boidLimit = 100;

    [SerializeField] private float separationWeight = 1.0f;
    [SerializeField] private float alignWeight = 1.0f;
    [SerializeField] private float cohesionWeight = 1.0f;

    private readonly List<Boid> boids = new List<Boid>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Update()
    {
        RefreshAllNeighbors();
    }

    public Boid CreateBoid(Vector3 position, Quaternion rotation)
    {
        if (!CanSpawnMore() || boidPrefab == null) return null;
        GameObject go = Instantiate(boidPrefab, position, rotation);
        Boid boid = go.GetComponent<Boid>();
        if (boid != null)
        {
            boid.Initialize(this);
            RegisterBoid(boid);
        }
        return boid;
    }

    public void RegisterBoid(Boid boid)
    {
        if (boid == null) return;
        if (!boids.Contains(boid))
        {
            boids.Add(boid);
        }
    }

    public void UnregisterBoid(Boid boid)
    {
        if (boid == null) return;
        if (boids.Contains(boid))
        {
            boids.Remove(boid);
        }
    }

    public IReadOnlyList<Boid> GetBoids()
    {
        return boids;
    }

    public void GetRuleWeights(out float separation, out float alignment, out float cohesion)
    {
        separation = separationWeight;
        alignment = alignWeight;
        cohesion = cohesionWeight;
    }

    public bool CanSpawnMore()
    {
        return boids.Count < boidLimit;
    }

    public void RefreshAllNeighbors()
    {
        var cached = boids;
        for (int i = 0; i < cached.Count; i++)
        {
            var b = cached[i];
            if (b != null)
            {
                b.RefreshNeighbors(cached);
            }
        }
    }

    public float SeparationWeight => separationWeight;
    public float AlignWeight => alignWeight;
    public float CohesionWeight => cohesionWeight;
}
