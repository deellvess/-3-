using Godot;

/// <summary>Управление персонажем, прицельная стрельба и постановка башен.</summary>
public partial class PlayerController : CharacterBody3D
{
    [Export] public float MoveSpeed { get; set; } = 5.5f;
    [Export] public float ShotInterval { get; set; } = 0.35f;
    [Export] public float MuzzleHeight { get; set; } = 0.7f;
    [Export] public float MuzzleOffset { get; set; } = 1.0f;
    [Export] public float BuildDistance { get; set; } = 3.0f;
    [Export] public float BuildClearance { get; set; } = 2.7f;
    [Export] public float BuildAreaHalfSize { get; set; } = 20.0f;
    [Export] public float GhostOpacity { get; set; } = 0.5f;
    [Export] public PackedScene BulletScene { get; set; }
    [Export] public PackedScene BasicTowerScene { get; set; }
    [Export] public PackedScene PoisonTowerScene { get; set; }
    [Export] public PackedScene FreezeTowerScene { get; set; }

    private Camera3D _camera;
    private MeshInstance3D _ghost;
    private StandardMaterial3D _ghostMaterial;
    private bool _building;
    private int _selectedTower;
    private double _shotCooldown;
    private Vector3 _buildPosition;
    private bool _buildPositionValid;

    public override void _Ready()
    {
        _camera = GetViewport().GetCamera3D();
        _ghost = GetNode<MeshInstance3D>("TowerPreview");
        _ghostMaterial = (StandardMaterial3D)_ghost.MaterialOverride;
        _ghost.Visible = false;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (GameManager.Instance?.IsGameOver == true)
            return;

        Vector3 input = Vector3.Zero;
        if (Input.IsKeyPressed(Key.W)) input.Z -= 1;
        if (Input.IsKeyPressed(Key.S)) input.Z += 1;
        if (Input.IsKeyPressed(Key.A)) input.X -= 1;
        if (Input.IsKeyPressed(Key.D)) input.X += 1;
        Velocity = input.Normalized() * MoveSpeed;
        MoveAndSlide();
        GlobalPosition = new Vector3(
            Mathf.Clamp(GlobalPosition.X, -BuildAreaHalfSize, BuildAreaHalfSize),
            GlobalPosition.Y,
            Mathf.Clamp(GlobalPosition.Z, -BuildAreaHalfSize, BuildAreaHalfSize));

        _shotCooldown -= delta;
        AimAtMouse();
        if (_building)
            UpdatePreview();
        else if ((Input.IsMouseButtonPressed(MouseButton.Left) || Input.IsKeyPressed(Key.Space)) && _shotCooldown <= 0)
            Shoot();
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (GameManager.Instance?.IsGameOver == true || @event is not InputEventKey key || !key.Pressed || key.Echo)
            return;

        if (key.Keycode == Key.B)
        {
            if (_building) PlaceTower();
            else BeginBuilding();
            GetViewport().SetInputAsHandled();
        }
        else if (key.Keycode == Key.Escape && _building)
        {
            EndBuilding();
            GetViewport().SetInputAsHandled();
        }
        else if (_building && key.Keycode is Key.Key1 or Key.Key2 or Key.Key3)
        {
            _selectedTower = key.Keycode switch { Key.Key1 => 0, Key.Key2 => 1, _ => 2 };
            GameManager.Instance?.ShowMessage($"Выбрана башня: {TowerName(_selectedTower)}", 1.5f);
            GetViewport().SetInputAsHandled();
        }
    }

    private void AimAtMouse()
    {
        if (_camera == null) return;
        Vector2 mouse = GetViewport().GetMousePosition();
        Vector3 origin = _camera.ProjectRayOrigin(mouse);
        Vector3 ray = _camera.ProjectRayNormal(mouse);
        if (Mathf.Abs(ray.Y) < 0.001f) return;
        float distance = (GlobalPosition.Y - origin.Y) / ray.Y;
        if (distance <= 0) return;
        Vector3 aim = origin + ray * distance;
        aim.Y = GlobalPosition.Y;
        if (GlobalPosition.DistanceSquaredTo(aim) > 0.01f)
            LookAt(aim, Vector3.Up);
    }

    private void Shoot()
    {
        if (BulletScene == null) return;
        Vector3 forward = -GlobalTransform.Basis.Z;
        Vector3 origin = GlobalPosition + Vector3.Up * MuzzleHeight + forward * MuzzleOffset;
        Bullet bullet = BulletScene.Instantiate<Bullet>();
        bullet.Initialize(forward, "enemy");
        GetTree().CurrentScene.AddChild(bullet);
        bullet.GlobalPosition = origin;
        _shotCooldown = Mathf.Max(0.05f, ShotInterval);
    }

    private void BeginBuilding()
    {
        _building = true;
        _ghost.Visible = true;
        UpdatePreview();
        GameManager.Instance?.ShowMessage("1/2/3 — тип башни, B — поставить, Esc — отмена", 3f);
    }

    private void EndBuilding()
    {
        _building = false;
        _ghost.Visible = false;
    }

    private void UpdatePreview()
    {
        Vector3 forward = -GlobalTransform.Basis.Z;
        _buildPosition = GlobalPosition + forward * BuildDistance;
        _buildPosition.Y = 0.45f;
        _ghost.GlobalPosition = _buildPosition;
        _buildPositionValid = CanBuildAt(_buildPosition);
        _ghostMaterial.AlbedoColor = _buildPositionValid
            ? new Color(0.2f, 1f, 0.4f, GhostOpacity)
            : new Color(1f, 0.2f, 0.2f, GhostOpacity);
    }

    private bool CanBuildAt(Vector3 position)
    {
        if (Mathf.Abs(position.X) > BuildAreaHalfSize || Mathf.Abs(position.Z) > BuildAreaHalfSize)
            return false;
        foreach (Node node in GetTree().GetNodesInGroup("player"))
        {
            if (node is Node3D body && body.GlobalPosition.DistanceSquaredTo(position) < BuildClearance * BuildClearance)
                return false;
        }
        foreach (Node node in GetTree().GetNodesInGroup("enemy"))
        {
            if (node is Node3D body && body.GlobalPosition.DistanceSquaredTo(position) < BuildClearance * BuildClearance)
                return false;
        }
        return true;
    }

    private void PlaceTower()
    {
        UpdatePreview();
        if (!_buildPositionValid)
        {
            GameManager.Instance?.ShowMessage("Здесь нельзя поставить башню", 2f);
            return;
        }
        PackedScene scene = SelectedScene();
        if (scene == null) return;
        StaticBody3D tower = scene.Instantiate<StaticBody3D>();
        GetTree().CurrentScene.AddChild(tower);
        tower.GlobalPosition = new Vector3(_buildPosition.X, 0f, _buildPosition.Z);
        EndBuilding();
        GameManager.Instance?.ShowMessage($"Построена башня: {TowerName(_selectedTower)}", 2f);
    }

    private PackedScene SelectedScene() => _selectedTower switch
    {
        0 => BasicTowerScene,
        1 => PoisonTowerScene,
        _ => FreezeTowerScene
    };

    private static string TowerName(int type) => type switch
    {
        0 => "обычная",
        1 => "ядовитая",
        _ => "ледяная"
    };
}
