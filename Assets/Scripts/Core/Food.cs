using UnityEngine;
using UnityEngine.SceneManagement;

public class Food : MonoBehaviour
{
    [Header("Food Data")]
    public string id;
    public string name;
    public int damage;
    public float speed;
    public string effect;
    public string description;
    public Color color;

    [Header("Visual")]
    public GameObject prefab;
    public Transform spawnPoint;

    private bool isActive = false;
    private bool isThrown = false;
    private float throwTime = 0f;
    private float throwDuration = 0f;

    public FoodData GetData()
    {
        return new FoodData
        {
            id = id,
            name = name,
            damage = damage,
            speed = speed,
            effect = effect,
            description = description,
            color = color
        };
    }

    public void Activate()
    {
        isActive = true;
        isThrown = false;
        throwTime = 0f;
        throwDuration = 0f;
    }

    public void Throw()
    {
        if (!isActive || isThrown)
        {
            return;
        }

        isThrown = true;
        throwTime = 0f;
        throwDuration = 1f / speed;

        // Spawn projectile
        if (prefab != null)
        {
            GameObject projectile = Instantiate(prefab, spawnPoint.position, Quaternion.identity);
            projectile.GetComponent<Projectile>().SetData(GetData());
        }
    }

    public void UpdateProjectile()
    {
        if (!isThrown)
        {
            return;
        }

        throwTime += Time.deltaTime;

        if (throwTime >= throwDuration)
        {
            isThrown = false;
            isActive = false;
        }
    }

    public void Update()
    {
        UpdateProjectile();
    }

    public void Destroy()
    {
        isActive = false;
        isThrown = false;
    }
}