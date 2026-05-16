using Godot;
using System;

public partial class Player : CharacterBody2D
{
	[Export]
	public float Speed = 400.0f;

	public override void _PhysicsProcess(double delta)
	{
		Vector2 velocity = Velocity;

		// Obtenemos el vector de dirección a través del InputManager
		Vector2 direction = InputManager.Instance.GetMovementDirection();
		
		if (direction != Vector2.Zero)
		{
			velocity = direction * Speed;
		}
		else
		{
			// Frenado suave
			velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);
			velocity.Y = Mathf.MoveToward(Velocity.Y, 0, Speed);
		}

		Velocity = velocity;
		MoveAndSlide();
	}
}
