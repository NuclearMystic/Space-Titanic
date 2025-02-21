using System.Collections;
using UnityEngine;

public class BreakableObject : MonoBehaviour
{
    public enum BreakableType { Furnace, Boiler, Gravity }
    public enum ObjectState { Normal, Broken, Repairing }

    public BreakableType objectType;
    public ObjectState state = ObjectState.Normal;

    public int maxHealth = 10;
    private int health;
    public GameObject normalPrefab;
    public GameObject brokenPrefab;

    private bool isUnderAttack = false; // Tracks if a gremlin is actively attacking

    private void Start()
    {
        health = maxHealth;
        SetBrokenState(false);
        Debug.Log("Breakable object " + gameObject.name + " is Running!");
    }

    public void AddDamage(int amount)
    {
        if (state == ObjectState.Broken && health <= 0) return; // Prevent damage below 0

        health -= amount;
        isUnderAttack = true; // Flag this object as under attack

        if (health <= 0)
        {
            Break();
        }
    }

    private void Break()
    {
        if (state == ObjectState.Broken) return; // Prevent redundant calls

        state = ObjectState.Broken;
        isUnderAttack = false;
        SetBrokenState(true);

        // Activate Zero-G if this is the Gravity Generator
        if (objectType == BreakableType.Gravity)
        {
            ZeroGManager.Instance.ActivateZeroG();
        }
    }

    private void SetBrokenState(bool broken)
    {
        state = broken ? ObjectState.Broken : ObjectState.Normal;
        gameObject.tag = broken ? "Broken" : "BreakableObject";

        if (normalPrefab != null && brokenPrefab != null)
        {
            normalPrefab.SetActive(!broken);
            brokenPrefab.SetActive(broken);
        }
    }

    public bool CanBeRepaired()
    {
        Debug.Log("DEBUG: Checking CanBeRepaired() on " + gameObject.name +
                  " | State: " + state +
                  " | isUnderAttack: " + isUnderAttack);

        return state == ObjectState.Broken && !isUnderAttack;
    }


    public void Restore()
    {
        if (state != ObjectState.Broken) return;

        Debug.Log("DEBUG: Restoring " + gameObject.name);
        health = maxHealth;
        isUnderAttack = false;
        state = ObjectState.Normal;
        SetBrokenState(false);

        // Deactivate Zero-G if this is the Gravity Generator
        if (objectType == BreakableType.Gravity)
        {
            ZeroGManager.Instance.DeactivateZeroG();
        }
    }

    public void NotifyGremlinLeft()
    {
        isUnderAttack = false; // Reset when gremlins leave
    }
}
