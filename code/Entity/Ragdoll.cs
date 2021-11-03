using Sandbox;
using System.Threading.Tasks;

partial class Ragdoll : AnimEntity
{
	RealityPlayer player;
	bool isBecome;
	bool isDeath;
	TimeSince timeSinceTakeDamage;
	TimeSince timeSinceScream;

	public Ragdoll() : base()
	{
	}

	public Ragdoll( RealityPlayer player ) : base()
	{
		this.player = player;
	}

	public void SetPlayer( RealityPlayer player )
	{
		this.player = player;
	}

	public bool IsValidPlayer()
	{
		return player.IsValid();
	}

	public void PainSound()
	{
		if ( !IsValidPlayer() ) return;
		if ( timeSinceScream < 0.8 || player.LifeState != LifeState.Alive || isDeath ) return;
		timeSinceScream = 0;

		if ( player.IsValid() )
			player.Position = Position;

		PlaySound( $"pain-{Rand.Int( 1, 10 )}" );
	}

	public void DeathSound()
	{
		if ( !IsValidPlayer() ) return;
		if ( isDeath ) return;
		if ( player.IsValid() )
			player.Position = Position;

		PlaySound( $"death-{Rand.Int( 1, 4 )}" );
	}

	public void FallingSound()
	{
		if ( !IsValidPlayer() ) return;
		if ( timeSinceScream < 10 || isDeath ) return;
		timeSinceScream = 0;

		if ( player.IsValid() )
			player.Position = Position;

		PlaySound( $"falling-{Rand.Int( 1, 4 )}" );
	}

	async Task BecomePlayer()
	{
		if ( isDeath || !IsValidPlayer() ) return;
		if ( player.LifeState != LifeState.Alive )
		{
			DeathSound();

			isDeath = true;

			_ = DeleteAsync( 120f );
		}

		if ( isBecome ) return;
		isBecome = true;

		await GameTask.DelaySeconds( 3f );

		if ( !player.IsValid() ) return;
		if ( isDeath )
		{
			player.Ragdoll = null;
			player = null;

			return;
		}

		player.BecomePlayer();
	}

	async Task PlayerDeath()
	{
		if ( isDeath || !IsValidPlayer() ) return;

		DeathSound();

		isDeath = true;

		_ = DeleteAsync( 120f );

		await GameTask.DelaySeconds( 3f );

		if ( !IsValidPlayer() ) return;

		player.Ragdoll = null;
		player = null;
	}

	public override void Spawn()
	{
		base.Spawn();

		Health = 1;
	}

	public override void TakeDamage( DamageInfo info )
	{
		if ( player.IsValid() )
			player.TakeDamage( info );

		PainSound();
	}

	public override void OnKilled()
	{
	}

	protected override void OnPhysicsCollision( CollisionEventData eventData )
	{
		base.OnPhysicsCollision( eventData );

		if ( eventData.Entity is RealityPlayer ply && Velocity.Length > 50 )
			ply.BecomeRagdoll( eventData.PreVelocity );

		if ( !IsValidPlayer() ) return;
		if ( player.LifeState != LifeState.Alive )
		{
			_ = PlayerDeath();

			return;
		}

		var speed = eventData.Speed * 2;

		if ( speed > 1100 && timeSinceTakeDamage > 0.5 )
		{
			timeSinceTakeDamage = 0;

			var damage = new DamageInfo()
			{
				Body = PhysicsBody,
				Flags = DamageFlags.Fall,
				Damage = eventData.Speed / 8,
				Position = Position,
				Attacker = player,
			};

			player.TakeDamage( damage );
		}

		if ( speed > 300 && player.LifeState == LifeState.Alive && !isDeath )
			PainSound();

		if ( Velocity.Length < 20 )
			_ = BecomePlayer();
	}

	[Event.Tick.Server]
	private void Tick()
	{
		if ( !IsValidPlayer() ) return;
		if ( WaterLevel.Entity != null )
			_ = BecomePlayer();

		if ( player.LifeState != LifeState.Alive )
			_ = PlayerDeath();

		if ( Velocity.Length > 800 )
			FallingSound();
	}
}
