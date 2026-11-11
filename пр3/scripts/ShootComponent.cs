using Godot;

/// <summary>Aims at the nearest visible target and fires a configured projectile.</summary>
[GlobalClass]
public partial class ShootComponent : Node
{
	[Export] public PackedScene BulletScene { get; set; }
	[Export] public float ShotInterval { get; set; } = 1.0f;
	[Export] public float MuzzleHeight { get; set; } = 0.4f;
	[Export] public float SpawnOffset { get; set; } = 0.8f;

	private Node3D _owner;
	private Node3D _visual;
	private VisibilityComponent _visibility;
	private double _cooldown;

	public override void _Ready()
	{
		_owner = (Node3D)GetParent();
		_visual = _owner.GetNode<Node3D>("Visual");
		_visibility = _owner.GetNode<VisibilityComponent>("VisibilityComponent");
	}
	public override void _PhysicsProcess(double delta)
	{
		_cooldown -= delta;
		Node3D target = _visibility.GetClosestTarget();
		if (target == null)
			return;

		Vector3 lookTarget = target.GlobalPosition;
		lookTarget.Y = _visual.GlobalPosition.Y;
		if (_visual.GlobalPosition.DistanceSquaredTo(lookTarget) > 0.001f)
			_visual.LookAt(lookTarget, Vector3.Up);

		if (_cooldown > 0 || BulletScene == null)
			return;

		Vector3 muzzle = _owner.GlobalPosition + Vector3.Up * MuzzleHeight;
		Vector3 aimPoint = target.GlobalPosition + Vector3.Up * MuzzleHeight;
		Vector3 direction = (aimPoint - muzzle).Normalized();
		if (direction == Vector3.Zero)
			return;

		var bullet = BulletScene.Instantiate<Bullet>();
		bullet.Initialize(direction, _visibility.TargetGroup);
		GetTree().CurrentScene.AddChild(bullet);
		bullet.GlobalPosition = muzzle + direction * SpawnOffset;
		_cooldown = Mathf.Max(0.1f, ShotInterval);
	}
}
