using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class GunShootSimple : MonoBehaviour
{
    [Header("Refs")]
    public Transform muzzle;
    public LineRenderer line;
    public ParticleSystem impactFx;

    [Header("Shoot")]
    public float range = 30f;
    public int damage = 1;
    public float fireCooldown = 0.12f;

    [Header("Auto Aim")]
    public float autoAimDistance = 25f;
    public float autoAimMaxAngle = 8f;
    public LayerMask enemyMask;
    public GunAmmo ammo;

    float lastFire;
    void Start()
    {
        if (!ammo) ammo = GetComponent<GunAmmo>();

    }
    void Update()
    {
        if (GameManager.I && (GameManager.I.IsLost || GameManager.I.IsWon)) return;

        // Works immediately in Editor testing:
        bool fire = Mouse.current != null && Mouse.current.leftButton.isPressed;

        // Try XR trigger too (works when simulator/headset provides it):
        if (!fire)
        {
            var rightTrig = Gamepad.current?.rightTrigger.ReadValue() ?? 0f;
            fire = rightTrig > 0.8f;
        }

        if (fire) TryFire();
    }

    void TryFire()
    {
        if (ammo && !ammo.CanShoot()) return;
      


        if (Time.time - lastFire < fireCooldown) return;
        lastFire = Time.time;

        Vector3 origin = muzzle.position;
        Vector3 dir = GetAutoAimedDirection(origin, muzzle.forward);

        Vector3 end = origin + dir * range;

        if (Physics.Raycast(origin, dir, out RaycastHit hit, range))
        {
            end = hit.point;

            var dmg = hit.collider.GetComponentInParent<IDamageable>();
            if (dmg != null) dmg.TakeDamage(damage);

            if (impactFx)
            {
                impactFx.transform.position = hit.point;
                impactFx.transform.rotation = Quaternion.LookRotation(hit.normal);
                impactFx.Play();
            }
        }

        StartCoroutine(FlashLine(origin, end));
        if (ammo) ammo.ConsumeBullet();

    }

    Vector3 GetAutoAimedDirection(Vector3 origin, Vector3 forward)
    {
        Collider[] hits = Physics.OverlapSphere(origin, autoAimDistance, enemyMask);

        Transform best = null;
        float bestAngle = autoAimMaxAngle;

        foreach (var c in hits)
        {
            Vector3 to = (c.bounds.center - origin);
            float angle = Vector3.Angle(forward, to);
            if (angle < bestAngle)
            {
                bestAngle = angle;
                best = c.transform;
            }
        }

        if (best)
        {
            Vector3 toBest = (best.GetComponent<Collider>().bounds.center - origin).normalized;
            return Vector3.Slerp(forward.normalized, toBest, 0.65f).normalized;
        }

        return forward.normalized;
    }

    IEnumerator FlashLine(Vector3 start, Vector3 end)
    {

        if (!line) yield break;
        line.enabled = true;
        line.SetPosition(0, start);
        line.SetPosition(1, end);
        yield return new WaitForSeconds(0.03f);
        line.enabled = false;
    }
}

//public interface IDamageable { void TakeDamage(int amount); }
