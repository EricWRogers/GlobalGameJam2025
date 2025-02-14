using UnityEngine;
using UnityEngine.InputSystem;

public class TestCharacter : CharacterBase {
    public override void OnAbility(InputValue input) {
        Debug.Log("Used Ability");
    }

    public override void OnBasicAttack(InputValue input) {
        Debug.Log("Used Basic");
    }

    public override void OnUltimate(InputValue input) {
        Debug.Log("Used Ultimate");
    }

    public override void OnUseItem(InputValue input) {
        Debug.Log("Used Item");
    }
}
