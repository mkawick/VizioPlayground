using UnityEngine;

public class StoveCounterVisual : MonoBehaviour
{
    [SerializeField]
    GameObject burnerOnVisual;
    [SerializeField]
    GameObject cookingParticlesVisual;
    [SerializeField]
    StoveCounter stove;
    private void Start()
    {
        stove.OnStoveStateChangedEvent += Stove_OnStoveStateChangedEvent;
        burnerOnVisual.SetActive(false);
        cookingParticlesVisual.SetActive(false);
    }

    private void Stove_OnStoveStateChangedEvent(object sender, StoveCounter.StoveStateChanged e)
    {
        burnerOnVisual.SetActive(e.burnerOn);
        cookingParticlesVisual.SetActive(e.isCooking);
    }
}
