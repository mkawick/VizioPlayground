using System;
using UnityEngine;

public interface IProgressBarUI 
{
    public event EventHandler<OnProgressChangedEventArgs> OnProgressChanged;
    public class OnProgressChangedEventArgs : EventArgs
    {
        public float percentage;
    }
}
