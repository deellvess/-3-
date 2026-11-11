using Godot;

/// <summary>Adds delayed poison ticks to the ordinary impact damage.</summary>
[GlobalClass]
public partial class PoisonDamageComponent : DamagingComponent
{
    [Export] public int TickDamage { get; set; } = 1;
    [Export] public int TickCount { get; set; } = 3;
    [Export] public float TickInterval { get; set; } = 0.7f;

    public override void ApplyDamage(Node3D target)
    {
        base.ApplyDamage(target);
        if (target.IsQueuedForDeletion() || TickCount <= 0 || TickDamage <= 0)
            return;

        var effect = new PoisonStatus();
        effect.Configure(TickDamage, TickCount, TickInterval);
        target.AddChild(effect);
    }
}
