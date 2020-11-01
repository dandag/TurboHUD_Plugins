using Turbo.Plugins.Default;
using System;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Collections.Generic;

namespace Turbo.Plugins.DAV
{
	public class DAV_NecroPetPlugin : BasePlugin, IInGameWorldPainter, IInGameTopPainter, INewAreaHandler, IItemLocationChangedHandler {
		public float barW, barH, barX, barY;
		
		// Command Skeleton ~~~~~~~~~~
		private static uint SkeletonSkillSNO = 453801;
		public HashSet<ActorSnoEnum> SkeletonActorSNOs = new HashSet<ActorSnoEnum> {
			ActorSnoEnum._p6_necro_commandskeletons_a, // Skeleton - No Rune
			ActorSnoEnum._p6_necro_commandskeletons_b, // Skeleton - Dark Mending
			ActorSnoEnum._p6_necro_commandskeletons_c, // Skeleton - Frenzy
			ActorSnoEnum._p6_necro_commandskeletons_d, // Skeleton - Kill Command
			ActorSnoEnum._p6_necro_commandskeletons_e, // Skeleton - Enforcer
			ActorSnoEnum._p6_necro_commandskeletons_f  // Skeleton - Freezing Grasp
		};
		public WorldDecoratorCollection SkeletonDecorator { get; set; }
		public WorldDecoratorCollection SkeletonActiveDecorator { get; set; }
		public TopLabelDecorator SkeletonCountLabel { get; set; }	
		public int SkeletonCount { get; set; } = 0;
		public string Label_Skeleton { get; set; } = "";
		public bool ShowSkeleton { get; set; } = false;
		
		// Command Skeleton Target ~~~~~~~~~~
		public WorldDecoratorCollection	SkeletonTargetDecorator { get; set; }
		public WorldDecoratorCollection	SkeletonTargetEliteDecorator { get; set; }
		public bool TargetOnEliteOnly { get; set; } = false;
		
		// Command Skeleton of other players ~~~~~~~~~~
		public WorldDecoratorCollection SkeletonOtherDecorator { get; set; }
		public string Label_SkeletonOthers { get; set; } = "";
		public bool ShowSkeletonOthers { get; set; } = false;
		
		// Skeleton Mage ~~~~~~~~~~
		private static uint MageSkillSNO = 462089;
		public HashSet<ActorSnoEnum> MageActorSNOs = new HashSet<ActorSnoEnum> {
			ActorSnoEnum._p6_necro_skeletonmage_a, // Skeleton Mage - No Rune
			ActorSnoEnum._p6_necro_skeletonmage_b, // Skeleton Mage - Gift of Death
			ActorSnoEnum._p6_necro_skeletonmage_c, // Skeleton Mage - Singularity
			ActorSnoEnum._p6_necro_skeletonmage_d, // Skeleton Mage - Life Support
			ActorSnoEnum._p6_necro_skeletonmage_e, // Skeleton Mage - Contamination
			ActorSnoEnum._p6_necro_skeletonmage_f_archer // Skeleton Mage - Archer
		};
		public WorldDecoratorCollection MageDecorator { get; set; }
		public TopLabelDecorator MageCountLabel { get; set; }
		public int MageCount { get; set; } = 0;
		public string Label_Mage { get; set; } = "";
		public bool ShowMage { get; set; } = false;
		
		// Golem ~~~~~~~~~~
		private static uint GolemSkillSNO = 451537;
		public HashSet<ActorSnoEnum> GolemActorSNOs = new HashSet<ActorSnoEnum> {
			ActorSnoEnum._p6_decaygolem, // No Rune & Flesh Golem Rune
			ActorSnoEnum._p6_bonegolem, // Bone Golem Rune
			ActorSnoEnum._p6_consumefleshgolem, // Decay Golem Rune
			ActorSnoEnum._p6_icegolem, // Ice Golem Rune
			ActorSnoEnum._p6_bloodgolem // Blood Golem Rune
		};
		public WorldDecoratorCollection GolemDecorator { get; set; }
		public string Label_Golem { get; set; } = "";
		public bool ShowGolem { get; set; } = true;
		
		// Simulacrum ~~~~~~~~~~
		private static uint SimulacrumSkillSNO = 465350;
		public HashSet<ActorSnoEnum> SimulacrumActorSNOs = new HashSet<ActorSnoEnum> {
			ActorSnoEnum._p6_necro_simulacrum_a_set, // New Haunted Visions 2.6.9
			ActorSnoEnum._p6_necro_simulacrum_a, // Old Haunted Visions
		};
		public WorldDecoratorCollection SimulacrumDecorator { get; set; }
		public string Label_Simulacrum { get; set; } = "My Sim";
		public bool ShowSimulacrum { get; set; } = true;

		// Simulacrum Clones of other players ~~~~~~~~~~
		public WorldDecoratorCollection SimulacrumOtherDecorator { get; set; }
		public string Label_SimulacrumOthers { get; set; } = "Sim";
		public bool ShowSimulacrumOthers { get; set; } = true;
		
		
		// Revive ~~~~~~~~~~
		private static uint ReviveSkillSNO = 462239;
		public WorldDecoratorCollection ReviveDecorator { get; set; }
		public TopLabelDecorator ReviveCountLabel { get; set; }
		public int ReviveCount { get; set; } = 0;
		public string Label_Revive { get; set; } = "";
		public bool ShowRevive { get; set; } = false;

		// Ringer timer ~~~~~~~~~~
		public IMonster ActiveTarget { get; set; }
		public IFont ActiveFont { get; set; }
		public IWatch ActiveTime { get; set; }
		public IWatch ActiveTimeLong { get; set; }
		public ITexture RingerTexture { get; set; }
		private bool SkillPress { get; set; } = false;
		public float Xpos { get; set; }
		public float Ypos { get; set; }
		public float texsize { get; set; }
		public int legpower { get; set; } = 30;
		public bool GRonly { get; set; } = false;
		public bool Bossonly { get; set; } = false;
		public bool showTimerShort { get; set; } = true;
		public bool showTimerLong { get; set; } = true;
		
		public DAV_NecroPetPlugin() {
			Enabled = true;
		}

		public override void Load(IController hud) {
			base.Load(hud);
			Order = 30950;
			// Display coordinated for indicators
			barW = Hud.Window.Size.Width * 0.012f;
			barH = Hud.Window.Size.Height * 0.0175f;
			barX = Hud.Window.Size.Width * 0.45f;
			barY = Hud.Window.Size.Height * 0.62f;
			
			// Display Ringer Timer
			Xpos = Hud.Window.Size.Width * 0.7f;
			Ypos = Hud.Window.Size.Height * 0.7f;
			texsize = Hud.Window.Size.Width * 0.025f;
			ActiveFont = Hud.Render.CreateFont("tahoma", 10, 255, 51, 51, 51, false, false, 250, 255, 255, 255, true);
			ActiveTarget = null;
			ActiveTime = Hud.Time.CreateWatch();
			ActiveTimeLong = Hud.Time.CreateWatch();
			RingerTexture = Hud.Texture.GetItemTexture(Hud.Sno.SnoItems.P6_Unique_Phylactery_02);
			
			// Decorator Setting
			var petsize = -1;

			SkeletonDecorator = new WorldDecoratorCollection(
				new GroundCircleDecorator(Hud) {
					Brush = Hud.Render.CreateBrush(240, 51, 255, 51, 3),
					Radius = petsize,
				},
				new GroundLabelDecorator(Hud) {
					BackgroundBrush = Hud.Render.CreateBrush(240, 51, 255, 51, 0),
					TextFont = Hud.Render.CreateFont("tahoma", 7, 255, 255, 255, 255, true, false, false)
				}
			);
			
			SkeletonOtherDecorator = new WorldDecoratorCollection(
				new GroundCircleDecorator(Hud) {
					Brush = Hud.Render.CreateBrush(240, 51, 255, 51, 3, SharpDX.Direct2D1.DashStyle.Dash),
					Radius = petsize,
				},
				new GroundLabelDecorator(Hud) {
					BackgroundBrush = Hud.Render.CreateBrush(240, 51, 255, 51, 0),
					TextFont = Hud.Render.CreateFont("tahoma", 7, 255, 255, 255, 255, true, false, false)
				}
			);
			
			SkeletonActiveDecorator = new WorldDecoratorCollection(
				new GroundCircleDecorator(Hud) {
					Brush = Hud.Render.CreateBrush(204, 255, 255, 153, 0),
					Radius = 0.8f,
				}
			);

			SkeletonTargetEliteDecorator = new WorldDecoratorCollection(
				new GroundShapeDecorator(Hud) {
					Brush = Hud.Render.CreateBrush(204, 255, 102, 102, 3f),
					ShapePainter = WorldStarShapePainter.NewDoubleSquare(Hud),
					RotationTransformator = new CircularRotationTransformator(Hud, 30),
					Radius = 5f
				},
				new MapShapeDecorator(Hud) {
					Brush = Hud.Render.CreateBrush(255, 255, 255, 255, -1),
					ShapePainter = new LineFromMeShapePainter(Hud)
				}
			);
			
			SkeletonTargetDecorator = new WorldDecoratorCollection(
				new GroundCircleDecorator(Hud) {
					Brush = Hud.Render.CreateBrush(180, 255, 0, 0, 6),
					Radius = 0.3f	
				},
				new GroundShapeDecorator(Hud) {
					Brush = Hud.Render.CreateBrush(204, 255, 102, 102, 3f),
					ShapePainter = WorldStarShapePainter.NewOctagon(Hud),
					RotationTransformator = new CircularRotationTransformator(Hud, 30),
					Radius = 5f
				}
			);

			MageDecorator = new WorldDecoratorCollection(
				new GroundCircleDecorator(Hud) {
					Brush = Hud.Render.CreateBrush(240, 255, 153, 51, 3),
					Radius = petsize,
				},
				new GroundLabelDecorator(Hud) {
					BackgroundBrush = Hud.Render.CreateBrush(240, 255, 153, 51, 0),
					TextFont = Hud.Render.CreateFont("tahoma", 7, 255, 255, 255, 255, true, false, false)
				}
			);

			GolemDecorator = new WorldDecoratorCollection(
				new GroundCircleDecorator(Hud) {
					Brush = Hud.Render.CreateBrush(240, 51, 153, 255, 3),
					Radius = 2f,
				},
				new GroundLabelDecorator(Hud) {
					BackgroundBrush = Hud.Render.CreateBrush(240, 51, 153, 255, 0),
					TextFont = Hud.Render.CreateFont("tahoma", 7, 255, 255, 255, 255, true, false, false)
				}
			);

			SimulacrumDecorator = new WorldDecoratorCollection(
				new GroundCircleDecorator(Hud) {
					Brush = Hud.Render.CreateBrush(255, 255, 255, 255, 3f),
					Radius = 0.3f,
				},
				new GroundCircleDecorator(Hud) {
					Brush = Hud.Render.CreateBrush(255, 0, 255, 255, 4f),
					Radius = 1f,
				},
				new GroundCircleDecorator(Hud) {
					Brush = Hud.Render.CreateBrush(255, 255, 255, 255, 4f),
					Radius = 1.6f,
				},
				new GroundLabelDecorator(Hud) {
					BackgroundBrush = Hud.Render.CreateBrush(240, 51, 153, 255, 0),
					TextFont = Hud.Render.CreateFont("tahoma", 7, 255, 255, 255, 255, true, false, false)
				}
			);

			SimulacrumOtherDecorator = new WorldDecoratorCollection(
				new GroundCircleDecorator(Hud) {
					Brush = Hud.Render.CreateBrush(255, 255, 255, 255, 3f),
					Radius = 0.3f,
				},
				new GroundCircleDecorator(Hud) {
					Brush = Hud.Render.CreateBrush(255, 0, 255, 255, 4f),
					Radius = 1f,
				},
				new GroundCircleDecorator(Hud) {
					Brush = Hud.Render.CreateBrush(255, 255, 255, 255, 4f),
					Radius = 1.6f,
				},
				new GroundLabelDecorator(Hud) {
					BackgroundBrush = Hud.Render.CreateBrush(240, 255, 51, 51, 0),
					TextFont = Hud.Render.CreateFont("tahoma", 7, 255, 255, 255, 255, true, false, false)
				}
			);

			ReviveDecorator = new WorldDecoratorCollection(
				new GroundCircleDecorator(Hud) {
					Brush = Hud.Render.CreateBrush(240, 153, 51, 255, 3),
					Radius = -1,
				},
				new GroundLabelDecorator(Hud) {
					BackgroundBrush = Hud.Render.CreateBrush(240, 153, 51, 255, 0),
					TextFont = Hud.Render.CreateFont("tahoma", 7, 255, 255, 255, 255, true, false, false)
				}
			);

			// Pet Number Count Label
			SkeletonCountLabel = new TopLabelDecorator(Hud) {
				TextFont = Hud.Render.CreateFont("tahoma", 7, 255, 255, 240, 0, false, false, true),
				BackgroundTexture1 = Hud.Texture.ButtonTextureGray,
				BackgroundTexture2 = Hud.Texture.BackgroundTextureGreen,
				BackgroundTextureOpacity2 = 0.5f,
				TextFunc = () => SkeletonCount.ToString()
			};

			MageCountLabel = new TopLabelDecorator(Hud) {
				TextFont = Hud.Render.CreateFont("tahoma", 7, 255, 255, 240, 0, false, false, true),
				BackgroundTexture1 = Hud.Texture.ButtonTextureOrange,
				BackgroundTexture2 = Hud.Texture.BackgroundTextureGreen,
				BackgroundTextureOpacity2 = 0.5f,
				TextFunc = () => MageCount.ToString()
			};
			
			ReviveCountLabel = new TopLabelDecorator(Hud) {
				TextFont = Hud.Render.CreateFont("tahoma", 7, 255, 255, 240, 0, false, false, true),
				BackgroundTexture1 = Hud.Texture.ButtonTextureOrange,
				BackgroundTexture2 = Hud.Texture.BackgroundTextureGreen,
				BackgroundTextureOpacity2 = 0.5f,
				TextFunc = () => ReviveCount.ToString()
			};
		}

		public void PaintWorld(WorldLayer layer) {
			if (ShowSkeletonOthers) {
				var players = Hud.Game.Players.Where(player => !player.IsMe && player.HeroClassDefinition.HeroClass == HeroClass.Necromancer && player.CoordinateKnown && Hud.Game.Me.SnoArea.Sno == player.SnoArea.Sno && (player.HeadStone == null));
				var OtherActors = Hud.Game.Actors.Where(EachActor => EachActor.SummonerAcdDynamicId != Hud.Game.Me.SummonerId && SkeletonActorSNOs.Contains(EachActor.SnoActor.Sno));
				
				foreach(var player in players) {
					var IsActive = player.Powers.BuffIsActive(SkeletonSkillSNO, 0);
					foreach (var OtherActor in OtherActors) {
						if (OtherActor.SummonerAcdDynamicId == player.SummonerId) {
							SkeletonOtherDecorator.Paint(layer, OtherActor, OtherActor.FloorCoordinate, Label_SkeletonOthers);
							if (IsActive)
								SkeletonActiveDecorator.Paint(layer, OtherActor, OtherActor.FloorCoordinate, Label_SkeletonOthers);
						}
					}
				}
			}
			if (ShowSimulacrumOthers) {
				var players = Hud.Game.Players.Where(player => !player.IsMe && player.HeroClassDefinition.HeroClass == HeroClass.Necromancer && player.CoordinateKnown && Hud.Game.Me.SnoArea.Sno == player.SnoArea.Sno && (player.HeadStone == null));
				var OtherActors = Hud.Game.Actors.Where(EachActor => EachActor.SummonerAcdDynamicId != Hud.Game.Me.SummonerId && SimulacrumActorSNOs.Contains(EachActor.SnoActor.Sno));
				
				foreach(var player in players) {
					var IsActive = player.Powers.BuffIsActive(SimulacrumSkillSNO, 0);
					foreach (var OtherActor in OtherActors) {
						if (OtherActor.SummonerAcdDynamicId == player.SummonerId) {
							SimulacrumOtherDecorator.Paint(layer, OtherActor, OtherActor.FloorCoordinate, Label_SimulacrumOthers);
						}
					}
				}
			}
			
			if (Hud.Game.Me.HeroClassDefinition.HeroClass != HeroClass.Necromancer) return;

			// For Skeleton Melee, only when equipping the skill
			var skill = Hud.Game.Me.Powers.UsedSkills.FirstOrDefault(s => s.SnoPower.Sno == Hud.Sno.SnoPowers.Necromancer_CommandSkeletons.Sno);
			if (skill != null) {
				var activeSkeleton = Hud.Game.Me.Powers.BuffIsActive(SkeletonSkillSNO, 0);
				var SkeletonActors = Hud.Game.Actors.Where(EachActor => EachActor.SummonerAcdDynamicId == Hud.Game.Me.SummonerId && SkeletonActorSNOs.Contains(EachActor.SnoActor.Sno));
					
				if (ShowSkeleton) {
					SkeletonCount = SkeletonActors.Count();
					foreach (var EachActor in SkeletonActors) {
						SkeletonDecorator.Paint(layer, EachActor, EachActor.FloorCoordinate, null);
						if (activeSkeleton)
							SkeletonActiveDecorator.Paint(layer, EachActor, EachActor.FloorCoordinate, Label_Skeleton);
					}
				}

				if (activeSkeleton || skill.Rune == 3) {
					IAttribute attr;
					switch (skill.Rune) {
						case 0: attr = Hud.Sno.Attributes.Power_Buff_4_Visual_Effect_A; break;
						case 1: attr = Hud.Sno.Attributes.Power_Buff_4_Visual_Effect_B; break;
						case 2: attr = Hud.Sno.Attributes.Power_Buff_4_Visual_Effect_C; break;
						case 3: attr = Hud.Sno.Attributes.Power_Buff_4_Visual_Effect_D; break;
						case 4: attr = Hud.Sno.Attributes.Power_Buff_4_Visual_Effect_E; break;
						default: attr = Hud.Sno.Attributes.Power_Buff_4_Visual_Effect_None; break;
					}
			
					var TargetonScreen = false;
					foreach (IMonster monster in Hud.Game.AliveMonsters) {
						if (monster.GetAttributeValueAsUInt(attr, Hud.Sno.SnoPowers.Necromancer_CommandSkeletons.Sno, 2) == 1) {
							TargetonScreen = true;
							
							if (ActiveTarget == null) {
								ActiveTarget = monster;
								if (Hud.Game.Me.Powers.BuffIsActive(476584, 0)) {
									ResetRinger(false);
									StartRinger();
								}
							}
							else if (ActiveTarget != monster) {
								ActiveTarget = monster;
								if (Hud.Game.Me.Powers.BuffIsActive(476584, 0)) {
									ResetRinger(true);
									StartRinger();
								}
							}
							
							var TargetisElite = (monster.IsElite && monster.SummonerAcdDynamicId == 0 && monster.Rarity != ActorRarity.RareMinion);
							if (TargetisElite)
								SkeletonTargetEliteDecorator.Paint(layer, null, monster.FloorCoordinate, null);
							else if (!TargetOnEliteOnly)
								SkeletonTargetDecorator.Paint(layer, null, monster.FloorCoordinate, null);
							
							break;
						}
					}
						
					if (Hud.Game.Me.Powers.BuffIsActive(476584, 0)) {
						if (SkeletonCount == 0) {
							ResetRinger(true);
							StartRinger();
						}
						if (!TargetonScreen) {
							ActiveTarget = null;
							ResetRinger(false);
						}
					}
				}
				else {
					ResetRinger(true);
				}
			}
			else SkeletonCount = 0;

			// For Skeleton Mages, only when equipping the skill
			if (ShowMage && Hud.Game.Me.Powers.UsedSkills.Where(x => x.SnoPower.Sno == MageSkillSNO) != null) {
				var MageActors = Hud.Game.Actors.Where(EachActor => EachActor.SummonerAcdDynamicId == Hud.Game.Me.SummonerId && MageActorSNOs.Contains(EachActor.SnoActor.Sno));
				MageCount = MageActors.Count();

				foreach (var EachActor in MageActors)
					MageDecorator.Paint(layer, EachActor, EachActor.FloorCoordinate, Label_Mage);
			}
			else MageCount = 0;
				
			
			// For Golem, only when equipping the skill
			if (ShowGolem && Hud.Game.Me.Powers.UsedSkills.Where(x => x.SnoPower.Sno == GolemSkillSNO) != null) {
				var GolemActors = Hud.Game.Actors.Where(EachActor => EachActor.SummonerAcdDynamicId == Hud.Game.Me.SummonerId && GolemActorSNOs.Contains(EachActor.SnoActor.Sno));

				foreach (var EachActor in GolemActors)
					GolemDecorator.Paint(layer, EachActor, EachActor.FloorCoordinate, Label_Golem);
			}

			// For Simulacrum, only when equipping the skill
			if (ShowSimulacrum && Hud.Game.Me.Powers.UsedSkills.Where(x => x.SnoPower.Sno == SimulacrumSkillSNO) != null) {
				var SimulacrumActors = Hud.Game.Actors.Where(EachActor => EachActor.SummonerAcdDynamicId == Hud.Game.Me.SummonerId && SimulacrumActorSNOs.Contains(EachActor.SnoActor.Sno));

				foreach (var EachActor in SimulacrumActors)
					SimulacrumDecorator.Paint(layer, EachActor, EachActor.FloorCoordinate, Label_Simulacrum);
			}
			
			// For Revive, only when equipping the skill
			if (ShowRevive && Hud.Game.Me.Powers.UsedSkills.Where(x => x.SnoPower.Sno == ReviveSkillSNO) != null) {
				var ReviveActors = Hud.Game.Actors.Where(EachActor => EachActor.SummonerAcdDynamicId == Hud.Game.Me.SummonerId && EachActor.SnoActor.Sno.ToString().Contains("_p6_necro_revive"));
				ReviveCount = ReviveActors.Count();

				foreach (var EachActor in ReviveActors)
					ReviveDecorator.Paint(layer, EachActor, EachActor.FloorCoordinate, Label_Revive);
			}
			else ReviveCount = 0;
		}

		public void PaintTopInGame(ClipState clipState) {
			if (clipState != ClipState.BeforeClip) return;
			if (Hud.Game.Me.HeroClassDefinition.HeroClass != HeroClass.Necromancer) return;

			if (SkeletonCount != 0) SkeletonCountLabel.Paint(barX, barY, barW, barH, HorizontalAlign.Center);
			if (MageCount != 0) MageCountLabel.Paint(barX + 2*barW, barY, barW, barH, HorizontalAlign.Center);
			if (ReviveCount != 0) ReviveCountLabel.Paint(barX + 4*barW, barY, barW, barH, HorizontalAlign.Center);
			
			if (ActiveTimeLong.IsRunning || ActiveTime.IsRunning) {
				RingerTexture.Draw(Xpos, Ypos, texsize, 2*texsize);

				var text = "";
				if (showTimerShort) {
					var acctime1 = ActiveTime.ElapsedMilliseconds/1000;
					var accpower1 = (int) acctime1 * legpower;
					text += ValueToString(acctime1, ValueFormat.NormalNumberNoDecimal) + " sec +" + accpower1 + "%";
				}
				if (showTimerLong) {
					var acctime2 = ActiveTimeLong.ElapsedMilliseconds/1000;
					var accpower2 = (int) acctime2 * legpower;
					if (showTimerShort) text += "\n";
					text += ValueToString(acctime2, ValueFormat.NormalNumberNoDecimal) + " sec +" + accpower2 + "%";
				}

				var textLayout = ActiveFont.GetTextLayout(text);
				ActiveFont.DrawText(textLayout, Xpos + texsize, Ypos + texsize - textLayout.Metrics.Height/2);
			}
		}
		
		public void OnNewArea(bool newGame, ISnoArea area) {
			if (Hud.Game.Me.HeroClassDefinition.HeroClass != HeroClass.Necromancer) return;
			
			ActiveTarget = null;
			ResetRinger(true);
			
			var items = Hud.Game.Items.Where(x => x.Location == ItemLocation.RightHand);
			foreach (var item in items) {
				if (item.SnoItem.Sno != Hud.Sno.SnoItems.P6_Unique_Phylactery_02.Sno) continue;
				
				foreach (var perfection in item.Perfections) {
					if (perfection.Attribute == Hud.Sno.Attributes.Item_Power_Passive) {
						legpower = (int) (perfection.Cur*100);
						return;
					}
				}
			}
		}
		
		public void OnItemLocationChanged(IItem item, ItemLocation from, ItemLocation to) {
			if (Hud.Game.Me.HeroClassDefinition.HeroClass != HeroClass.Necromancer) return;
			if (to != ItemLocation.RightHand) return;
			if (item.SnoItem.Sno == Hud.Sno.SnoItems.P6_Unique_Phylactery_02.Sno) {
				foreach (var perfection in item.Perfections) {
					if (perfection.Attribute == Hud.Sno.Attributes.Item_Power_Passive) {
						legpower = (int) (perfection.Cur*100);
						ActiveTarget = null;
						ResetRinger(true);
						return;
					}
				}
			}
			else {
				ActiveTarget = null;
				ResetRinger(true);
				return;
			}
		}
		
		public void ResetRinger(bool resetAll) {
			if (ActiveTime.IsRunning) {
				ActiveTime.Stop();
				ActiveTime.Reset();
			}
			
			if (resetAll && ActiveTimeLong.IsRunning) {
				ActiveTimeLong.Stop();
				ActiveTimeLong.Reset();
			}
		}
		
		public void StartRinger() {
			if (GRonly && Hud.Game.SpecialArea != SpecialArea.GreaterRift) return;
			if (Bossonly && Hud.Game.RiftPercentage < 100) return;
			
			if (!ActiveTime.IsRunning)
				ActiveTime.Start();
			
			if (!ActiveTimeLong.IsRunning)
				ActiveTimeLong.Start();
		}
	}
}