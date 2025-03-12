using System;
using UnityEngine;
using UnityEngine.UI;
using static CuttingCounter;

public class ProgressBarUI : MonoBehaviour
{
    [SerializeField] private GameObject counter;
    private IProgressBarUI cuttingCounter;
    [SerializeField] private Image barImage;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (counter)
        {
            cuttingCounter = counter.GetComponent<IProgressBarUI>();
            if (cuttingCounter == null)
            {
                Debug.LogError("cuttingCounter is wrong type");
            }
            cuttingCounter.OnProgressChanged += ProgressBar_OnProgressChanged;
            barImage.fillAmount = 0;
            Show();
        }
    }

    private void ProgressBar_OnProgressChanged(object sender, IProgressBarUI.OnProgressChangedEventArgs e)
    {
        barImage.fillAmount = e.percentage;
        Show();
    }

    void Show()
    {
        gameObject.SetActive((barImage.fillAmount >0) && (barImage.fillAmount<0.99f));
    }
}
