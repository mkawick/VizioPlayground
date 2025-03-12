using UnityEngine;
using static Player;

public class SelectedCounterVisual : MonoBehaviour
{
    [SerializeField]
    GameObject[] visual;
    [SerializeField]
    BaseCounter parentCounter;

    public void SetSelected(bool isSelected)
    {
        for (int i = 0; i < visual.Length; i++)
        {
            visual[i].SetActive(isSelected);
        }
    }

    private void Start()
    {
        if (Player.Instance)
        {
            Player.Instance.OnSelectCounterChange += Instance_OnSelectCounterChange;
        }
    }

    private void Instance_OnSelectCounterChange(object sender, Player.OnSelectCounterChangeEventArgs eventArgs)
    {
        if (parentCounter == null)
        {
            return;
        }

        for (int i = 0; i < visual.Length; i++)
        {
            visual[i].SetActive(eventArgs.selectedCounter == parentCounter);
        }
    }
}
