using Godot;

/// <summary>Stores hit points and removes the owner when they reach zero.</summary>
[GlobalClass]
public partial class HealthComponent : Node
{
	[Export] public int MaxHealth { get; set; } = 3;

	public int CurrentHealth { get; private set; }

	public override void _Ready()
	{
		CurrentHealth = MaxHealth;
	}
	public void TakeDamage(int amount)
	{
		if (amount <= 0 || CurrentHealth <= 0)
			return;

		CurrentHealth = Mathf.Max(0, CurrentHealth - amount);
		if (CurrentHealth == 0)
			Die();
	}
	public void Heal(int amount)
	{
		if (amount <= 0 || CurrentHealth <= 0)
			return;

		CurrentHealth = Mathf.Min(MaxHealth, CurrentHealth + amount);
	}
	private void Die()
	{
		GetParent().QueueFree();
	}
}
