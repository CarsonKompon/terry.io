using Sandbox;
using System;

public sealed class Player : Component, Component.ITriggerListener
{
	public static Player Local => GameManager.ActiveScene.GetAllComponents<Player>().FirstOrDefault( x => !x.IsProxy );

	[Sync] public int Score { get; set; }
	[Sync] public int TopScore { get; set; }
	public float Speed => 450f / Transform.Scale.x;
	public float Scale => MathX.Clamp( 1 + TopScore / 50f, 1, 25 );

	List<GameObject> ObjectsInRange = new();

	protected override void OnStart()
	{
		Score = 0;
		TopScore = 0;
		ObjectsInRange.Clear();
	}

	protected override void OnUpdate()
	{
		if ( IsProxy ) return;

		// Get mouse input
		var centerX = Screen.Width / 2f;
		var centerY = Screen.Height / 2f;
		var mouse = new Vector2( (Mouse.Position.x - centerX) / 200f, (Mouse.Position.y - centerY) / 200f ).Normal;

		// Move player
		var pos = Transform.Position;
		pos += new Vector3( mouse.y, mouse.x, 0 ) * Time.Delta * Speed;
		pos.x = MathX.Clamp( pos.x, -5000, 5000 );
		pos.y = MathX.Clamp( pos.y, -5000, 5000 );
		Transform.Position = pos;

		// Scale the player
		var currentScale = Transform.Scale.x;
		currentScale = MathX.LerpTo( currentScale, Scale, Time.Delta * 2 );
		Transform.Scale = new Vector3( currentScale, currentScale, currentScale );

		// Move camera to player
		Scene.Camera.Transform.Position = Transform.Position.WithZ( 200 );

		// Check if we can eat anything in range
		foreach ( var obj in ObjectsInRange )
		{
			if ( obj.Transform.Position.Distance( Transform.Position ) > (64f * currentScale) ) continue;
			if ( obj.Components.Get<Pellet>() is Pellet pellet )
			{
				Eat( pellet.Value );
				pellet.DestroyMe();
			}
			if ( obj.Components.GetInParentOrSelf<Player>() is Player other )
			{
				if ( other.Scale <= (Scale * 0.85f) )
				{
					Eat( other.Score );
					other.Kill( Network.OwnerConnection.SteamId );
				}
			}
		}
	}

	public void Eat( int value )
	{
		Score += value;
		TopScore = Math.Max( Score, TopScore );
	}

	[Broadcast]
	public void Kill( ulong killer )
	{
		if ( !IsProxy )
		{
			GameHud.Instance.ShowGameOver( killer );
		}
		GameObject.Destroy();
	}

	public void OnTriggerEnter( Collider other )
	{
		if ( !ObjectsInRange.Contains( other.GameObject ) )
			ObjectsInRange.Add( other.GameObject );
	}

	public void OnTriggerExit( Collider other )
	{
		if ( ObjectsInRange.Contains( other.GameObject ) )
			ObjectsInRange.Remove( other.GameObject );
	}
}