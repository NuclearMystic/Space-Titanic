using System.Collections;
using UnityEngine;

public class BreakableObject : MonoBehaviour
{
    public enum BreakableType { Furnace, Boiler, Gravity }
    public BreakableType objectType;

    public int health = 10;
    public GameObject normalPrefab;
    public GameObject brokenPrefab;
    public bool isBroken = false;
    private bool isBeingRepaired = false;



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

    public void StartRepair()
    {
        if (!isBroken || isBeingRepaired) return;

        isBeingRepaired = true;
        UIController.Instance.ShowRepairMeter(true);
        StartCoroutine(RepairCoroutine());
    }

    private IEnumerator RepairCoroutine()
    {
        int repairAmount = 10; // Amount required to fully repair
        float repairSpeed = 5f; // Adjust speed if needed
        int currentProgress = 0;

        while (currentProgress < repairAmount && isBeingRepaired)
        {
            yield return new WaitForSeconds(repairSpeed);

            currentProgress++;
            UIController.Instance.UpdateRepairMeter(health);

            if (currentProgress >= repairAmount)
            {
                health = 10;
                isBroken = false;
                SetBrokenState(false);
                UIController.Instance.ShowRepairMeter(false);
                isBeingRepaired = false;
            }
        }
    }

    public void StopRepair()
    {
        isBeingRepaired = false;
        UIController.Instance.ShowRepairMeter(false);
    }

    public void Restore()
    {
        if (!isBroken) return;

        health = 10;
        isBroken = false;
        gameObject.tag = "BreakableObject";
        SetBrokenState(false);
    }
}