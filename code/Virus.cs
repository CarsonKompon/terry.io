using System;
using Sandbox;

public sealed class Virus : Component
{
	[Property] ModelRenderer Renderer { get; set; }
	[Property] GameObject PelletPrefab { get; set; }

	public static int TotalCount { get; private set; } = 0;

	int BurstCount = 0;
	TimeSince timeSinceLastBurst = 0f;

	protected override void OnStart()
	{
		TotalCount++;
		Renderer.MaterialOverride = Renderer.MaterialOverride.CreateCopy();
		Renderer.MaterialOverride.Set( "g_vTint", Color.Green );
	}

	protected override void OnUpdate()
	{
		if ( IsProxy ) return;

		foreach ( var player in Scene.GetAllComponents<Player>() )
		{
			if ( player.Score > 132 && player.Transform.Position.Distance( Transform.Position ) < 64f * player.Scale )
			{
				player.Eat( 100 );
				DestroyMe();
				break;
			}
			else if ( player.Score < 132 && player.Transform.Position.Distance( Transform.Position ) < 192f )
			{
				player.Kill( player.Network.OwnerConnection.SteamId );
				StartBurst();
				break;
			}
		}

		if ( timeSinceLastBurst > 0.33f && BurstCount > 0 )
		{
			SpawnPellet();
			BurstCount--;
			timeSinceLastBurst = 0f;
		}
	}

	void StartBurst()
	{
		BurstCount += Random.Shared.Int( 10, 20 );
	}

	void SpawnPellet()
	{
		var pellet = PelletPrefab.Clone( Transform.World );
		pellet.NetworkSpawn( null );
		var pelletScript = pellet.Components.Get<Pellet>();
		pelletScript.Velocity = new Vector3( Random.Shared.Float( -1, 1 ), Random.Shared.Float( -1, 1 ), 0 ).Normal * Random.Shared.Float( 700f, 1100f );
	}

	[Broadcast]
	public void DestroyMe()
	{
		TotalCount--;
		GameObject.Destroy();
	}

	public static void ResetCount()
	{
		TotalCount = 0;
	}
}