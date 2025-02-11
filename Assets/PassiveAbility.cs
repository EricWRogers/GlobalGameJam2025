using UnityEngine;

public class PassiveAbility : ScriptableObject
{
    private CharacterBase owner;
    public void Activate(CharacterBase _owner) {
        owner = _owner;
        //TODO Register in the Update Ticker for passives
    }
}
