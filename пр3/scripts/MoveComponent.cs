using Godot;

/// <summary>Moves a ranged enemy toward the base until a target is visible.</summary>
[GlobalClass]
public partial class MoveComponent : Node
{
	[Export] public float Speed { get; set; } = 3.0f;
	[Export] public float StopDistance { get; set; } = 0.2f;
	[Export] public Vector3 CenterPosition { get; set; } = Vector3.Zero;

	private CharacterBody3D _body;
	private Node3D _visual;
	private VisibilityComponent _visibility;

	public override void _Ready()
	{
		_body = (CharacterBody3D)GetParent();
		_visual = _body.GetNode<Node3D>("Visual");
		_visibility = _body.GetNode<VisibilityComponent>("VisibilityComponent");
	}
	public override void _PhysicsProcess(double delta)
	{
		Vector3 offset = CenterPosition - _body.GlobalPosition;
		offset.Y = 0;

		if (_visibility.GetClosestTarget() != null || offset.Length() <= StopDistance)
		{
			_body.Velocity = Vector3.Zero;
			return;
		}
		Vector3 lookTarget = CenterPosition;
		lookTarget.Y = _visual.GlobalPosition.Y;
		_visual.LookAt(lookTarget, Vector3.Up);

		_body.Velocity = offset.Normalized() * Speed;
		_body.MoveAndSlide();
	}
}
