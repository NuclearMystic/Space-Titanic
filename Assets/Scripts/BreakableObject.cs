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

    private bool isBeingRepaired = false;
    private bool isUnderAttack = false; // New: Tracks if a gremlin is actively attacking

    private void Start()
    {
        health = maxHealth;
        SetBrokenState(false);
    }

    public void AddDamage(int amount)
    {
        if (state == ObjectState.Broken && health <= 0) return; // Prevent damage below 0

        health -= amount;
        isUnderAttack = true; // Flag this object as under attack

        if (isBeingRepaired) // Stop repair if attacked
        {
            StopRepair();
        }

        if (health <= 0)
        {
            Break();
        }
    }

    private void Break()
    {
        if (state == ObjectState.Broken) return; // Prevent redundant calls

        state = ObjectState.Broken;
        SetBrokenState(true);
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

    public void StartRepair()
    {
        if (state != ObjectState.Broken || isBeingRepaired || isUnderAttack) return; // Don't allow repairs during attack

        isBeingRepaired = true;
        state = ObjectState.Repairing;
        isUnderAttack = false; // Reset attack state when repairing starts
        UIController.Instance.ShowRepairMeter(true);

        StartCoroutine(HealOverTime(maxHealth - health, 5f));
    }

    private IEnumerator HealOverTime(int repairAmount, float duration)
    {
        float repairSpeed = duration / repairAmount;
        int repaired = 0;

        while (repaired < repairAmount && isBeingRepaired)
        {
            yield return new WaitForSeconds(repairSpeed);

            health++;
            repaired++;
            UIController.Instance.UpdateRepairMeter(health);

            if (health >= maxHealth)
            {
                CompleteRepair();
                yield break;
            }
        }
    }

    private void CompleteRepair()
    {
        if (health < maxHealth) return; // Ensure repair doesn't complete in one frame

        health = maxHealth;
        isBeingRepaired = false;
        state = ObjectState.Normal;
        isUnderAttack = false;
        SetBrokenState(false);
        UIController.Instance.ShowRepairMeter(false);
    }

    public void StopRepair()
    {
        isBeingRepaired = false;
        state = ObjectState.Broken;
        UIController.Instance.ShowRepairMeter(false);
    }

    public void Restore()
    {
        if (state != ObjectState.Broken) return;

        health = maxHealth;
        isUnderAttack = false;
        SetBrokenState(false);
    }

    public void NotifyGremlinLeft()
    {
        isUnderAttack = false; // Reset when gremlins leave
    }
}
