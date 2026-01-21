using UnityEngine;
using System;
using Random = UnityEngine.Random;

public class EnemyBase : MonoBehaviour, IDamageable
{
    public enum MovementType { Drone, Runner }

    [Header("Type")]
    public MovementType movementType = MovementType.Drone;

    [Header("Stats")]
    public int maxHp = 2;
    public float baseSpeed = 1.6f;

    [Header("Damage on Touch")]
    public int touchDamagePerSecond = 15;
    public float touchRange = 1.2f;

    [Header("FX")]
    public float hitFlashTime = 0.06f;

    public static event Action<EnemyBase> OnAnyEnemyDied;

    int hp;
    Transform target;
    PlayerHealth playerHealth;

    Renderer rend;
    Color originalColor;

    // Movement state
    Vector3 runnerSideDir;
    float runnerZigTimer;
    float runnerZigInterval;

    Vector3 droneSideDir;
    float dronePhase;
    float droneRetargetTimer;

    void Awake()
    {
        hp = maxHp;
        rend = GetComponentInChildren<Renderer>();
        if (rend) originalColor = rend.material.color;
    }

    void Start()
    {
        target = Camera.main ? Camera.main.transform : null;
        playerHealth = FindObjectOfType<PlayerHealth>();

        // Runner params
        runnerSideDir = (Random.value < 0.5f) ? Vector3.left : Vector3.right;
        runnerZigInterval = Random.Range(0.25f, 0.45f);
        runnerZigTimer = Random.Range(0f, runnerZigInterval);

        // Drone params
        droneSideDir = Random.onUnitSphere; droneSideDir.y = 0;
        droneSideDir.Normalize();
        dronePhase = Random.Range(0f, 1000f);
        droneRetargetTimer = Random.Range(0.4f, 1.0f);
    }

    void Update()
    {
        if (!target) return;
        if (playerHealth && playerHealth.isDead) return; // stop chasing after death

        Vector3 toTarget = target.position - transform.position;
        toTarget.y = 0f;

        Vector3 forward = toTarget.sqrMagnitude > 0.001f ? toTarget.normalized : transform.forward;

        Vector3 move = (movementType == MovementType.Runner) ? RunnerMove(forward) : DroneMove(forward);
        transform.position += move * Time.deltaTime;

        // face target (upright)
        Vector3 lookPos = new Vector3(target.position.x, transform.position.y, target.position.z);
        transform.LookAt(lookPos);

        HandleTouchDamage();
    }

    Vector3 RunnerMove(Vector3 forward)
    {
        runnerZigTimer += Time.deltaTime;
        if (runnerZigTimer >= runnerZigInterval)
        {
            runnerZigTimer = 0f;
            runnerZigInterval = Random.Range(0.22f, 0.4f);
            runnerSideDir = -runnerSideDir;
        }

        float sideStrength = 0.85f;
        Vector3 side = runnerSideDir * sideStrength;

        float jitter = Mathf.Sin(Time.time * 8.5f + transform.GetInstanceID()) * 0.12f;

        Vector3 dir = (forward + side + (transform.right * jitter)).normalized;
        float speed = baseSpeed * 1.25f;
        return dir * speed;
    }

    Vector3 DroneMove(Vector3 forward)
    {
        droneRetargetTimer -= Time.deltaTime;
        if (droneRetargetTimer <= 0f)
        {
            droneRetargetTimer = Random.Range(0.45f, 1.1f);
            droneSideDir = Random.insideUnitSphere; droneSideDir.y = 0;
            if (droneSideDir.sqrMagnitude < 0.01f) droneSideDir = transform.right;
            droneSideDir.Normalize();
        }

        float wave = Mathf.Sin(Time.time * 2.6f + dronePhase);
        Vector3 lateral = droneSideDir * (0.55f * wave);

        float bob = Mathf.Sin(Time.time * 3.2f + dronePhase) * 0.25f;

        Vector3 dir = (forward * 0.9f + lateral).normalized;
        float speed = baseSpeed * 0.95f;

        Vector3 move = dir * speed;
        move.y = bob;
        return move;
    }

    void HandleTouchDamage()
    {
        if (!playerHealth || playerHealth.isDead) return;

        float d = Vector3.Distance(transform.position, target.position);
        if (d < touchRange)
        {
            int dmg = Mathf.CeilToInt(touchDamagePerSecond * Time.deltaTime);
            playerHealth.TakeDamage(dmg);
        }
    }

    public void TakeDamage(int amount)
    {
        hp -= amount;

        if (rend)
        {
            rend.material.color = Color.white;
            CancelInvoke(nameof(ResetColor));
            Invoke(nameof(ResetColor), hitFlashTime);
        }

        if (hp <= 0) Die();
    }

    void ResetColor()
    {
        if (rend) rend.material.color = originalColor;
    }

    void Die()
    {
        if (GameManager.I) GameManager.I.AddScore(10);
        OnAnyEnemyDied?.Invoke(this);
        Destroy(gameObject);
    }
}

public interface IDamageable
{
    void TakeDamage(int amount);
}
