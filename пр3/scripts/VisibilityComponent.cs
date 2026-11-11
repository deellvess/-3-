using Godot;
using System.Collections.Generic;

/// <summary>Tracks nearby bodies in a target group and selects the closest one.</summary>
[GlobalClass]
public partial class VisibilityComponent : Area3D
{
    [Export] public string TargetGroup { get; set; } = "enemy";
    [Export] public float DetectionRadius { get; set; } = 7.0f;
    private readonly HashSet<Node3D> _visibleTargets = new();
    public override void _Ready()
    {
        var collision = GetNode<CollisionShape3D>("CollisionShape3D");
        collision.Shape = new SphereShape3D { Radius = Mathf.Max(0.1f, DetectionRadius) };
        Monitoring = true;
        BodyEntered += OnBodyEntered;
        BodyExited += OnBodyExited;
    }
    public Node3D GetClosestTarget()
    {
        Node3D closest = null;
        float bestDistanceSquared = float.PositiveInfinity;

        foreach (Node3D target in _visibleTargets)
        {
            if (!GodotObject.IsInstanceValid(target) || target.IsQueuedForDeletion())
                continue;

            float distanceSquared = GlobalPosition.DistanceSquaredTo(target.GlobalPosition);
            if (distanceSquared < bestDistanceSquared)
            {
                bestDistanceSquared = distanceSquared;
                closest = target;
            }
        }

        return closest;
    }

    private void OnBodyEntered(Node3D body)
    {
        if (body.IsInGroup(TargetGroup))
            _visibleTargets.Add(body);
    }

    private void OnBodyExited(Node3D body)
    {
        _visibleTargets.Remove(body);
    }
}
