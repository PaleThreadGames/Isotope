using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Atomic Integrity")]
    public int currentProtons = 3;
    public int maxProtons = 5;

    [Header("Ionic Stabilization")]
    public float ionConcentration = 0f; 
    public float maxConcentration = 100f;
    public float healCost = 33f;

    public void AddIons(float amount)
    {
        ionConcentration = Mathf.Clamp(ionConcentration + amount, 0, maxConcentration);
    }

    public void StabilizeNucleus()
    {
        if (ionConcentration >= healCost && currentProtons < maxProtons)
        {
            ionConcentration -= healCost;
            currentProtons++;
            Debug.Log("Nucleus Stabilized. Current Protons: " + currentProtons);
        }
    }

    public void TakeDamage(int amount)
    {
        currentProtons -= amount;
        if (currentProtons <= 0) Collapse();
    }

    void Collapse()
    {
        Debug.Log("Atomic Structure Collapsed: Game Over");
    }
}