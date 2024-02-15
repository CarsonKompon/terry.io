using System;
using Sandbox;

public sealed class Pellet : Component
{
	[Sync] public int Value { get; set; } = 1;
	[Property] ModelRenderer Renderer { get; set; }

	public static int TotalCount { get; private set; } = 0;

	protected override void OnStart()
	{
		TotalCount++;
		Renderer.MaterialOverride = Renderer.MaterialOverride.CreateCopy();
		Renderer.MaterialOverride.Set( "g_vTint", new ColorHsv( Random.Shared.Float( 0, 360 ), 1, 1 ).ToColor() );
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