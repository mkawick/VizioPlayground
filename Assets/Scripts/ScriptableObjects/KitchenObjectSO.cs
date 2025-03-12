using UnityEngine;

[CreateAssetMenu(fileName = "KitchenObjectSO", menuName = "Scriptable Objects/KitchenObjectSO")]
public class KitchenObjectSO : ScriptableObject
{
    public Transform objectPrefab;
    public Sprite sprite;
    public string objectName;
}
