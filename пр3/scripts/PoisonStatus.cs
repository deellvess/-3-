using Godot;

/// <summary>Lives on the damaged object so poison continues after the bullet vanishes.</summary>
public partial class PoisonStatus : Node
{
	private int _tickDamage;
	private int _ticksLeft;
	private float _interval;
	private double _remaining;

	public void Configure(int tickDamage, int ticks, float interval)
	{
		_tickDamage = tickDamage;
		_ticksLeft = ticks;
		_interval = Mathf.Max(0.1f, interval);
		_remaining = _interval;
	}

	public override void _PhysicsProcess(double delta)
	{
		_remaining -= delta;
		if (_remaining > 0)
			return;

		var health = GetParent().GetNodeOrNull<HealthComponent>("HealthComponent");
		if (health == null || health.CurrentHealth <= 0)
		{
			QueueFree();
			return;
		}

		health.TakeDamage(_tickDamage);
		_ticksLeft--;
		if (_ticksLeft <= 0)
	}
}
