using Godot;
using System;

public partial class Player : CharacterBody2D
{
	[Export]
	public float Speed = 400.0f;

	public override void _PhysicsProcess(double delta)
	{
		Vector2 velocity = Velocity;

		// Obtenemos el vector de dirección basado en las acciones configuradas
		Vector2 direction = Input.GetVector("move_left", "move_right", "move_up", "move_down");
		
		if (direction != Vector2.Zero)
		{
			velocity = direction * Speed;
			// Rotamos el personaje hacia la dirección del movimiento
			Rotation = direction.Angle();
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
