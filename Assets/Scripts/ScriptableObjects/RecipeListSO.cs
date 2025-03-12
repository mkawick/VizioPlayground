using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

//[CreateAssetMenu(fileName = "RecipeListSO", menuName = "Scriptable Objects/RecipeListSO")]
// only create one
public class RecipeListSO : ScriptableObject
{
    public List<BurgerRecipeSO> recipeList;
}
