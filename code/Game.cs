using Sandbox;

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
}
