using Godot;

/// <summary>Controls the duration of a freeze without modifying the movement component.</summary>
public partial class FreezeStatus : Node
{
	private double _remaining;
	private MoveComponent _movement;
	private Crawler _crawler;
	private float _originalSpeed;

	public void Extend(float duration)
	{
		_remaining = Mathf.Max((float)_remaining, Mathf.Max(0.1f, duration));
	}

	public override void _Ready()
	{
		_movement = GetParent().GetNodeOrNull<MoveComponent>("MoveComponent");
		if (_movement != null)
		{
			_originalSpeed = _movement.Speed;
			_movement.Speed = 0;
		}
		_crawler = GetParent() as Crawler;
		if (_crawler != null)
			_crawler.IsFrozen = true;
	}

	public override void _PhysicsProcess(double delta)
	{
		_remaining -= delta;
		if (_remaining > 0)
			return;

		if (GodotObject.IsInstanceValid(_movement))
			_movement.Speed = _originalSpeed;
		if (GodotObject.IsInstanceValid(_crawler))
			_crawler.IsFrozen = false;
		QueueFree();
	}
}
