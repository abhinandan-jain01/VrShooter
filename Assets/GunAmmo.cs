using UnityEngine;
using UnityEngine.InputSystem;

public class GunAmmo : MonoBehaviour
{
    [Header("Ammo")]
    public int MagSize { get; private set; } = 40;
    public int MagAmmo { get; private set; }
    public int ReserveAmmo { get; private set; }

    [Header("Reload")]
    public float reloadTime = 1.1f;
    public bool isReloading { get; private set; }

    [Header("Inputs")]
    public InputActionProperty xrReloadAction; // bind to XRI RightHand "Select" or "PrimaryButton" etc.
    public KeyCode reloadKey = KeyCode.R; // you can change to T if R used for restart

    void OnEnable()
    {
        if (xrReloadAction.action != null) xrReloadAction.action.Enable();
    }

    void OnDisable()
    {
        if (xrReloadAction.action != null) xrReloadAction.action.Disable();
    }

    public void Configure(int magSize)
    {
        MagSize = Mathf.Max(1, magSize);
    }

    // Called by GameManager when a wave starts
    public void SetWaveAmmo(int totalAmmo)
    {
        totalAmmo = Mathf.Max(0, totalAmmo);
        MagAmmo = Mathf.Min(MagSize, totalAmmo);
        ReserveAmmo = Mathf.Max(0, totalAmmo - MagAmmo);
        isReloading = false;
    }

    void Update()
    {
        if (GameManager.I != null && (GameManager.I.IsLost || GameManager.I.IsWon)) return;

        bool reloadPressed = Input.GetKeyDown(reloadKey);

        if (!reloadPressed && xrReloadAction.action != null)
        {
            if (xrReloadAction.action.ReadValue<float>() > 0.8f)
                reloadPressed = true;
        }

        if (reloadPressed)
        {
            TryReload();
        }
    }

    public bool CanShoot()
    {
        if (isReloading) return false;
        if (MagAmmo > 0) return true;

        // If mag empty and reserve exists, auto reload suggestion
        if (ReserveAmmo > 0) return false;

        // Out of ammo => lose
        if (GameManager.I) GameManager.I.OutOfAmmoLose();
        return false;
    }

    public void ConsumeBullet()
    {
        if (MagAmmo <= 0) return;
        MagAmmo--;

        // If total ammo ends completely after this shot => lose (your rule)
        if (MagAmmo == 0 && ReserveAmmo == 0)
        {
            if (GameManager.I) GameManager.I.OutOfAmmoLose();
        }
    }

    public void TryReload()
    {
        if (isReloading) return;
        if (ReserveAmmo <= 0) return;
        if (MagAmmo == MagSize) return;

        StartCoroutine(ReloadRoutine());
    }

    System.Collections.IEnumerator ReloadRoutine()
    {
        isReloading = true;
        yield return new WaitForSeconds(reloadTime);

        int needed = MagSize - MagAmmo;
        int take = Mathf.Min(needed, ReserveAmmo);
        ReserveAmmo -= take;
        MagAmmo += take;

        isReloading = false;
    }
}
