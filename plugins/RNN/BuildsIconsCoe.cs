using Turbo.Plugins.Default;  
using System;
using System.Globalization;
using System.Linq;    
using System.Collections.Generic;
using SharpDX;
using SharpDX.Direct2D1;

namespace Turbo.Plugins.RNN
{        
	public class BuildsIconsAndCoe : BasePlugin, IInGameTopPainter, ICustomizer, INewAreaHandler
	{		
		private Dictionary<HeroClass,List<int>> AllBonusElements { get; set; }
		private int[] IndexToBonus { get; set; } = new int[8] { 7, 5, 3, 1, 6, 2, 0, 4 };
		
		private int Ticks0 { get; set; } = 0;
		private int Count0 { get; set; } = 0;		
		
		private float SizeIconWidth  { get; set; } 
		private float SizeIconHeight  { get; set; } 
		private SharpDX.DirectWrite.TextLayout layout { get; set; } = null;	  

		private IFont FontAS { get; set; }
		private IFont FontWhite { get; set; }
		private IFont FontGray { get; set; }
		private IFont FontGreen { get; set; }
		private IFont FontRed { get; set; }		
		private IFont FontYellow { get; set; }
		private IFont FontOrange { get; set; }
		private IFont FontBlue { get; set; }
		private IFont FontDefault { get; set; } = null;
		private IFont FontNames { get; set; }	
		private IFont FontLocust { get; set; }			

		private IFont FontStacks { get; set; }
		private IFont FontTimeLeft { get; set; }
		private IFont FontTimeLeft2 { get; set; }		
		private IFont FontStacksRed { get; set; }
		
        private IBrush TimeLeftClockBrush { get; set; }		
        private IBrush BrushBlack { get; set; }
        private IBrush BrushGreen { get; set; }
		private IBrush BrushRed { get; set; }		
        private IBrush BrushYellow { get; set; }
		private IBrush BrushOrange { get; set; }
		private IBrush BrushBlue { get; set; }
		private IBrush BrushDefault { get; set; } 
		
		private IFont FontCounter { get; set; }
		private IFont FontExpl { get; set; }
		private IFont FontLimit { get; set; }	
		
		
		public float Xpor  { get; set; }
		public float Ypor  { get; set; }		
		public float SizeMultiplier  { get; set; }
		public bool ShowCoe { get; set; }
		public bool OnlyGR { get; set; }
		public bool OnlyMe { get; set; } = false;
		public bool ShowMe { get; set; }
		public bool ShowOthers { get; set; }
		public float Opacity { get; set; }
		public bool InactiveRedFrame { get; set; }
		public bool ShowNames { get; set; }
		public int ProgressBarWidth { get; set; } = 2;	
		
		public bool Singularity { get; set; }
		public bool Nayr { get; set; }
		
		public bool SpiritBarrage { get; set; }
		public bool ShowGlobes { get; set; }
		public bool ShowBigBadVoodoo { get; set; }
		public bool ShowLocust { get; set; }
		public float SBWarning { get; set; }
		private Dictionary<uint,int> Phantasms { get;set; } = new Dictionary<uint,int>();		
		
		public BuildsIconsAndCoe()
		{
			Enabled = false;    
		}
		
		public override void Load(IController hud)
        {
            base.Load(hud);	
			Order = 30001;
			/* Necro */
			Singularity = true;			// Enable Icons Build Singularity	
			Nayr = true;				// Enable Icons Build Nayr	
			/* WitchDoctor	*/
			SpiritBarrage = true;		// Enable Icons Build Spirit Barrage	
			SBWarning = 2.0f;			// 9.0f...0f Text will take the color yellow when it reaches this value
			ShowGlobes = true;			// Health Globes Counter (floor)
			ShowBigBadVoodoo = true;	// Show Big Bad Voodoo
			ShowLocust = false;			// Locust Affected Monster Counters (Total \n Elites) at 40y
			
			/* Common */			
			ShowMe = true;				// Show for my character
			ShowOthers = true;			// Show for others character
			ShowNames = true;			// Show players names
			OnlyGR = false;				// Show  in GR only
			Xpor = 0.75f;				// To set the x coordinate of the icon
			Ypor = 0.055f;				// To set the y coordinate of the icon			
			SizeMultiplier = 1.0f;		// Size multiplier for icons
			Opacity = 0.75f;				// 0f..1f  Opacity for icon texture	
			ShowCoe = false;			// Show COE status 
			InactiveRedFrame = false;	// Inactive elements (coe) are shown in red (active in yellow)
			ProgressBarWidth = 2;		// Width progressbar Coe (pixels)
			
            TimeLeftClockBrush = Hud.Render.CreateBrush(220, 0, 0, 0, 0);	
            BrushBlack = Hud.Render.CreateBrush(255, 80, 80, 80, 0);
            BrushGreen = Hud.Render.CreateBrush(255, 0, 255 , 0, 0);
			BrushYellow = Hud.Render.CreateBrush(255, 255, 255 , 0, 0);
			BrushOrange = Hud.Render.CreateBrush(255, 255, 185 , 0, 0);
			BrushBlue = Hud.Render.CreateBrush(255, 50, 150 , 250, 0);	

            AllBonusElements = new Dictionary<HeroClass, List<int>>	// 1 = Arcane, 2 = Cold, 3 = Fire, 4 = Holy, 5 = Lightning, 6 = Physical, 7 = Poison
			{	
                {	HeroClass.Necromancer,	new List<int>{2,6,7}		},
                {	HeroClass.Barbarian,	new List<int>{2,3,5,6}		},
                {	HeroClass.DemonHunter,	new List<int>{2,3,5,6}		},
                {	HeroClass.Wizard,		new List<int>{1,2,3,5}		},
                {	HeroClass.WitchDoctor,	new List<int>{2,3,6,7}		},
                {	HeroClass.Crusader,		new List<int>{3,4,5,6}		},
                {	HeroClass.Monk,			new List<int>{2,3,4,5,6}	},
            };				
		}

		public void Customize()
		{
			var OpacityBrush = (int) (Opacity * 255);	
			BrushRed = Hud.Render.CreateBrush(OpacityBrush, 255, 50 , 50, 0);
			
			FontWhite = Hud.Render.CreateFont("tahoma", 8f * SizeMultiplier, 255, 255, 255, 255, true, false, 255, 0, 0, 0, true);
			FontGray = Hud.Render.CreateFont("tahoma", 6f * SizeMultiplier, 235, 235, 235, 235, true, false, 255, 0, 0, 0, true);
			FontGreen = Hud.Render.CreateFont("tahoma", 10f * SizeMultiplier, 255, 0, 255, 0, true, false, 255, 0, 0, 0, true);			
			FontRed = Hud.Render.CreateFont("tahoma", 10f * SizeMultiplier, 255, 255, 0, 0, true, false, 255, 0, 0, 0, true);			
			FontYellow = Hud.Render.CreateFont("tahoma", 10f * SizeMultiplier, 255, 255, 255, 0, true, false, 255, 0, 0, 0, true);
			FontOrange = Hud.Render.CreateFont("tahoma", 10f * SizeMultiplier, 255, 255, 150, 0, true, false, 255, 0, 0, 0, true);
			FontBlue = Hud.Render.CreateFont("tahoma", 5f * SizeMultiplier, 255, 50, 150, 250, true, false, 255, 0, 0, 0, true);
			FontNames = Hud.Render.CreateFont("tahoma", 6f * SizeMultiplier, 235, 235, 235, 235, false, false, 255, 0, 0, 0, true);

			FontStacks = Hud.Render.CreateFont("tahoma", 8f * SizeMultiplier, 255, 255, 255, 0, true, false, 255, 0, 0, 0, true);
			FontStacksRed = Hud.Render.CreateFont("tahoma", 8f * SizeMultiplier, 255, 255, 0, 0, true, false, 160, 0, 0, 0, true);
			FontTimeLeft = Hud.Render.CreateFont("tahoma", 7f * SizeMultiplier, 255, 0, 255, 0, true, false, 255, 0, 0, 0, true);				
			FontTimeLeft2 = Hud.Render.CreateFont("tahoma", 5f * SizeMultiplier, 255, 0, 255, 0, true, false, 255, 0, 0, 0, true);							
			
			FontCounter = Hud.Render.CreateFont("tahoma", 6f * SizeMultiplier, 255, 0, 255, 0, true, false, 160, 0, 0, 0, true);
			FontLimit = Hud.Render.CreateFont("tahoma", 6f * SizeMultiplier, 255, 255, 220, 100, true, false, 160, 0, 0, 0, true);
			FontExpl = Hud.Render.CreateFont("tahoma", 7f * SizeMultiplier, 255, 50, 150, 255, true, false, 160, 0, 0, 0, true);	

			FontLocust = Hud.Render.CreateFont("tahoma", 6f * SizeMultiplier, 255, 255, 255, 255, false, false, 255, 0, 0, 0, true);			

			FontAS = Hud.Render.CreateFont("tahoma", 4f * SizeMultiplier, 255, 255, 255, 255, false, false, 255, 0, 0, 0, true);
			
			SizeIconWidth = Hud.Texture.BuffFrameTexture.Width  * 0.62f * SizeMultiplier;
			SizeIconHeight = Hud.Texture.BuffFrameTexture.Height * 0.62f * SizeMultiplier;
			
			if ((SBWarning < 0f) || (SBWarning > 9f)) { SBWarning = 2; }			
		}

		public void OnNewArea(bool newGame, ISnoArea area)
		{	
			if (newGame) Phantasms.Clear();
 		}
		
        private void DrawTimeLeftClock(RectangleF rect, double elapsed, double timeLeft)   // plugins\Default\BuffLists\Painter\BuffPainter.cs
        {
            if ((timeLeft > 0) && (elapsed >= 0) && (TimeLeftClockBrush != null))
            {
                var endAngle = Convert.ToInt32(360.0d / (timeLeft + elapsed) * elapsed);
                var startAngle = 0;
                TimeLeftClockBrush.Opacity = 1 - (float)(0.5f / (timeLeft + elapsed) * elapsed);
                var rad = rect.Width * 0.45f;
                using (var pg = Hud.Render.CreateGeometry())
                {
                    using (var gs = pg.Open())
                    {
                        gs.BeginFigure(rect.Center, FigureBegin.Filled);
                        for (var angle = startAngle; angle <= endAngle; angle++)
                        {
                            var mx = rad * (float)Math.Cos((angle - 90) * Math.PI / 180.0f);
                            var my = rad * (float)Math.Sin((angle - 90) * Math.PI / 180.0f);
                            var vec = new Vector2(rect.Center.X + mx, rect.Center.Y + my);
                            gs.AddLine(vec);
                        }

                        gs.EndFigure(FigureEnd.Closed);
                        gs.Close();
                    }

                    TimeLeftClockBrush.DrawGeometry(pg);
                }
            }
        }

		public void DrawIconBuff(float x, float y, float width, float height, uint idtexture, int stacks, double timeleft, double elapsed, double timeleft2 = 0)
		{
			Hud.Texture.GetTexture(idtexture).Draw(x, y, width, height, Opacity);
			Hud.Texture.BuffFrameTexture.Draw(x, y, width, height, Opacity);
			if (timeleft > 0)
			{
				DrawTimeLeftClock(new RectangleF(x, y , width, height), elapsed , timeleft);
				layout = FontTimeLeft.GetTextLayout(timeleft.ToString( (timeleft < 1)? "F1" : "F0") );
				FontTimeLeft.DrawText(layout, x + ((width - (float)Math.Ceiling(layout.Metrics.Width))/6.0f), y + ((height - (float)Math.Ceiling(layout.Metrics.Height))/14.0f) );
				if (timeleft2 > 0)	
				{
					layout = FontTimeLeft2.GetTextLayout(timeleft2.ToString((timeleft2 < 1)? "F1" : "F0") );
					FontTimeLeft2.DrawText(layout, x + ((width - (float)Math.Ceiling(layout.Metrics.Width))/5.0f), y + ((height - (float)Math.Ceiling(layout.Metrics.Height))/1.1f) );
				}	
			}
			layout = FontStacks.GetTextLayout( stacks.ToString() );
			(stacks == 0?FontStacksRed:FontStacks).DrawText(layout, x + ((width - (float)Math.Ceiling(layout.Metrics.Width))/1.15f), y + ((height - (float)Math.Ceiling(layout.Metrics.Height))/1.05f) );
		}
		
		public void DrawIconSkill(float x, float y, float width, float height, IPlayerSkill skill, int index = 1)
		{
			Hud.Texture.GetTexture(skill.SnoPower.NormalIconTextureId).Draw(x, y, width, height, Opacity);
			if (skill.Buff != null) // 465839
			{
				Hud.Texture.BuffFrameTexture.Draw(x, y, width, height, Opacity);
				
				double remaining = 0d;		double elapsed = 0d;
				if (skill.CooldownFinishTick > Hud.Game.CurrentGameTick) {
					remaining = (skill.CooldownFinishTick - Hud.Game.CurrentGameTick) / 60.0d;
					elapsed = (Hud.Game.CurrentGameTick - skill.CooldownStartTick) / 60.0d;
					DrawTimeLeftClock(new RectangleF(x, y , width, height), elapsed , remaining);
					layout = FontWhite.GetTextLayout( remaining.ToString( (remaining < 1)? "F1":"F0" ) );
				}
				else { layout = FontWhite.GetTextLayout(" 🞴 "); }
				FontWhite.DrawText(layout, x + ((width - (float)Math.Ceiling(layout.Metrics.Width))/1.40f), y + ((height - (float)Math.Ceiling(layout.Metrics.Height))/1.1f) );
				
				double timeleft = skill.Buff.TimeLeftSeconds[index];		
				if (timeleft > 0)
				{
					layout = FontTimeLeft.GetTextLayout(timeleft.ToString( (timeleft < 1)? "F1" : "F0") );
					FontTimeLeft.DrawText(layout, x + ((width - (float)Math.Ceiling(layout.Metrics.Width))/6.0f), y + ((height - (float)Math.Ceiling(layout.Metrics.Height))/14.0f) );	
				}
			}
			else { Hud.Texture.DebuffFrameTexture.Draw(x, y, width, height, Opacity); }
		}
		
		public void PaintTopInGame(ClipState clipState)
		{
			if (clipState != ClipState.BeforeClip) return;
			if (!Hud.Game.IsInGame) return;
			if (OnlyGR && !Hud.Game.Me.InGreaterRift) return;
	
			var players = Hud.Game.Players.Where( p => p.IsMe?ShowMe:(ShowOthers && p.HasValidActor) ).OrderBy(p => p.PortraitIndex);
			if (players.Count() == 0) return;

			var y =  Hud.Window.Size.Height * Ypor ;
			foreach(var player in players)
			{
				var x =  Hud.Window.Size.Width * Xpor;
				if (player.HeroClassDefinition.HeroClass == HeroClass.Necromancer)
				{				
					IPlayerSkill skill = player.Powers.UsedNecromancerPowers.SkeletalMage;  // 462089
					if ((skill != null) && (skill?.Rune == 1))
					{
						if (!Singularity) continue;
						
						{  // Simple bloque						
							if (ShowNames) { layout = FontNames.GetTextLayout(player.BattleTagAbovePortrait); FontNames.DrawText(layout,x + 1,y ); y += layout.Metrics.Height + 1; }
							if (skill.Buff != null)
							{
								var stacks = ( (skill.Buff.IconCounts[6] == 1) && (skill.Buff.TimeLeftSeconds[5] == 0) )? 0:skill.Buff.IconCounts[6];
								if (stacks == 0) 
								{
									DrawIconBuff(x,y,SizeIconWidth,SizeIconHeight,skill.SnoPower.NormalIconTextureId,0, 0, 0 );
								}
								else 
								{
									DrawIconBuff(x,y,SizeIconWidth,SizeIconHeight,skill.SnoPower.NormalIconTextureId,stacks,skill.Buff.TimeLeftSeconds[5],skill.Buff.TimeElapsedSeconds[5],skill.Buff.TimeLeftSeconds[6]);
								}
							}
							else 
							{
								Hud.Texture.GetTexture(skill.SnoPower.NormalIconTextureId).Draw(x, y, SizeIconWidth,SizeIconHeight, Opacity);
								Hud.Texture.DebuffFrameTexture.Draw(x, y, SizeIconWidth,SizeIconHeight, Opacity);						
							}
							x += SizeIconWidth;	
						}						
										
						skill = player.Powers.UsedNecromancerPowers.BoneArmor;  // 466857
						if (skill != null)  
						{			
							if (skill.Buff != null)
							{
								DrawIconBuff(x,y,SizeIconWidth,SizeIconHeight,skill.SnoPower.NormalIconTextureId,skill.Buff.IconCounts[0],skill.Buff.TimeLeftSeconds[0], skill.Buff.TimeElapsedSeconds[0] );
							}
							else 
							{
								Hud.Texture.GetTexture(skill.SnoPower.NormalIconTextureId).Draw(x, y, SizeIconWidth,SizeIconHeight, Opacity);
								Hud.Texture.DebuffFrameTexture.Draw(x, y, SizeIconWidth,SizeIconHeight, Opacity);						
							}
							x += SizeIconWidth;
						}
						
						skill = player.Powers.UsedNecromancerPowers.Simulacrum; // 465350
						if (skill != null)
						{					
							DrawIconSkill(x,y,SizeIconWidth,SizeIconHeight,skill, 1);	// index 1 , para averiguar el timeleft					
							x += SizeIconWidth;
						}
						
						skill = player.Powers.UsedNecromancerPowers.LandOfTheDead; // 465839
						if (skill != null)
						{
							DrawIconSkill(x,y,SizeIconWidth,SizeIconHeight,skill, 0);	// index 0
							// x += SizeIconWidth;  // Si seguimos añadiendo más..
						}
					}
					else if (player.Powers.BuffIsActive(476587))
					{
						if (!Nayr) continue;
						
						{	// Simple bloque
							if (ShowNames) { layout = FontNames.GetTextLayout(player.BattleTagAbovePortrait); FontNames.DrawText(layout,x + 1,y ); y += layout.Metrics.Height + 1; }
							var c = player.Powers.GetBuff(476587).IconCounts[7];
							if (c > 0) 
							{
								int j = 7; double timeleft = 0; double timeleft2 = 15;
								for (var i = 1; i < 7; i++)
								{
									var t = player.Powers.GetBuff(476587).TimeLeftSeconds[i];
									if (t > 0) 
									{
										if (t > timeleft) { j = i; timeleft = t; }
										if (t < timeleft2) { timeleft2 = t; }
									}
								}
								DrawIconBuff(x,y,SizeIconWidth,SizeIconHeight,2831437924, c, player.Powers.GetBuff(476587).TimeLeftSeconds[j], player.Powers.GetBuff(476587).TimeElapsedSeconds[j],(c > 1)?timeleft2:0 );
							}
							else { DrawIconBuff(x,y,SizeIconWidth,SizeIconHeight,2831437924, 0, 0, 0); }
							x += SizeIconWidth;	
						}						

						skill = player.Powers.UsedNecromancerPowers.BoneArmor;  // 466857
						if (skill != null)  // 466857
						{			
							if (skill.Buff != null)
							{
								DrawIconBuff(x,y,SizeIconWidth,SizeIconHeight,skill.SnoPower.NormalIconTextureId,skill.Buff.IconCounts[0],skill.Buff.TimeLeftSeconds[0], skill.Buff.TimeElapsedSeconds[0] );
							}
							else 
							{
								Hud.Texture.GetTexture(skill.SnoPower.NormalIconTextureId).Draw(x, y, SizeIconWidth,SizeIconHeight, Opacity);
								Hud.Texture.DebuffFrameTexture.Draw(x, y, SizeIconWidth,SizeIconHeight, Opacity);						
							}
							x += SizeIconWidth;
						}
						
						if (player.Powers.BuffIsActive(475251))
						{		
							Hud.Texture.InventoryLegendaryBackgroundLarge.Draw(x,y,SizeIconWidth,SizeIconHeight,Opacity);
							Hud.Texture.GetItemTexture(Hud.Sno.SnoItems.P65_Unique_Scythe2H_02).Draw(x,y,SizeIconWidth,SizeIconHeight,Opacity);
							if (Math.Abs(Hud.Game.CurrentGameTick - Ticks0) > 20)
							{
								Count0 = Hud.Game.AliveMonsters.Where(m => m.FloorCoordinate.XYDistanceTo(player.FloorCoordinate) <= 25).Count();
								Ticks0 = Hud.Game.CurrentGameTick;
							}
							layout = FontTimeLeft.GetTextLayout( Count0.ToString() );
							FontStacks.DrawText(layout, x + ((SizeIconWidth - (float)Math.Ceiling(layout.Metrics.Width))/2.0f), y + ((SizeIconHeight - (float)Math.Ceiling(layout.Metrics.Height))/2.0f) );						
							x += SizeIconWidth;
						}							
					}
					else continue;
				}
				else if (player.HeroClassDefinition.HeroClass == HeroClass.WitchDoctor)
				{
					IPlayerSkill skill = player.Powers.UsedWitchDoctorPowers.SpiritBarrage; // 108506
					if ( (skill != null) && (skill.Rune == 2 || player.Powers.BuffIsActive(484270)))
					{
						if (!SpiritBarrage) continue;
						
						{	// Simple bloque
							if (ShowNames) { layout = FontNames.GetTextLayout(player.BattleTagAbovePortrait); FontNames.DrawText(layout,x + 1,y ); y += layout.Metrics.Height + 1; }
							var actors = Hud.Game.Actors.Where(a => a.SnoActor.Sno == ActorSnoEnum._wd_spiritbarragerune_aoe_ghostmodel && (a.SummonerAcdDynamicId == player.SummonerId));
							Hud.Texture.GetTexture(skill.SnoPower.NormalIconTextureId).Draw(x, y, SizeIconWidth , SizeIconHeight, Opacity); // TextureId 1117784160
							int total = actors.Count();
							if (total > 0)
							{
								foreach(var a in actors) 
								{ 
									if (!Phantasms.ContainsKey(a.AnnId)) { Phantasms[a.AnnId] = a.CreatedAtInGameTick; }
								}
								actors = actors.OrderByDescending(a => Phantasms[a.AnnId]);							
								Hud.Texture.BuffFrameTexture.Draw(x, y, SizeIconWidth, SizeIconHeight, Opacity);
								var c = 0;							
								foreach (var actor in actors)
								{
									if (++c > 3) break;
									var t = (player.Powers.BuffIsActive(484270)?10:5) - (Hud.Game.CurrentGameTick - Phantasms[actor.AnnId]) /  60f;
									if (t <= 0)
									{
										var layout = FontExpl.GetTextLayout("🞴");							
										FontExpl.DrawText(layout, x + ((SizeIconWidth - (float)Math.Ceiling(layout.Metrics.Width))/8.0f), y + (layout.Metrics.Height * 0.56f * (c - 1)) );							
									}
									else
									{
										FontDefault = (t > SBWarning)? FontCounter:FontLimit;
										var layout = FontDefault.GetTextLayout( String.Format("{0:0}",(int) (t + 0.90)) ); // Redondeará a X si es menor  a X.10
										FontDefault.DrawText(layout, x + ((SizeIconWidth - (float)Math.Ceiling(layout.Metrics.Width))/7.0f), y + (layout.Metrics.Height * 0.85f * (c - 1)) );																	
									}
								}							
							}				
							else {	Hud.Texture.BuffFrameTexture.Draw(x, y, SizeIconWidth, SizeIconHeight, Opacity);	}
							layout = FontStacks.GetTextLayout(total.ToString());
							(total == 0?FontStacksRed:FontStacks).DrawText(layout, x + ((SizeIconWidth - (float)Math.Ceiling(layout.Metrics.Width))/1.15f), y + ((SizeIconHeight - (float)Math.Ceiling(layout.Metrics.Height))/1.05f) );
							layout = FontAS.GetTextLayout(player.Offense.AttackSpeed.ToString("F2", CultureInfo.InvariantCulture));
							FontAS.DrawText(layout,x + ((SizeIconWidth - (float)Math.Ceiling(layout.Metrics.Width))/1.15f), y + ((SizeIconHeight - (float)Math.Ceiling(layout.Metrics.Height))/7.0f) );
							x +=  SizeIconWidth;
						}
						
						skill = player.Powers.UsedWitchDoctorPowers.SoulHarvest; 
						if (skill != null)  
						{			
							if (skill.Buff != null) // 67616
							{      
								DrawIconBuff(x,y,SizeIconWidth,SizeIconHeight,skill.SnoPower.NormalIconTextureId,skill.Buff.IconCounts[0],skill.Buff.TimeLeftSeconds[0],skill.Buff.TimeElapsedSeconds[0] );
							}
							else 
							{
								Hud.Texture.GetTexture(skill.SnoPower.NormalIconTextureId).Draw(x, y, SizeIconWidth, SizeIconHeight, Opacity);	// skill.SnoPower.NormalIconTextureId = 2196086897
								Hud.Texture.DebuffFrameTexture.Draw(x, y, SizeIconWidth, SizeIconHeight, Opacity);								
							}
							x += SizeIconWidth;
						}
						if (ShowLocust) 
						{
							skill = player.Powers.UsedWitchDoctorPowers.LocustSwarm; 
							if (skill != null)  
							{
								Hud.Texture.GetTexture(skill.SnoPower.NormalIconTextureId).Draw(x, y, SizeIconWidth,SizeIconHeight, Opacity);
								Hud.Texture.BuffFrameTexture.Draw(x, y, SizeIconWidth,SizeIconHeight, Opacity);
								var m = Hud.Game.AliveMonsters.Where(a => a.FloorCoordinate.XYDistanceTo(player.FloorCoordinate) <= 40);
								var mLc = m.Where(a => a.Locust).Count();
								if (mLc > 0)
								{
									var mc = m.Count();
									var e = m.Where(a => (a.Rarity == ActorRarity.Champion || a.Rarity == ActorRarity.Rare || a.Rarity == ActorRarity.Boss) && (a.SummonerAcdDynamicId == 0) );
									var ec = e.Count();
									var eLc = e.Where(a => a.Locust).Count();
									if (mc > ec)
									{
										layout = FontLocust.GetTextLayout( mLc.ToString() + "|" + mc.ToString());
										FontLocust.DrawText(layout, x + ((SizeIconWidth - (float)Math.Ceiling(layout.Metrics.Width))/2.0f), y + ((SizeIconHeight - (float)Math.Ceiling(layout.Metrics.Height))/6.0f) );
									}									
									if (ec > 0)
									{
										layout = FontLocust.GetTextLayout( eLc.ToString() + "|" + ec.ToString());
										FontLocust.DrawText(layout, x + ((SizeIconWidth - (float)Math.Ceiling(layout.Metrics.Width))/2.0f), y + ((SizeIconHeight - (float)Math.Ceiling(layout.Metrics.Height))/1.20f) );
									}
								}
								x += SizeIconWidth;							
							}
						}						
						if (ShowBigBadVoodoo) 
						{
							skill = player.Powers.UsedWitchDoctorPowers.BigBadVoodoo;	
							if (skill != null)
							{					
								DrawIconSkill(x,y,SizeIconWidth,SizeIconHeight,skill, 4);	// index 4 , para averiguar el timeleft					
								x += SizeIconWidth;
							}
						}
						
						if (player.Powers.UsedPassives.Any(p => p.Sno == Hud.Sno.SnoPowers.WitchDoctor_Passive_GruesomeFeast.Sno)) // 208594
						{
							IBuff buff = player.Powers.GetBuff(208594);
							if (buff != null)  // Hud.Sno.SnoPowers.WitchDoctor_Passive_GruesomeFeast.NormalIconTextureId  = 1591242582
							{				
								DrawIconBuff(x,y,SizeIconWidth,SizeIconHeight,1591242582,buff.IconCounts[1],buff.TimeLeftSeconds[1],buff.TimeElapsedSeconds[1] );	
							}
							else 
							{
								Hud.Texture.GetTexture(1591242582).Draw(x, y, SizeIconWidth, SizeIconHeight, Opacity);
								Hud.Texture.DebuffFrameTexture.Draw(x, y, SizeIconWidth, SizeIconHeight, Opacity);									
							}
							x +=  SizeIconWidth;		
							if (ShowGlobes)
							{
								var n = Hud.Game.Actors.Where(a => a.SnoActor.Kind == ActorKind.HealthGlobe).Count();
								if (n > 0)
								{
									layout = FontWhite.GetTextLayout(n.ToString());
									var w = (n < 10)? layout.Metrics.Width + 1:(layout.Metrics.Width/2 + 2);
									BrushRed.DrawEllipse(x + SizeIconWidth / 2 , y + SizeIconHeight / 2 , w , w );
									FontWhite.DrawText(layout,x + (SizeIconWidth - layout.Metrics.Width) / 2 , y + (SizeIconHeight - layout.Metrics.Height) / 2 - 1 );
								}
							}
						}
					}
					else continue;					
				}
				else continue;
				
				x =  Hud.Window.Size.Width * Xpor;	y +=  SizeIconHeight;			
				if (ShowCoe && (player.Powers.GetBuff(Hud.Sno.SnoPowers.ConventionOfElements.Sno) != null) ) // 430674
				{				
					var BuffCoE = player.Powers.GetBuff(430674);
					var HeroBonusElements = AllBonusElements[player.HeroClassDefinition.HeroClass];
					var j =	HeroBonusElements.Count;	var ActiveElement = -1; var BestElement = -1;  double MaxBonus = -1; 	bool UniqueBestElement = false;
					for(var k = j - 1; k > -1 ; k--)
					{
						var index = HeroBonusElements[k];
						if (BuffCoE.IconCounts[index] > 0)	{	ActiveElement = index;  }
						
						var bonus = player.Offense.ElementalDamageBonus[IndexToBonus[index]];
						if (bonus > MaxBonus) 
						{
							MaxBonus = bonus; 
							BestElement = index;
							UniqueBestElement = true;
						}	
						else if (bonus == MaxBonus) UniqueBestElement = false;				
					}				
					if ((ActiveElement != -1) && (BestElement != -1) )
					{						
						for (var l = 0; l < j; l++)
						{
							if (HeroBonusElements[j - 1] != BestElement)
							{
								HeroBonusElements.Insert(0,HeroBonusElements[j - 1]);
								HeroBonusElements.RemoveAt(j);
							}
							else  {   break;  }
						} 
						y += ProgressBarWidth;
						foreach(var index in HeroBonusElements) 
						{				
							Hud.Texture.GetTexture(BuffCoE.SnoPower.Icons[index].TextureId).Draw(x, y, SizeIconWidth, SizeIconHeight, Opacity);
							DrawTimeLeftClock(new RectangleF(x, y , SizeIconWidth, SizeIconHeight), BuffCoE.TimeElapsedSeconds[index], BuffCoE.TimeLeftSeconds[index]);				
							
							double s = 0; 						
							if (UniqueBestElement)
							{
								if (index == ActiveElement)
								{
									s = BuffCoE.TimeLeftSeconds[index];
									if (index == BestElement) { FontDefault = FontGreen; BrushDefault = BrushGreen; }
									else { FontDefault = FontGray; }
									Hud.Texture.BuffFrameTexture.Draw(x, y, SizeIconWidth, SizeIconHeight, Opacity);
								}
								else
								{			
									if (index == BestElement)
									{
										s = (j - HeroBonusElements.IndexOf(ActiveElement) - 1) * 4 - BuffCoE.TimeElapsedSeconds[ActiveElement] ;			
										if (  s >  ((j - 1) * 2) ) { FontDefault = FontOrange;	BrushDefault = BrushOrange; }
										else { FontDefault = FontYellow; BrushDefault = BrushYellow; }
									}
									(InactiveRedFrame?Hud.Texture.DebuffFrameTexture:Hud.Texture.BuffFrameTexture).Draw(x, y, SizeIconWidth, SizeIconHeight, Opacity);
								}
							}
							else
							{
								if (player.Offense.ElementalDamageBonus[IndexToBonus[index]] == player.Offense.HighestElementalDamageBonus) 
								{
									layout = FontBlue.GetTextLayout( (player.Offense.HighestElementalDamageBonus * 100).ToString() );
									FontBlue.DrawText(layout, x + (SizeIconWidth - (float)Math.Ceiling(layout.Metrics.Width))/1.15f , y + (SizeIconHeight - (float)Math.Ceiling(layout.Metrics.Height))/1.07f );
								}
								s = BuffCoE.TimeLeftSeconds[index];
								BrushDefault = BrushBlue;	FontDefault = FontWhite;
								Hud.Texture.BuffFrameTexture.Draw(x, y, SizeIconWidth, SizeIconHeight, Opacity);
							}

							if (s > 0) 
							{
								//layout = FontDefault.GetTextLayout(s.ToString( (s < 1)? "F1" : "F0") );
								layout = FontDefault.GetTextLayout( (s < 1)? s.ToString("F1") : String.Format("{0:0}",(int) (s + 0.80)) ); // Redondeará a X si es menor  a X.20
								FontDefault.DrawText(layout, x + ((SizeIconWidth - (float)Math.Ceiling(layout.Metrics.Width))/2.0f), y + ((SizeIconHeight - (float)Math.Ceiling(layout.Metrics.Height))/2.0f));
							}	
							x += SizeIconWidth;						
						}
						BrushBlack.DrawRectangle(x , y, - SizeIconWidth * j , - ProgressBarWidth);
						var t = (float) ( SizeIconWidth * (j - HeroBonusElements.IndexOf(ActiveElement) - BuffCoE.TimeElapsedSeconds[ActiveElement] / 4) );
						if (t > 0) BrushDefault.DrawRectangle(x, y, - t , - ProgressBarWidth);
						y =  y + SizeIconHeight;
					}					
				}
				y += SizeIconHeight * (ShowNames?0.1f:0.4f);
			}
		}
	}
}