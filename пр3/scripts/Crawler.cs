using Godot;

/// <summary>Ground enemy that chases the nearest friendly body and attacks by contact.</summary>
public partial class Crawler : CharacterBody3D
{
    [Export] public float Speed { get; set; } = 3.0f;
    [Export] public float StopDistance { get; set; } = 0.6f;
    [Export] public float Gravity { get; set; } = 12.0f;

    public bool IsFrozen { get; set; }

    public override void _PhysicsProcess(double delta)
    {
        Node3D target = FindClosestTarget();
        Vector3 movement = Vector3.Zero;

        if (!IsFrozen && target != null)
        {
            movement = target.GlobalPosition - GlobalPosition;
            movement.Y = 0;
            if (movement.Length() <= StopDistance)
                movement = Vector3.Zero;
        }

        if (movement != Vector3.Zero)
        {
            Vector3 facing = target.GlobalPosition;
            facing.Y = GlobalPosition.Y;
            GetNode<Node3D>("Visual").LookAt(facing, Vector3.Up);
            movement = movement.Normalized() * Speed;
        }

        Velocity = new Vector3(movement.X, IsOnFloor() ? 0 : Velocity.Y - Gravity * (float)delta, movement.Z);
        MoveAndSlide();
    }

    private Node3D FindClosestTarget()
    {
        Node3D closest = null;
        float shortest = float.PositiveInfinity;
        foreach (Node candidate in GetTree().GetNodesInGroup("player"))
        {
            if (candidate is not Node3D body || body.IsQueuedForDeletion())
                continue;

            float distance = GlobalPosition.DistanceSquaredTo(body.GlobalPosition);
            if (distance < shortest)
            {
                shortest = distance;
                closest = body;
            }
        }
        return closest;
    }
}
