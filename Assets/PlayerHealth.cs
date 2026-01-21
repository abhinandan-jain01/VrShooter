using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public int maxHp = 100;
    public int hp { get; private set; }
    public bool isDead { get; private set; }

    void Awake()
    {
        hp = maxHp;
        isDead = false;
    }

    public void TakeDamage(int amount)
    {
        if (isDead) return;

        hp -= Mathf.Abs(amount);

        if (GameManager.I) GameManager.I.FlashDamage();

        if (hp <= 0)
        {
            hp = 0;
            isDead = true;
            if (GameManager.I) GameManager.I.Lose();
        }
    }

    public void ForceDead()
    {
        hp = 0;
        isDead = true;
    }

    public void ResetHealth()
    {
        hp = maxHp;
        isDead = false;
    }
}
