using Sandbox;

[Spawnable]
[Library( "weapon_fists", Title = "Fists" )]
partial class Fists : WeaponMelee
{
	public override string ViewModelPath => "models/first_person/first_person_arms.vmdl";

	public override int Bucket => 2;
	public override float PrimarySpeed => 0.5f;
	public override float SecondarySpeed => 0.5f;
	public override float PrimaryDamage => 25f;
	public override float PrimaryForce => 100f;
	public override float SecondaryDamage => 25f;
	public override float SecondaryForce => 100f;
	public override float PrimaryMeleeDistance => 80f;
	public override float SecondaryMeleeDistance => 80f;
	public override float ImpactSize => 20f;
	public override CType Crosshair => CType.None;
	public override string Icon => "ui/weapons/weapon_fists.png";
	public override string PrimaryAnimationHit => "attack";
	public override string PrimaryAnimationMiss => "attack";
	public override string SecondaryAnimationHit => "attack";
	public override string SecondaryAnimationMiss => "attack";
	public override bool CanUseSecondary => true;

	public override void OnCarryDrop( Entity dropper )
	{
	}

	public override void SimulateAnimator( PawnAnimator anim )
	{
		anim.SetAnimParameter( "holdtype", 5 );
		anim.SetAnimParameter( "aim_body_weight", 1.0f );
	}
}
