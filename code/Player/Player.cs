using Sandbox;
using System.Collections.Generic;

partial class RealityPlayer : Player
{
	private TimeSince timeSinceDropped;

	private DamageInfo lastDamage;

	[Net] public Ragdoll Ragdoll { get; set; }

	public ClothingContainer Clothing = new();
	public Vector3 RespawnPoint { get; set; }

	public RealityPlayer()
	{
		Inventory = new Inventory( this );
	}

	public RealityPlayer( Client cl ) : this()
	{
		// Load clothing from client data
		Clothing.LoadFromClient( cl );
	}

	public override void Respawn()
	{
		SetModel( "models/citizen/citizen.vmdl" );

		Controller = new WalkController();
		Animator = new StandardPlayerAnimator();

		CameraMode = new RealityCamera();

		if ( DevController is NoclipController )
		{
			DevController = null;
		}

		EnableAllCollisions = true;
		EnableDrawing = true;
		EnableHideInFirstPerson = true;
		EnableShadowInFirstPerson = true;

		Clothing.DressEntity( this );

		Inventory.Add( new Fists(), true );
		Inventory.Add( new Flashlight() );

		base.Respawn();
	}

	public override void OnKilled()
	{
		if ( Ragdoll.IsValid() )
			Position = Ragdoll.Position;

		base.OnKilled();

		if ( lastDamage.Flags.HasFlag( DamageFlags.Vehicle ) )
		{
			Particles.Create( "particles/impact.flesh.bloodpuff-big.vpcf", lastDamage.Position );
			Particles.Create( "particles/impact.flesh-big.vpcf", lastDamage.Position );
			PlaySound( "kersplat" );
		}

		BecomeRagdoll( Velocity, lastDamage.Flags, lastDamage.Position, lastDamage.Force, GetHitboxBone( lastDamage.HitboxIndex ) );

		if ( Inventory is Inventory inv )
			inv.DropAll();

		Inventory.DeleteContents();
	}

	public override void TakeDamage( DamageInfo info )
	{
		if ( GetHitboxGroup( info.HitboxIndex ) == 1 )
		{
			info.Damage *= 2.0f;
		}

		lastDamage = info;

		base.TakeDamage( info );

		if ( info.Flags != DamageFlags.Fall )
			BecomeRagdoll( Velocity, lastDamage.Flags, lastDamage.Position, lastDamage.Force, GetHitboxBone( lastDamage.HitboxIndex ), true );
	}

	public override PawnController GetActiveController()
	{
		if ( DevController != null ) return DevController;

		return base.GetActiveController();
	}

	string oldavatar = ConsoleSystem.GetValue( "avatar" );

	public override void Simulate( Client cl )
	{
		base.Simulate( cl );

		if ( Input.ActiveChild != null )
		{
			ActiveChild = Input.ActiveChild;
		}

		if ( LifeState != LifeState.Alive )
			return;

		var newavatar = ConsoleSystem.GetValue( "avatar" );

		if ( oldavatar != newavatar )
		{
			oldavatar = newavatar;

			Clothing.LoadFromClient( Client );
			Clothing.DressEntity( this );
		}

		var controller = GetActiveController();
		if ( controller != null )
			EnableSolidCollisions = !controller.HasTag( "noclip" );

		if ( Ragdoll == null )
		{
			TickPlayerUse();
			SimulateActiveChild( cl, ActiveChild );

			if ( Input.Pressed( InputButton.View ) )
				BecomeRagdoll();

			if ( Input.Pressed( InputButton.Drop ) )
			{
				var dropped = Inventory.DropActive();
				if ( dropped != null )
				{
					dropped.PhysicsGroup.ApplyImpulse( Velocity + EyeRotation.Forward * 80.0f + Vector3.Up * 100.0f, true );
					dropped.PhysicsGroup.ApplyAngularImpulse( Vector3.Random * 100.0f, true );

					timeSinceDropped = 0;
				}
			}
		}

		if ( Velocity.Length > 500 && controller != null && !controller.HasTag( "noclip" ) && Ragdoll == null )
			BecomeRagdoll();

		if ( controller != null && !controller.HasTag( "noclip" ) )
		{
			List<Vector3> vecs = new() { Rotation.Forward, Rotation.Backward, Rotation.Left, Rotation.Right };
			var dist = 22f;
			var len = 250f;

			foreach ( var vec in vecs )
			{
				var startpos = Position + Vector3.Up * 3;
				var EndPosition = startpos + vec * dist;

				var tr = Trace.Ray( startpos, EndPosition )
					.UseHitboxes()
					.Ignore( this )
					.Size( 5 )
					.Run();

				//DebugOverlay.TraceResult( tr );

				if ( tr.Hit && controller.GroundEntity == null && Velocity.Length > len )
					BecomeRagdoll();
			}

			foreach ( var vec in vecs )
			{
				var startpos = EyePosition;
				var EndPosition = startpos + vec * dist;

				var tr = Trace.Ray( startpos, EndPosition )
					.UseHitboxes()
					.Ignore( this )
					.Size( 5 )
					.Run();

				//DebugOverlay.TraceResult( tr );

				if ( tr.Hit && controller.GroundEntity == null && Velocity.Length > len )
					BecomeRagdoll();
			}
		}

		//DebugOverlay.ScreenText( new Vector2( 200, 250 ), $"{Health} | {Velocity.Length}" );
	}

	public override void StartTouch( Entity other )
	{
		var controller = GetActiveController();
		var downVel = Velocity * Vector3.Down;

		if ( other is not Weapon && controller != null && !controller.HasTag( "noclip" ) && controller.GroundEntity == null && Velocity.Length > 250 && downVel.z < 135 )
			BecomeRagdoll();

		if ( timeSinceDropped < 0.1 ) return;

		base.StartTouch( other );
	}

	[ConCmd.Server( "inventory_current" )]
	public static void SetInventoryCurrent( string entName )
	{
		var target = ConsoleSystem.Caller.Pawn as Player;
		if ( target == null ) return;

		var inventory = target.Inventory;
		if ( inventory == null )
			return;

		for ( int i = 0; i < inventory.Count(); ++i )
		{
			var slot = inventory.GetSlot( i );
			if ( !slot.IsValid() )
				continue;

			if ( slot.ClassName != entName )
				continue;

			inventory.SetActiveSlot( i, false );

			break;
		}
	}

	//[Event("render.postprocess")]
	//public static void DrawPostProcess()
	//{
		//Render.BlendMode = BlendMode.AlphaBlend;

		//Render.CopyFrameBuffer();

		//Render.Material = Material.Load( "postprocess/standard.vpost" );
		//Render.DrawScreenQuad();

		//Render.Compute.Using
	//}
}
