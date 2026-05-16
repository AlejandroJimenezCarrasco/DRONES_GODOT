using Godot;
using System;

public partial class Projectile : Area2D
{
	[Export] public float Speed = 600.0f;
	public Vector2 Direction = Vector2.Zero;

	public override void _PhysicsProcess(double delta)
	{
		if (Direction != Vector2.Zero)
		{
			Position += Direction * Speed * (float)delta;
		}
	}

	public void OnBodyEntered(Node2D body)
	{
		// Por ahora solo imprimimos y destruimos el proyectil
		GD.Print("Proyectil impactó con: " + body.Name);
		QueueFree();
	}

	public void OnScreenExited()
	{
		QueueFree();
	}
}
