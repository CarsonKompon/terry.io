using Sandbox;
using System;

public sealed class Player : Component, Component.ITriggerListener
{
	[Property] GameObject Body { get; set; }
	[Sync] public int Score { get; set; } = 10;
	public float Speed => (Body.Transform.Scale.x / MathF.Pow( Body.Transform.Scale.x, 1.44f )) * 640f;
	public float Scale => MathF.Sqrt( Score / 10f );

	Vector3 Velocity { get; set; } = Vector2.Zero;
	TimeUntil timeUntilMerge = 0f;
	List<GameObject> ObjectsInRange = new();

	protected override void OnStart()
	{
		timeUntilMerge = 30 + (Score * 0.02f);
		ObjectsInRange.Clear();
	}

	protected override void OnUpdate()
	{
		// Scale the player
		var currentScale = Body.Transform.Scale.x;
		currentScale = MathX.LerpTo( currentScale, Scale, Time.Delta * 2 );
		Body.Transform.Scale = new Vector3( currentScale, currentScale, currentScale );

		if ( IsProxy ) return;

		// Get mouse input
		var center = Transform.Position.WithZ( 0 );
		var mouseRay = Scene.Camera.ScreenPixelToRay( Mouse.Position );
		var mousePos = Scene.Trace.Ray( mouseRay, 10000f ).Run().HitPosition.WithZ( 0 );
		var mouse = new Vector2( (mousePos.x - center.x) / 200f, (mousePos.y - center.y) / 200f );
		if ( mouse.Length > 1f ) mouse = mouse.Normal;

		// Move player
		var pos = Transform.Position;
		Velocity = Velocity.LerpTo( mouse * Speed, Time.Delta * 10 );
		pos += new Vector3( Velocity.x, Velocity.y, 0 ) * Time.Delta;
		pos.x = MathX.Clamp( pos.x, -5000, 5000 );
		pos.y = MathX.Clamp( pos.y, -5000, 5000 );
		Transform.Position = pos;

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
				if ( timeUntilMerge > 0f && Network.OwnerId == other.Network.OwnerId ) continue;
				if ( other.Scale <= (Scale * 0.85f) || (Network.OwnerId == other.Network.OwnerId && other.timeUntilMerge <= 0f) )
				{
					Eat( other.Score );
					other.Kill( Network.OwnerConnection.SteamId );
				}
			}
		}

		// Push out of friendlies
		if ( timeUntilMerge > 0f )
		{
			foreach ( var other in Scene.GetAllComponents<Player>() )
			{
				if ( other == this ) continue;
				if ( other.Network.OwnerId != Network.OwnerId ) continue;
				var dist = other.Transform.Position.Distance( Transform.Position );
				if ( dist < (128f * currentScale) )
				{
					var dir = (Transform.Position - other.Transform.Position).Normal;
					Velocity += dir * Speed * 40f * Time.Delta;
				}
			}
		}

		// Split
		if ( Input.Pressed( "Jump" ) )
		{
			if ( Score >= 35 )
			{
				int half = (int)MathF.Floor( Score / 2f );
				Score -= half;
				Transform.Scale = new Vector3( Scale, Scale, Scale );
				var split = GameObject.Clone( Transform.World );
				split.NetworkSpawn();
				var splitPlayer = split.Components.Get<Player>();
				splitPlayer.Transform.Scale = new Vector3( splitPlayer.Scale, splitPlayer.Scale, splitPlayer.Scale );
				splitPlayer.Score = half;
				splitPlayer.timeUntilMerge = 30 + (half * 0.02f);
				splitPlayer.Velocity = Velocity * 12f;
				ObjectsInRange.Add( split );
			}
		}
	}

	public void Eat( int value )
	{
		Score += value;
	}

	[Broadcast]
	public void Kill( ulong killer )
	{
		if ( !IsProxy && Scene.GetAllComponents<Player>().Where( x => x.Network.OwnerId == Network.OwnerId ).Count() <= 1 )
		{
			GameHud.Instance.ShowGameOver( killer );
		}

		GameObject.DestroyImmediate();
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

	public static Dictionary<Connection, int> GetScores()
	{
		var scores = new Dictionary<Connection, int>();
		foreach ( var player in GameManager.ActiveScene.GetAllComponents<Player>() )
		{
			if ( scores.ContainsKey( player.Network.OwnerConnection ) )
			{
				scores[player.Network.OwnerConnection] += player.Score;
			}
			else
			{
				scores.Add( player.Network.OwnerConnection, player.Score );
			}
		}
		scores = scores.OrderByDescending( x => x.Value ).ToDictionary( x => x.Key, x => x.Value );
		return scores;
	}

	public static int GetLocalScore()
	{
		int score = 0;
		foreach ( var player in GameManager.ActiveScene.GetAllComponents<Player>() )
		{
			if ( player.Network.OwnerId == Connection.Local.Id )
			{
				score += player.Score;
			}
		}
		return score;
	}
}