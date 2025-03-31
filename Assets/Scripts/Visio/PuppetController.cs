using System;

using UnityEngine;
using UnityEngine.TextCore.Text;
using static UnityEngine.ParticleSystem;

public enum CharacterStatusFlags
{
    None, Invisible, Slow, Myopia
}
public struct CharacterDefinitionVisibility
{
    public float range;
}
public class CharacterDefinition
{
    public CharacterDefinitionVisibility visibility;
}

public struct PhysicsLayer
{
    public int LayerInt { get; private set; }
    public int id { get; private set; }
    static int UniqueId = 1;

    public PhysicsLayer(int inLayerInt)
    {
        LayerInt = inLayerInt;
        id = UniqueId++;
    }

    public static readonly PhysicsLayer invisibleObjects = new PhysicsLayer(LayerMask.NameToLayer("InvisibleObjects"));
    public static readonly PhysicsLayer particles = new PhysicsLayer(LayerMask.NameToLayer("Particles"));

}

public class Character
{
    public bool isInvisible;
    public CharacterDefinition modifiedDefinition;

    public void AddStatus (CharacterStatusFlags flag)
    {

    }
    public void RemoveStatus(CharacterStatusFlags flag)
    {

    }
}

public class PuppetController: MonoBehaviour
{
    Character character;
    public Character mainCharacter => character;
}