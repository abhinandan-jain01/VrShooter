using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    public static GameManager I { get; private set; }

    [Header("Refs")]
    public PlayerHealth playerHealth;
    public GunAmmo gunAmmo;

    [Header("UI Text (TMP)")]
    public TMP_Text hpText;
    public TMP_Text waveText;
    public TMP_Text aliveText;
    public TMP_Text scoreText;
    public TMP_Text ammoText;
    public TMP_Text timerText;

    [Header("Panels")]
    public GameObject damagePanel;   // full-screen red overlay (Canvas child)
    public GameObject gameOverPanel; // GAME OVER window/panel
    public GameObject winPanel;      // WIN window/panel

    [Header("Damage Overlay")]
    public float damageFlashDuration = 0.18f;

    [Header("Restart Inputs")]
    public KeyCode restartKey = KeyCode.R;
    public InputActionProperty xrRestartAction; // bind later to any XRI button (menu/primary)

    [Header("Wave Timer")]
    public float waveDurationSeconds = 45f;

    [Header("Wave Ammo (per wave total)")]
    public int magSize = 40;
    public int wave1TotalAmmo = 120;
    public int wave2TotalAmmo = 140;
    public int wave3TotalAmmo = 160;

    public int Score { get; private set; }
    public int CurrentWave { get; private set; }
    public int AliveEnemies { get; private set; }
    public bool IsWon { get; private set; }
    public bool IsLost { get; private set; }

    float waveTimeLeft;
    bool waveRunning;
    float damageFlashTimer;

    void Awake()
    {
        if (I != null && I != this) { Destroy(gameObject); return; }
        I = this;
        Time.timeScale = 1f;
    }

    void OnEnable()
    {
        if (xrRestartAction.action != null) xrRestartAction.action.Enable();
    }

    void OnDisable()
    {
        if (xrRestartAction.action != null) xrRestartAction.action.Disable();
    }

    void Start()
    {
        if (!playerHealth) playerHealth = FindObjectOfType<PlayerHealth>();
        if (!gunAmmo) gunAmmo = FindObjectOfType<GunAmmo>();

        ResetRun();
        RefreshUI();
        SetPanels();
    }

    void Update()
    {
        // Damage flash timer
        if (damagePanel)
        {
            if (damageFlashTimer > 0f) damageFlashTimer -= Time.unscaledDeltaTime;
            damagePanel.SetActive(damageFlashTimer > 0f);
        }

        // Timer (use unscaled? no, only while running)
        if (waveRunning && !IsWon && !IsLost)
        {
            waveTimeLeft -= Time.deltaTime;
            if (waveTimeLeft <= 0f)
            {
                waveTimeLeft = 0f;
                Lose(); // time over => lose
            }
        }

        RefreshUI();
        SetPanels();

        // Restart (PC + XR) when ended
        if (IsWon || IsLost)
        {
            bool restartPressed = Input.GetKeyDown(restartKey);

            if (!restartPressed && xrRestartAction.action != null)
            {
                // Works for both float/button actions
                float v = 0f;
                try { v = xrRestartAction.action.ReadValue<float>(); } catch { /* ignore */ }
                if (v > 0.8f) restartPressed = true;
            }

            if (restartPressed)
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    public void ResetRun()
    {
        Score = 0;
        CurrentWave = 0;
        AliveEnemies = 0;
        IsWon = false;
        IsLost = false;
        waveRunning = false;
        waveTimeLeft = waveDurationSeconds;
        damageFlashTimer = 0f;

        Time.timeScale = 1f;

        if (gameOverPanel) gameOverPanel.SetActive(false);
        if (winPanel) winPanel.SetActive(false);
        if (damagePanel) damagePanel.SetActive(false);

        RefreshUI();
    }

    public void FlashDamage()
    {
        damageFlashTimer = damageFlashDuration;
    }

    public void SetWave(int wave)
    {
        CurrentWave = wave;

        waveTimeLeft = waveDurationSeconds;
        waveRunning = true;

        // Give ammo per wave
        int totalAmmo = (wave == 1) ? wave1TotalAmmo : (wave == 2) ? wave2TotalAmmo : wave3TotalAmmo;
        if (gunAmmo)
        {
            gunAmmo.Configure(magSize);
            gunAmmo.SetWaveAmmo(totalAmmo);
        }

        RefreshUI();
    }

    public void AddScore(int amount)
    {
        if (IsLost || IsWon) return;
        Score += amount;
        RefreshUI();
    }

    public void AddAlive(int delta)
    {
        AliveEnemies = Mathf.Max(0, AliveEnemies + delta);
        RefreshUI();
    }

    public void Win()
    {
        if (IsWon || IsLost) return;
        IsWon = true;
        waveRunning = false;

        StopEverything();
        RefreshUI();
        SetPanels();
    }

    public void Lose()
    {
        if (IsWon || IsLost) return;
        IsLost = true;
        waveRunning = false;

        StopEverything();
        RefreshUI();
        SetPanels();
    }

    public void OutOfAmmoLose()
    {
        Lose();
    }

    void StopEverything()
    {
        // Freeze whole game
        Time.timeScale = 0f;

        // Ensure player marked dead
        if (playerHealth && !playerHealth.isDead) playerHealth.ForceDead();
    }

    void SetPanels()
    {
        if (gameOverPanel) gameOverPanel.SetActive(IsLost);
        if (winPanel) winPanel.SetActive(IsWon);
    }

    void RefreshUI()
    {
        if (playerHealth && hpText) hpText.text = $"HP: {playerHealth.hp}/{playerHealth.maxHp}";
        if (waveText) waveText.text = $"Wave: {Mathf.Max(1, CurrentWave)}";
        if (aliveText) aliveText.text = $"Alive: {AliveEnemies}";
        if (scoreText) scoreText.text = $"Score: {Score}";

        if (ammoText && gunAmmo) ammoText.text = $"Ammo: {gunAmmo.MagAmmo}/{gunAmmo.ReserveAmmo}";

        if (timerText)
        {
            int t = Mathf.CeilToInt(waveTimeLeft);
            timerText.text = $"Time: {t}";
        }
    }
}
