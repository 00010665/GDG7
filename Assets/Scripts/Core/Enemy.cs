using UnityEngine;
using UnityEngine.SceneManagement;

public class Enemy : MonoBehaviour
{
    [Header("Enemy Data")]
    public string id;
    public string name;
    public int hp;
    public float speed;
    public int damage;
    public Color color;

    [Header("Visual")]
    public GameObject prefab;
    public Transform spawnPoint;

    private bool isActive = false;
    private bool isDead = false;
    private float moveTime = 0f;
    private float moveDuration = 0f;
    private Vector3 targetPosition;

    public EnemyData GetData()
    {
        return new EnemyData
        {
            id = id,
            name = name,
            hp = hp,
            speed = speed,
            damage = damage,
            color = color
        };
    }

    public void Activate()
    {
        isActive = true;
        isDead = false;
        moveTime = 0f;
        moveDuration = 0f;
    }

    public void StartMove(Vector3 target)
    {
        targetPosition = target;
        moveTime = 0f;
        moveDuration = 1f / speed;
    }

    public void TakeDamage(int damage)
    {
        if (isDead)
        {
            return;
        }

        hp -= damage;
        UpdateVisuals();

        if (hp <= 0)
        {
            Die();
        }
    }

    public void Die()
    {
        isDead = true;
        isActive = false;

        // Reward player
        GameManager.Instance.OnEnemyDefeated(this);
    }

    public void Update()
    {
        if (!isActive || isDead)
        {
            return;
        }

        if (moveTime < moveDuration)
        {
            moveTime += Time.deltaTime;
            Vector3 direction = (targetPosition - transform.position).normalized;
            transform.position += direction * speed * Time.deltaTime;
        }
        else
        {
            // Enemy reached target
            // Trigger attack or other behavior
        }
    }

    private void UpdateVisuals()
    {
        // Update visual effects
    }

    private void Start()
    {
        if (prefab != null)
        {
            GameObject enemy = Instantiate(prefab, spawnPoint.position, Quaternion.identity);
            enemy.GetComponent<Enemy>().SetData(GetData());
        }
    }

    private void SetData(EnemyData data)
    {
        // Set enemy data
    }
}