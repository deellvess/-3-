using Godot;


/// <summary>Projectile motion; its child damage component handles every hit.</summary>
public partial class Bullet : Node3D
{
	[Export] public float Speed { get; set; } = 12.0f;
	[Export] public float Lifetime { get; set; } = 3.0f;

	private Vector3 _direction;
	private double _age;

	public void Initialize(Vector3 direction, string targetGroup)
	{
		_direction = direction.Normalized();
		GetNode<DamagingComponent>("DamagingComponent").SetTargetGroup(targetGroup);
	}
	public override void _PhysicsProcess(double delta)
	{
		GlobalPosition += _direction * Speed * (float)delta;
		_age += delta;

		if (_age >= Lifetime)
			QueueFree();
	}
}
