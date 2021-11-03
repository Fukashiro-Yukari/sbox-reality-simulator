using Sandbox;
using System;
using System.Linq;

public class RealityGame : Game
{
	public RealityGame()
	{
		if ( IsServer )
			_ = new RealityHud();
	}

	public override void ClientJoined( Client cl )
	{
		base.ClientJoined( cl );

		var player = new RealityPlayer( cl );
		player.Respawn();

		cl.Pawn = player;
	}

	public override void ClientDisconnect( Client cl, NetworkDisconnectionReason reason )
	{
		if ( cl.Pawn.IsValid() && cl.Pawn is RealityPlayer ply )
			ply.BecomeRagdoll();

		base.ClientDisconnect( cl, reason );
	}

	public override void MoveToSpawnpoint( Entity pawn )
	{
		base.MoveToSpawnpoint( pawn );

		if ( pawn is RealityPlayer ply )
			ply.RespawnPoint = pawn.Transform.Position;
	}

	[ServerCmd( "spawn_entity" )]
	public static void SpawnEntity( string entName )
	{
		var owner = ConsoleSystem.Caller.Pawn;

		if ( owner == null )
			return;

		var attribute = Library.GetAttribute( entName );

		if ( attribute == null || !attribute.Spawnable )
			return;

		if ( owner is not RealityPlayer ply ) return;

		var tr = Trace.Ray( ply.RespawnPoint + Vector3.Up * 2, ply.RespawnPoint + Vector3.Up * 500 )
			.UseHitboxes()
			.Ignore( ply )
			.Size( 2 )
			.Run();

		var ent = Library.Create<Entity>( entName );

		ent.Position = tr.EndPos;
		ent.PlaySound( "falling-3" );
	}
}
