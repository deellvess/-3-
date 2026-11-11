using Godot;

/// <summary>Temporarily stops an enemy's movement after the ordinary hit.</summary>
[GlobalClass]
public partial class FreezeDamageComponent : DamagingComponent
{
    [Export] public float FreezeDuration { get; set; } = 2.0f;

    public override void ApplyDamage(Node3D target)
    {
        base.ApplyDamage(target);
        if (target.IsQueuedForDeletion())
            return;

        var status = target.GetNodeOrNull<FreezeStatus>("FreezeStatus");
        if (status != null)
            status.Extend(FreezeDuration);
        else
        {
            status = new FreezeStatus { Name = "FreezeStatus" };
            status.Extend(FreezeDuration);
            target.AddChild(status);
        }
    }
}
