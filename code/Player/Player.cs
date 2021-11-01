using Sandbox;

partial class RealityPlayer : Player
{
	private TimeSince timeSinceDropped;

	private DamageInfo lastDamage;

	[Net] public PawnController VehicleController { get; set; }
	[Net] public PawnAnimator VehicleAnimator { get; set; }
	[Net] public Ragdoll Ragdoll { get; set; }
	[Net, Predicted] public ICamera VehicleCamera { get; set; }
	[Net, Predicted] public Entity Vehicle { get; set; }
	[Net, Predicted] public ICamera MainCamera { get; set; }

	public ICamera LastCamera { get; set; }
	public Clothing.Container Clothing = new();

	public RealityPlayer()
	{
		Inventory = new Inventory( this );
	}

	public RealityPlayer( Client cl ) : this()
	{
		// Load clothing from client data
		Clothing.LoadFromClient( cl );
	}

	public override void Spawn()
	{
		MainCamera = new RealityCamera();
		LastCamera = MainCamera;

		base.Spawn();
	}

	public override void Respawn()
	{
		SetModel( "models/citizen/citizen.vmdl" );

		Controller = new WalkController();
		Animator = new StandardPlayerAnimator();

		MainCamera = LastCamera;
		Camera = MainCamera;

		if ( DevController is NoclipController )
		{
			DevController = null;
		}

		EnableAllCollisions = true;
		EnableDrawing = true;
		EnableHideInFirstPerson = true;
		EnableShadowInFirstPerson = true;

		Clothing.DressEntity( this );

		string[] weps = { "weapon_crossbow", "weapon_pumpshotgun", "weapon_smg" };

		Inventory.Add( Library.Create<Weapon>( weps[Rand.Int( 0, weps.Length - 1 )] ) );
		Inventory.Add( new Pistol() );
		Inventory.Add( new Fists() );
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

		VehicleController = null;
		VehicleAnimator = null;
		VehicleCamera = null;
		Vehicle = null;

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
			BecomeRagdoll( Velocity, lastDamage.Flags, lastDamage.Position, lastDamage.Force, GetHitboxBone( lastDamage.HitboxIndex ) );
	}

	public override PawnController GetActiveController()
	{
		if ( VehicleController != null ) return VehicleController;
		if ( DevController != null ) return DevController;

		return base.GetActiveController();
	}

	public override PawnAnimator GetActiveAnimator()
	{
		if ( VehicleAnimator != null ) return VehicleAnimator;

		return base.GetActiveAnimator();
	}

	public ICamera GetActiveCamera()
	{
		if ( VehicleCamera != null ) return VehicleCamera;

		return MainCamera;
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

		if ( VehicleController != null && DevController is NoclipController )
		{
			DevController = null;
		}

		var controller = GetActiveController();
		if ( controller != null )
			EnableSolidCollisions = !controller.HasTag( "noclip" );

		if (Ragdoll == null )
		{
			TickPlayerUse();
			SimulateActiveChild( cl, ActiveChild );

			if ( Input.Pressed( InputButton.View ) )
				BecomeRagdoll( Velocity );

			Camera = GetActiveCamera();

			if ( Input.Pressed( InputButton.Drop ) )
			{
				var dropped = Inventory.DropActive();
				if ( dropped != null )
				{
					dropped.PhysicsGroup.ApplyImpulse( Velocity + EyeRot.Forward * 80.0f + Vector3.Up * 100.0f, true );
					dropped.PhysicsGroup.ApplyAngularImpulse( Vector3.Random * 100.0f, true );

					timeSinceDropped = 0;
				}
			}
		}

		var DownVel = Velocity * Rotation.Down;

		if ( DownVel.z > 350 && controller != null && !controller.HasTag( "noclip" ) && Ragdoll == null )
			BecomeRagdoll( Velocity );

		if ( controller != null && !controller.HasTag( "noclip" ) )
		{
			var startpos = Position + Vector3.Up * 4;
			var endpos = startpos + Rotation.Forward * 30;

			//DebugOverlay.Line( startpos, endpos );

			var tr = Trace.Ray( startpos, endpos )
				.Ignore( this )
				.HitLayer( CollisionLayer.All, false )
				.HitLayer( CollisionLayer.STATIC_LEVEL )
				.HitLayer( CollisionLayer.Solid )
				.Run();

			if ( tr.Hit && controller.GroundEntity == null && Velocity.Length > 300 )
				BecomeRagdoll( Velocity );

			//DebugOverlay.ScreenText( new Vector2( 200, 250 ), $"{Health} | {Velocity.Length}" );
		}
	}

	public void ResetDroppedTime()
	{
		timeSinceDropped = 0;
	}

	public override void StartTouch( Entity other )
	{
		if ( timeSinceDropped < 1 ) return;

		base.StartTouch( other );
	}

	[ServerCmd( "inventory_current" )]
	public static void SetInventoryCurrent( string entName )
	{
		var target = ConsoleSystem.Caller.Pawn;
		if ( target == null ) return;

		var inventory = target.Inventory;
		if ( inventory == null )
			return;

		for ( int i = 0; i < inventory.Count(); ++i )
		{
			var slot = inventory.GetSlot( i );
			if ( !slot.IsValid() )
				continue;

			if ( !slot.ClassInfo.IsNamed( entName ) )
				continue;

			inventory.SetActiveSlot( i, false );

			break;
		}
	}

	// TODO

	//public override bool HasPermission( string mode )
	//{
	//	if ( mode == "noclip" ) return true;
	//	if ( mode == "devcam" ) return true;
	//	if ( mode == "suicide" ) return true;
	//
	//	return base.HasPermission( mode );
	//	}
}
