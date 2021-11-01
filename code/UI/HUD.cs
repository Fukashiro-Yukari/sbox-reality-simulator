using Sandbox;
using Sandbox.UI;

[Library]
public partial class RealityHud : HudEntity<RootPanel>
{
	public RealityHud()
	{
		if ( !IsClient )
			return;

		RootPanel.StyleSheet.Load( "/UI/HUD.scss" );

		RootPanel.AddChild<Black>();
		RootPanel.AddChild<ChatBox>();
		RootPanel.AddChild<VoiceList>();
		RootPanel.AddChild<Scoreboard<ScoreboardEntry>>();
		RootPanel.AddChild<InventoryBar>();
	}
}
