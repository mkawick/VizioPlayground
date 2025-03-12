using System;
using UnityEngine;

public class CuttingCounterVisual : MonoBehaviour
{
    private const string CUT = "Cut";
    Animator animator;
    [SerializeField] 
    CuttingCounter cuttingCounter;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        cuttingCounter.OnCut += OnPlayerCut;
    }

    private void OnPlayerCut(object sender, EventArgs e)
    {
        animator.SetTrigger(CUT);
    }
}
