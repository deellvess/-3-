using Godot;
using System.Collections.Generic;

/// <summary>Reusable hit area for projectiles and contact attacks.</summary>
[GlobalClass]
public partial class DamagingComponent : Area3D
{
    [Export] public int Damage { get; set; } = 1;
    [Export] public string[] TargetGroups { get; set; } = System.Array.Empty<string>();
    [Export] public bool OneShot { get; set; } = true;
    [Export] public float RepeatInterval { get; set; } = 0.75f;

    private readonly Dictionary<ulong, double> _lastHits = new();
    private double _elapsed;
    private bool _consumed;

    public override void _Ready()
    {
        Monitoring = true;
        BodyEntered += OnBodyEntered;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (OneShot)
            return;

        _elapsed += delta;
        foreach (Node3D body in GetOverlappingBodies())
            TryDamage(body);
    }

    public void SetTargetGroup(string group)
    {
        TargetGroups = new[] { group };
    }

    /// <summary>Applies direct damage; subclasses add effects after calling base.</summary>
    public virtual void ApplyDamage(Node3D target)
    {
        target.GetNodeOrNull<HealthComponent>("HealthComponent")?.TakeDamage(Damage);
    }

    private void OnBodyEntered(Node3D body)
    {
        TryDamage(body);
    }

    private void TryDamage(Node3D body)
    {
        if (_consumed || IsQueuedForDeletion() || !GodotObject.IsInstanceValid(body) || body.IsQueuedForDeletion())
            return;

        bool matches = false;
        foreach (string group in TargetGroups)
        {
            if (body.IsInGroup(group))
            {
                matches = true;
                break;
            }
        }
        if (!matches)
            return;

        ulong id = body.GetInstanceId();
        if (!OneShot && _lastHits.TryGetValue(id, out double lastHit)
            && _elapsed - lastHit < Mathf.Max(0.05f, RepeatInterval))
            return;

        _lastHits[id] = _elapsed;
        ApplyDamage(body);
        if (OneShot)
        {
            _consumed = true;
            DestroyAfterHit();
        }
    }

    /// <summary>Removes the projectile owner after a single successful hit.</summary>
    public virtual void DestroyAfterHit()
    {
        GetParent().QueueFree();
    }
}
