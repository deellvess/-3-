using Godot;
using System.Collections.Generic;

/// <summary>Управляет волнами врагов, интерфейсом и завершением партии.</summary>
public partial class GameManager : Node3D
{
	public static GameManager Instance { get; private set; }
	public bool IsGameOver { get; private set; }

	[Export] public float EnemySpawnInterval { get; set; } = 8f;
	[Export] public int AdditionalEnemyCount { get; set; } = 10;
	[Export] public float SpawnDistance { get; set; } = 18f;
	[Export] public PackedScene RangedEnemyScene { get; set; }
	[Export] public PackedScene CrawlerScene { get; set; }

	private readonly HashSet<ulong> _trackedEnemies = new();
	private Label _statusLabel;
	private Label _helpLabel;
	private Label _messageLabel;
	private Label _endLabel;
	private HealthComponent _playerHealth;
	private HealthComponent _coreHealth;
	private double _spawnTimer;
	private double _messageTimer;
	private int _spawnedEnemies;
	private int _activeEnemies;

	public override void _Ready()
	{
		Instance = this;
		_spawnTimer = EnemySpawnInterval;
		_playerHealth = GetNodeOrNull<HealthComponent>("Player/HealthComponent");
		_coreHealth = GetNodeOrNull<HealthComponent>("Core/HealthComponent");
		CreateHud();

		GetTree().NodeAdded += RegisterEnemy;
		foreach (Node node in GetTree().GetNodesInGroup("enemy")) RegisterEnemy(node);

		Node core = GetNodeOrNull<Node>("Core");
		Node player = GetNodeOrNull<Node>("Player");
		if (core != null) core.TreeExiting += () => EndGame("Ядро уничтожено — поражение");
		if (player != null) player.TreeExiting += () => EndGame("Игрок погиб — поражение");
		UpdateHud();
	}

	public override void _ExitTree()
	{
		if (Instance == this) Instance = null;
		GetTree().NodeAdded -= RegisterEnemy;
	}

	public override void _Process(double delta)
	{
		if (IsGameOver) return;

		if (_messageTimer > 0)
		{
			_messageTimer -= delta;
			if (_messageTimer <= 0) _messageLabel.Text = "";
		}

		if (_spawnedEnemies < AdditionalEnemyCount)
		{
			_spawnTimer -= delta;
			if (_spawnTimer <= 0)
			{
				SpawnEnemy();
				_spawnTimer = Mathf.Max(0.5f, EnemySpawnInterval);
			}
		}

		if (_spawnedEnemies >= AdditionalEnemyCount && _activeEnemies == 0)
			EndGame("Все волны отбиты — победа!");

		UpdateHud();
	}

	public void ShowMessage(string message, float seconds = 2f)
	{
		_messageLabel.Text = message;
		_messageTimer = Mathf.Max(0.1f, seconds);
	}

	private void RegisterEnemy(Node node)
	{
		if (!node.IsInGroup("enemy") || !_trackedEnemies.Add(node.GetInstanceId())) return;
		_activeEnemies++;
		node.TreeExiting += () => OnEnemyRemoved(node);
	}

	private void OnEnemyRemoved(Node node)
	{
		if (_trackedEnemies.Remove(node.GetInstanceId()))
			_activeEnemies--;
	}

	private void SpawnEnemy()
	{
		PackedScene scene = _spawnedEnemies % 2 == 0 ? CrawlerScene : RangedEnemyScene;
		if (scene == null) return;
		Node3D enemy = scene.Instantiate<Node3D>();
		AddChild(enemy);
		float angle = (float)GD.RandRange(0.0, Mathf.Tau);
		enemy.GlobalPosition = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * SpawnDistance;
		_spawnedEnemies++;
		ShowMessage($"Волна: {_spawnedEnemies}/{AdditionalEnemyCount}", 1.2f);
	}

	private void EndGame(string text)
	{
		if (IsGameOver) return;
		IsGameOver = true;
		_endLabel.Text = text;
		_messageLabel.Text = "";
		UpdateHud();
		GetTree().Paused = true;
	}

	private void CreateHud()
	{
		CanvasLayer layer = new() { Name = "HUD" };
		AddChild(layer);
		_statusLabel = new Label { Position = new Vector2(18, 14), Modulate = new Color(0.9f, 0.96f, 1f) };
		_statusLabel.AddThemeFontSizeOverride("font_size", 22);
		layer.AddChild(_statusLabel);
		_helpLabel = new Label
		{
			Position = new Vector2(18, 46),
			Text = "WASD — ходьба  |  мышь — прицел  |  ЛКМ/пробел — выстрел  |  B — стройка  |  1/2/3 — тип  |  Esc — отмена",
			Modulate = new Color(0.8f, 0.9f, 0.9f)
		};
		_helpLabel.AddThemeFontSizeOverride("font_size", 16);
		layer.AddChild(_helpLabel);
		_messageLabel = new Label { Position = new Vector2(18, 78), Modulate = new Color(1f, 0.9f, 0.3f) };
		_messageLabel.AddThemeFontSizeOverride("font_size", 20);
		layer.AddChild(_messageLabel);
		_endLabel = new Label { Position = new Vector2(180, 175), Modulate = new Color(1f, 0.95f, 0.8f) };
		_endLabel.AddThemeFontSizeOverride("font_size", 38);
		layer.AddChild(_endLabel);
	}

	private void UpdateHud()
	{
		if (_statusLabel == null) return;
		string player = _playerHealth == null || !GodotObject.IsInstanceValid(_playerHealth) ? "0" : _playerHealth.CurrentHealth.ToString();
		string core = _coreHealth == null || !GodotObject.IsInstanceValid(_coreHealth) ? "0" : _coreHealth.CurrentHealth.ToString();
		_statusLabel.Text = $"Игрок: {player} HP     Ядро: {core} HP     Враги: {_activeEnemies}";
	}
}
