using Godot;
using System;

public partial class Bunker : StaticBody2D
{
	[Export]
	public Node2D Arrow;
	
	private bool _isActive = false;
	private Vector2 _currentDirection = Vector2.Up;
	private Player _playerRef;

	[Export] public PackedScene ProjectileScene;
	private bool _isCharging = false;

	public override void _Ready()
	{
		SetBunkerActive(false);
	}

	public override void _Process(double delta)
	{
		if (!_isActive) return;

		if (Input.IsActionJustPressed("interact"))
		{
			GameManager.Instance.ChangeState(GameState.Exploration);
			return;
		}

		// 1. Cambio de lado con WASD
		Vector2 inputDir = InputManager.Instance.GetMovementDirection();
		if (inputDir != Vector2.Zero)
		{
			if (Mathf.Abs(inputDir.X) > Mathf.Abs(inputDir.Y))
				_currentDirection = new Vector2(Mathf.Sign(inputDir.X), 0);
			else
				_currentDirection = new Vector2(0, Mathf.Sign(inputDir.Y));
		}

		// 2. Oscilación con el ratón (limitada a 90 grados en total, +/- 45 del eje)
		Vector2 mousePos = GetGlobalMousePosition();
		Vector2 dirToMouse = (mousePos - GlobalPosition).Normalized();
		
		float targetAngle = dirToMouse.Angle();
		float baseAngle = _currentDirection.Angle();
		
		// Calculamos la diferencia de ángulo y la limitamos a 45 grados (PI/4)
		float angleDiff = Mathf.AngleDifference(baseAngle, targetAngle);
		float clampedDiff = Mathf.Clamp(angleDiff, -Mathf.Pi / 4, Mathf.Pi / 4);
		float finalAngle = baseAngle + clampedDiff;

		if (Arrow != null)
		{
			Arrow.Rotation = finalAngle;
			Arrow.Position = new Vector2(Mathf.Cos(finalAngle), Mathf.Sin(finalAngle)) * 40.0f;
		}

		// 3. Disparo
		if (Input.IsActionJustPressed("charge_projectile"))
		{
			_isCharging = true;
		}

		if (Input.IsActionJustReleased("charge_projectile") && _isCharging)
		{
			FireProjectile(new Vector2(Mathf.Cos(finalAngle), Mathf.Sin(finalAngle)));
			_isCharging = false;
		}
	}

	private void FireProjectile(Vector2 direction)
	{
		if (ProjectileScene == null)
		{
			GD.PrintErr("ProjectileScene no asignada en el Búnker");
			return;
		}

		Projectile projectile = ProjectileScene.Instantiate<Projectile>();
		GetParent().AddChild(projectile);
		
		projectile.GlobalPosition = GlobalPosition + (direction * 45.0f);
		projectile.Direction = direction;
		projectile.Rotation = direction.Angle();
		
		GD.Print("¡Fuego!");
	}

	public void SetBunkerActive(bool active)
	{
		_isActive = active;
		if (Arrow != null) Arrow.Visible = active;
		
		if (active && _playerRef != null)
		{
			// Guardamos la posición actual por seguridad (aunque ya la tenemos del búnker)
			// Y mandamos al jugador "al limbo" para que no estorbe
			_playerRef.GlobalPosition = new Vector2(-10000, -10000);
		}
		else if (!active && _playerRef != null)
		{
			// Si estamos desactivando (saliendo), posicionamos al jugador en el borde
			_playerRef.GlobalPosition = GlobalPosition + (_currentDirection * 60.0f);
		}
	}

	public void OnBodyEntered(Node2D body)
	{
		if (body is Player player && GameManager.Instance.CurrentState == GameState.Exploration)
		{
			_playerRef = player;
			
			// Calculamos la dirección de entrada para la flecha
			Vector2 entryVector = (GlobalPosition - player.GlobalPosition).Normalized();
			if (Mathf.Abs(entryVector.X) > Mathf.Abs(entryVector.Y))
			{
				_currentDirection = new Vector2(Mathf.Sign(entryVector.X), 0);
			}
			else
			{
				_currentDirection = new Vector2(0, Mathf.Sign(entryVector.Y));
			}

			UpdateArrowPosition();
			
			// Notificamos al manager que cambie al modo búnker
			GameManager.Instance.ChangeState(GameState.BunkerMode);
		}
	}

	private void UpdateArrowPosition()
	{
		if (Arrow == null) return;

		// Posicionamos la flecha en uno de los 4 bordes del búnker
		// El búnker es de 64x64 (según el ColorRect actual)
		float offset = 40.0f; 
		Arrow.Position = _currentDirection * offset;
		Arrow.Rotation = _currentDirection.Angle();
	}
}
