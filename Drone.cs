using Godot;
using System;

public partial class Drone : Area2D
{
	[Export] public float Speed = 25.0f;
	[Export] public PackedScene ExplosionScene;
	private Vector2 _targetPosition;

	public override void _Ready()
	{
		_targetPosition = new Vector2(576, 324);
		LookAt(_targetPosition);
		
		BodyEntered += OnBodyEntered;
		AreaEntered += OnAreaEntered;
	}

	public override void _PhysicsProcess(double delta)
	{
		if (GameManager.Instance.CurrentState == GameState.GameOver) return;

		Vector2 direction = (_targetPosition - GlobalPosition).Normalized();
		GlobalPosition += direction * Speed * (float)delta;
		Rotation = direction.Angle();
	}

	private void OnBodyEntered(Node2D body)
	{
		if (body is Bunker)
		{
			GameManager.Instance.ChangeState(GameState.GameOver);
			QueueFree();
		}
	}

	private void OnAreaEntered(Area2D area)
	{
		// Detectamos el misil azul o la nueva piedra
		if (area is Projectile || area is Stone)
		{
			Die();
			area.QueueFree();
		}
	}

	public void Die()
	{
		if (ExplosionScene != null)
		{
			Node2D explosion = ExplosionScene.Instantiate<Node2D>();
			explosion.GlobalPosition = GlobalPosition;
			GetParent().AddChild(explosion);
		}
		
		GD.Print("Drone destruido");
		QueueFree();
	}
}
