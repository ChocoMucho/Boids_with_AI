using UnityEngine;

public class BoidSpawner : MonoBehaviour
{
    [SerializeField] int initialCount = 100;
    [SerializeField] Vector3 areaSize = new Vector3(20, 20, 20);

    void Start()
    {
        for (int i = 0; i < initialCount; i++)
        {
            Vector3 pos = transform.position + new Vector3(
                Random.Range(-areaSize.x * 0.5f, areaSize.x * 0.5f),
                Random.Range(-areaSize.y * 0.5f, areaSize.y * 0.5f),
                Random.Range(-areaSize.z * 0.5f, areaSize.z * 0.5f)
            );
            Vector3 dir = Random.onUnitSphere;
            if(dir == Vector3.zero) dir = Vector3.forward;
            Quaternion rot = Quaternion.LookRotation(dir);
            BoidsManager.Instance.CreateBoid(pos, rot);
        }
    }
}
