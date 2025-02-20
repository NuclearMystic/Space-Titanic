using UnityEngine;

public class BreakableObject : MonoBehaviour
{
    public enum BreakableType { Furnace, Boiler, Gravity }
    public BreakableType objectType;

    public int health = 10;
    public GameObject normalPrefab;
    public GameObject brokenPrefab;
    public bool isBroken = false;

    private void Start()
    {
        // Ensure the object starts in its normal state
        SetBrokenState(false);
    }

    public void AddDamage(int amount)
    {
        if (isBroken) return;

        health -= amount;
        if (health <= 0)
        {
            Break();
        }
    }

    private void Break()
    {
        isBroken = true;
        gameObject.tag = "Broken";
        SetBrokenState(true);
    }

    private void SetBrokenState(bool broken)
    {
        gameObject.tag = broken ? "Broken" : "BreakableObject";
        if (normalPrefab != null && brokenPrefab != null)
        {
            normalPrefab.SetActive(!broken);
            brokenPrefab.SetActive(broken);
        }
    }
}