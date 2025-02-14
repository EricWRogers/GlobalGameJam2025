using UnityEngine;

public interface IPassive {
    public void Activate(CharacterBase owner);
    public void ToggleActive();
    public void Tick();
    public void FixedTick();
}

public abstract class PassiveAbility : ScriptableObject, IPassive {
    protected CharacterBase owner;
    public bool IsEnabled {
        get; protected set;
    }

    public virtual void Activate(CharacterBase _owner) {
        owner = _owner;
    }

    public abstract void FixedTick();
    public abstract void Tick();

    public virtual void ToggleActive() {
        IsEnabled = !IsEnabled;
    }
}

[CreateAssetMenu(menuName = "Passives/TestPassive")]
public class TestPassive : PassiveAbility {

    public override void FixedTick() {
        
    }

    public override void Tick() {
        
    }
}
