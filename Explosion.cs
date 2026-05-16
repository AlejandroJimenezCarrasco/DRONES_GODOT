using Godot;
using System;

public partial class Explosion : CpuParticles2D
{
	public override void _Ready()
	{
		Emitting = true;
		// Destruir el nodo después de que termine la animación
		GetTree().CreateTimer(Lifetime).Timeout += () => QueueFree();
	}
}
