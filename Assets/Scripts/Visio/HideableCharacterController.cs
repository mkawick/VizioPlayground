using System;
using UnityEngine;

public class HideableCharacterController : IHideableObject
{
    public override void CancelEffect(int id)
    {
        // NotImplementedException
    }

    public override void ClearAllEffects()
    {
        // NotImplementedException
    }

    PuppetController _puppet;

    protected PuppetController puppet
    {
        get
        {
            if (_puppet == null)
            {
                _puppet = GetComponent<PuppetController>();
            }
            return _puppet;
        }
    }

    public override bool HasVisibilityEffect()
    {
        return puppet.mainCharacter.isInvisible;
    }

    public override void ApplyVisibilityEffect(int lengthInMs, bool isInvisible)
    {
        if (isInvisible)
        {
            puppet.mainCharacter.AddStatus(CharacterStatusFlags.Invisible);
        }
        else
        {
            puppet.mainCharacter.RemoveStatus(CharacterStatusFlags.Invisible);
        }
    }

    

    public override void ApplyZoneSettings(VisibilityZoneDefinition definition)
    {
        var puppetMainCharacter = puppet.mainCharacter;

        if (definition.speed < 1)
        {
            puppetMainCharacter.AddStatus(CharacterStatusFlags.Slow);
        }
        else
        {
            puppetMainCharacter.RemoveStatus(CharacterStatusFlags.Slow);
        }

        if (definition.visibility < 1)
        {
            puppetMainCharacter.AddStatus(CharacterStatusFlags.Myopia);
            var visibilityRange = puppetMainCharacter.modifiedDefinition.visibility.range;
            normalVisionRange = visibilityRange;
            restrictedVisionRange = definition.visibility * visibilityRange;
        }
        else
        {
            puppetMainCharacter.RemoveStatus(CharacterStatusFlags.Myopia);
            var visibilityRange = puppetMainCharacter.modifiedDefinition.visibility.range;
            normalVisionRange = visibilityRange;
            restrictedVisionRange = visibilityRange;
        }
    }
}
