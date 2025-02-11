using UnityEngine;

public interface ICharacter {
    public void OnBasicAttack();
    public void OnAbility();
    public void OnUltimate();
    public void OnUseItem();
}


public abstract class CharacterBase : ClientBase, ICharacter {
    public PassiveAbility passive;

    public abstract void OnAbility();
    public abstract void OnBasicAttack();
    public abstract void OnUltimate();
    public abstract void OnUseItem();

    private void Start() {
        passive.Activate(this);
        Debug.Log("This is me straight up doing other things as a network behavior");
    }
}

public class CharacterImpl : CharacterBase {
    public override void OnAbility() {
        throw new System.NotImplementedException();
    }

    public override void OnBasicAttack() {
        throw new System.NotImplementedException();
    }

    public override void OnUltimate() {
        throw new System.NotImplementedException();
    }

    public override void OnUseItem() {
        throw new System.NotImplementedException();
    }
}
