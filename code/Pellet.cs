using System;
using Sandbox;

public sealed class Pellet : Component
{
	[Sync] public int Value { get; set; } = 1;
	[Sync] public Vector3 Velocity { get; set; } = Vector3.Zero;
	[Property] ModelRenderer Renderer { get; set; }

	public static int TotalCount { get; private set; } = 0;

	protected override void OnStart()
	{
		TotalCount++;
		Renderer.MaterialOverride = Renderer.MaterialOverride.CreateCopy();
		Renderer.MaterialOverride.Set( "g_vTint", new ColorHsv( Random.Shared.Float( 0, 360 ), 1, 1 ).ToColor() );
	}

	protected override void OnUpdate()
	{
		if ( IsProxy ) return;

		Transform.Position += Velocity * Time.Delta;
		Velocity = Velocity.LerpTo( Vector3.Zero, Time.Delta * 2 );
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