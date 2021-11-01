using Sandbox;
using System.Threading.Tasks;

partial class Ragdoll : AnimEntity
{
	RealityPlayer player;
	bool isBecome;
	bool isDeath;
	TimeSince timeSinceTakeDamage;

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

	async Task BecomePlayer()
	{
		if ( isDeath || !player.IsValid() ) return;
		if ( player.LifeState != LifeState.Alive )
		{
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
		if ( isDeath || !player.IsValid() ) return;

		isDeath = true;

		_ = DeleteAsync( 120f );

		await GameTask.DelaySeconds( 3f );

		if ( !player.IsValid() ) return;

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
		player?.TakeDamage( info );
	}

	public override void OnKilled()
	{
	}

	protected override void OnPhysicsCollision( CollisionEventData eventData )
	{
		base.OnPhysicsCollision( eventData );

		if ( !player.IsValid() ) return;
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

		if ( Velocity.Length < 10 )
			_ = BecomePlayer();
	}

	[Event.Tick.Server]
	private void Tick()
	{
		if ( !player.IsValid() ) return;
		if ( WaterLevel.Entity != null )
			_ = BecomePlayer();

		if ( player.LifeState != LifeState.Alive )
			_ = PlayerDeath();
	}
}
