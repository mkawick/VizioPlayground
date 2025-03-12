using System;
using UnityEngine;

public class ContainerCounterVisual : MonoBehaviour
{
    private const string OPEN_CLOSE = "OpenClose";
    Animator animator;
    [SerializeField] 
    ContainerCounter containerCounter;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        containerCounter.OnPlayerGrabObject += OnPlayerInteractEvent;
    }

    private void OnPlayerInteractEvent(object sender, EventArgs e)
    {
        animator.SetTrigger(OPEN_CLOSE);
    }
}
