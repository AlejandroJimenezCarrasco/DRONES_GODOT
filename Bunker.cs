using Godot;
using System;

public partial class Bunker : StaticBody2D
{
	[Export] public Node2D Arrow;
	[Export] public Node2D TargetCircle;
	[Export] public PackedScene ProjectileScene;
	[Export] public PackedScene StoneScene;
	
	private bool _isActive = false;
	private Vector2 _currentDirection = Vector2.Up;
	private Player _playerRef;
	private Control _inventoryUI;

	public enum ProjectileType { Missile, Stone }
	private ProjectileType _currentType = ProjectileType.Missile;

	private bool _isCharging = false;
	private float _chargeTime = 0.0f;
	private float _maxChargeTime = 1.5f;
	private float _overchargeTime = 0.0f;
	private float _maxOverchargeTime = 3.0f;
	private float _pulsePhase = 0.0f;

	public override void _Ready()
	{
		SetBunkerActive(false);
		Node hud = GetTree().Root.FindChild("HUD", true, false);
		if (hud != null)
		{
			_inventoryUI = hud.GetNode<Control>("InventoryUI");
			UpdateInventoryUI();
		}
	}

	public override void _Process(double delta)
	{
		if (!_isActive) return;

		if (Input.IsActionJustPressed("switch_weapon"))
		{
			_currentType = (_currentType == ProjectileType.Missile) ? ProjectileType.Stone : ProjectileType.Missile;
			UpdateInventoryUI();
			ResetCharge(); // Resetear visuales al cambiar
		}

		if (Input.IsActionJustPressed("interact"))
		{
			GameManager.Instance.ChangeState(GameState.Exploration);
			return;
		}

		Vector2 mousePos = GetGlobalMousePosition();
		Vector2 dirToMouse = (mousePos - GlobalPosition).Normalized();

		if (Arrow != null)
		{
			Arrow.Rotation = dirToMouse.Angle();
			
			if (_isCharging)
			{
				float chargePercent = Mathf.Clamp(_chargeTime / _maxChargeTime, 0.0f, 1.0f);
				
				if (_currentType == ProjectileType.Missile)
				{
					// LOGICA MISIL: La flecha crece
					_chargeTime += (float)delta;
					Arrow.Scale = new Vector2(1.0f + (chargePercent * 2.0f), 1.0f);
					if (TargetCircle != null) TargetCircle.Visible = false;
				}
				else
				{
					// LOGICA PIEDRA: Flecha fija (0.7) y el círculo se aleja
					_chargeTime += (float)delta;
					Arrow.Scale = new Vector2(0.7f, 0.7f); 
					if (TargetCircle != null)
					{
						TargetCircle.Visible = true;
						// Rango reducido en un 30% aproximadamente
						float dist = 70.0f + (chargePercent * 350.0f);
						TargetCircle.Position = dirToMouse * dist;
					}
				}

				// Pulsación si llegamos al máximo (Overcharge)
				if (_chargeTime >= _maxChargeTime)
				{
					_overchargeTime += (float)delta;
					_pulsePhase += (float)delta * 15.0f;
					
					float alphaPulse = 0.5f + Mathf.Sin(_pulsePhase) * 0.5f;
					Color pulseColor = new Color(1, 1, 1, alphaPulse);
					
					Arrow.Modulate = pulseColor;
					if (TargetCircle != null && _currentType == ProjectileType.Stone)
					{
						TargetCircle.Modulate = pulseColor;
					}
					
					if (_overchargeTime >= _maxOverchargeTime)
					{
						Fire(dirToMouse);
						ResetCharge();
					}
				}
				else
				{
					// Resetear modulación si no estamos en overcharge
					Arrow.Modulate = new Color(1, 1, 1, 1);
					if (TargetCircle != null) TargetCircle.Modulate = new Color(1, 1, 1, 1);
				}
			}
		}

		if (Input.IsActionJustPressed("charge_projectile"))
		{
			_isCharging = true;
		}

		if (Input.IsActionJustReleased("charge_projectile") && _isCharging)
		{
			Fire(dirToMouse);
			ResetCharge();
		}
	}

	private void Fire(Vector2 direction)
	{
		// Punto de salida en el borde (40px)
		Vector2 spawnPos = GlobalPosition + (direction * 40.0f);
		
		if (_currentType == ProjectileType.Missile)
		{
			float dist = 70.0f * Arrow.Scale.X;
			FireProjectile(direction, spawnPos, dist);
		}
		else
		{
			if (TargetCircle != null)
			{
				float targetDist = TargetCircle.Position.Length();
				FireStone(direction, spawnPos, targetDist);
			}
		}
	}

	private void ResetCharge()
	{
		_isCharging = false;
		_chargeTime = 0.0f;
		_overchargeTime = 0.0f;
		_pulsePhase = 0.0f;
		if (Arrow != null)
		{
			Arrow.Scale = Vector2.One;
			Arrow.Modulate = new Color(1, 1, 1, 1);
		}
		if (TargetCircle != null) TargetCircle.Visible = false;
	}

	private void UpdateInventoryUI()
	{
		if (_inventoryUI == null) return;
		Control slots = _inventoryUI.GetNode<Control>("Slots");
		for (int i = 0; i < 5; i++)
		{
			ColorRect slot = slots.GetChild<ColorRect>(i);
			slot.Color = (i == (int)_currentType) ? new Color(0.8f, 0.8f, 0.1f) : new Color(0.3f, 0.3f, 0.3f);
		}
	}

	private void FireProjectile(Vector2 dir, Vector2 pos, float dist)
	{
		if (ProjectileScene == null) return;
		Projectile p = ProjectileScene.Instantiate<Projectile>();
		GetParent().AddChild(p);
		p.GlobalPosition = pos;
		p.Direction = dir;
		p.Rotation = dir.Angle();
	}

	private void FireStone(Vector2 dir, Vector2 pos, float targetDist)
	{
		if (StoneScene == null) return;
		Stone s = StoneScene.Instantiate<Stone>();
		
		s.GlobalPosition = pos;
		s.Direction = dir;
		s.TargetDistance = targetDist;
		s.Rotation = dir.Angle();
		
		GetParent().AddChild(s);
	}

	public void SetBunkerActive(bool active)
	{
		_isActive = active;
		if (Arrow != null) Arrow.Visible = active;
		if (TargetCircle != null) TargetCircle.Visible = false;

		if (active && _playerRef != null)
			_playerRef.GlobalPosition = new Vector2(-10000, -10000);
		else if (!active && _playerRef != null)
			_playerRef.GlobalPosition = GlobalPosition + (_currentDirection * 60.0f);
	}

	public void OnBodyEntered(Node2D body)
	{
		if (body is Player player && GameManager.Instance.CurrentState == GameState.Exploration)
		{
			_playerRef = player;
			Vector2 ev = (GlobalPosition - player.GlobalPosition).Normalized();
			_currentDirection = (Mathf.Abs(ev.X) > Mathf.Abs(ev.Y)) ? new Vector2(Mathf.Sign(ev.X), 0) : new Vector2(0, Mathf.Sign(ev.Y));
			if (Arrow != null)
			{
				Arrow.Rotation = _currentDirection.Angle();
				Arrow.Position = Vector2.Zero;
			}
			GameManager.Instance.ChangeState(GameState.BunkerMode);
		}
	}
}
