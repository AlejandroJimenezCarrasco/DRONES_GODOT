using Godot;
using System;

public partial class Stone : Area2D
{
	[Export] public float BaseSpeed = 400.0f;
	public Vector2 Direction = Vector2.Zero;
	public float TargetDistance = 0.0f;

	private float _elapsedTime = 0.0f;
	private float _totalDuration = 0.0f;
	private Vector2 _startPosition;
	
	private Node2D _visual;
	private Node2D _shadow;

	public override void _Ready()
	{
		_visual = GetNode<Node2D>("Visual");
		_shadow = GetNode<Node2D>("Shadow");
		_startPosition = GlobalPosition;

		// Calculamos la duración total del viaje basándonos en la distancia
		// Usamos una velocidad media para el cálculo
		_totalDuration = Mathf.Max(0.6f, TargetDistance / BaseSpeed);
	}

	public override void _PhysicsProcess(double delta)
	{
		_elapsedTime += (float)delta;
		float t = Mathf.Clamp(_elapsedTime / _totalDuration, 0.0f, 1.0f);

		// LOGICA DE MOVIMIENTO (Acelera - Frena - Acelera)
		// Usamos una curva personalizada para la velocidad: 1 -> 0.1 -> 1
		float speedMult = 1.0f - (Mathf.Sin(t * Mathf.Pi) * 0.9f);
		
		// Calculamos la posición basada en la integración de la velocidad
		// Para simplificar y que llegue exacto al sitio, usamos una interpolación suave
		// pero con el feeling de la velocidad variable.
		float distanceFactor = t - (Mathf.Sin(t * 2 * Mathf.Pi) / (2 * Mathf.Pi));
		GlobalPosition = _startPosition + Direction * (distanceFactor * TargetDistance);

		// LOGICA VISUAL (Fingir altura)
		float heightCurve = Mathf.Sin(t * Mathf.Pi);
		
		// 1. Escala (Simula cercanía a cámara al subir)
		float scale = 1.0f + (heightCurve * 0.4f);
		_visual.Scale = new Vector2(scale, scale);

		// 2. Posición Visual (SUBE hacia arriba en la pantalla)
		// El nodo Visual se desplaza hacia -Y, mientras que el Area2D sigue en el "suelo"
		_visual.Position = new Vector2(0, -heightCurve * 80.0f);

		// 3. Sombra (Se queda en el suelo, pero se encoge un poco al alejarse el objeto)
		_shadow.Position = Vector2.Zero; // Siempre en la base del proyectil
		float shadowScale = 1.0f - (heightCurve * 0.4f);
		_shadow.Scale = new Vector2(shadowScale, shadowScale * 0.5f);

		if (t >= 1.0f)
		{
			QueueFree();
		}
	}

	private void OnAreaEntered(Area2D area)
	{
		if (area is Drone drone)
		{
			drone.Die();
			QueueFree();
		}
	}
}
