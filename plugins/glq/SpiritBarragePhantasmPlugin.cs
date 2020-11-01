// Modified. Original (by glq): https://www.ownedcore.com/forums/diablo-3/turbohud/turbohud-discussions/618926-damage-count-down-circle-of-spirit-barrage-post3728819.html#post3728819
using System;
using System.Linq;
using System.Collections.Generic;
using Turbo.Plugins.Default;
using SharpDX;
using SharpDX.Direct2D1;

namespace Turbo.Plugins.glq
{
	public class SpiritBarragePhantasmPlugin : BasePlugin, IInGameWorldPainter, ICustomizer, INewAreaHandler
	{

		private Dictionary<uint,int> Phantasms { get;set; } = new Dictionary<uint,int>();
		private int MyIndex { get; set; } = -1;
		
		private TopLabelWithTitleDecorator PhantomCountDecorator { get; set; }

		private IBrush BrushSolid { get; set; }
		private IBrush BrushDash { get; set; }
		private IBrush BrushCounter { get; set; }		
        private IBrush TimeLeftClockBrush { get; set; }	

		private IFont FontText { get; set; }		

		private const int radius_countdown = 25;
		
		public bool ShowOthers {get; set;}
		public bool ShowCounter {get; set;}
		public bool ShowExplosionCircle {get; set;}
		public int CircleSeconds {get; set;}
	
		public SpiritBarragePhantasmPlugin()
		{
			Enabled = true;
		}

		public override void Load(IController hud)
		{
			base.Load(hud);
			Order = 1001;
			
			ShowOthers = true;			// Also show for other players.
			ShowCounter = true;			// Counter
			ShowExplosionCircle = true;	// Show  additional circle after "CircleSeconds"
			CircleSeconds = 8;			// 0 .. 10 ,  Draw additional circle after this seconds ( 8 -> the last 2 seconds)
	
			BrushDash = hud.Render.CreateBrush(150, 0, 128, 255, 3, SharpDX.Direct2D1.DashStyle.Dash);
			BrushSolid = hud.Render.CreateBrush(255, 0, 128, 255, 2);			
			BrushCounter = hud.Render.CreateBrush(255, 0, 128, 255, 0);
            TimeLeftClockBrush = Hud.Render.CreateBrush(220, 0, 0, 0, 0);
			
			FontText = Hud.Render.CreateFont("tahoma", 9f, 255, 100, 255, 150, true, false, 128, 0, 0, 0, true);
											
			PhantomCountDecorator = new TopLabelWithTitleDecorator(Hud)
            {
                BackgroundBrush = Hud.Render.CreateBrush(80, 134, 238, 240, 0),
                BorderBrush = Hud.Render.CreateBrush(255, 0, 0, 0, -1),
                TextFont = Hud.Render.CreateFont("tahoma", 8, 255, 255, 0, 0, true, false, true),
            };
					
		}

		public void Customize()
		{
			if ((CircleSeconds < 0) || (CircleSeconds > 10)) 	{ CircleSeconds = 8; }				
		}

		public void OnNewArea(bool newGame, ISnoArea area)
		{	
			if (newGame || (MyIndex != Hud.Game.Me.Index) )   // Fix partialment the newGame limitation
			{
				MyIndex = Hud.Game.Me.Index;
				Phantasms.Clear();
			}
 		}

        private void DrawTimeLeftClock(RectangleF rect, double elapsed, double timeLeft)   // plugins\Default\BuffLists\Painter\BuffPainter.cs
        {
            if ((timeLeft > 0) && (elapsed >= 0) && (TimeLeftClockBrush != null))
            {
                var endAngle = Convert.ToInt32(360.0d / (timeLeft + elapsed) * elapsed);
                var startAngle = 0;
                TimeLeftClockBrush.Opacity = 1 - (float)(0.5f / (timeLeft + elapsed) * elapsed);
                var rad = rect.Width; // * 0.45f;
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
		
		public void PaintWorld(WorldLayer layer)
		{
			if (!Hud.Game.IsInGame) return;			
			if (layer != WorldLayer.Ground) return;
			
			var actors = Hud.Game.Actors.Where(a => a.SnoActor.Sno == ActorSnoEnum._wd_spiritbarragerune_aoe_ghostmodel);										
			if (actors.Any())
			{
				foreach(var a in actors) 
				{ 
				  if (!Phantasms.ContainsKey(a.AnnId)) { Phantasms[a.AnnId] = a.CreatedAtInGameTick; }
				}
				var total = 0;
				foreach(var player in Hud.Game.Players)
				{					
					if (!ShowOthers && !player.IsMe) continue;
					var actorsPlayer = actors.Where(a => a.SummonerAcdDynamicId == player.SummonerId).OrderByDescending(a => Phantasms[a.AnnId]);
					var c = 0; 
					foreach (var actor in actorsPlayer)
					{
						if (c++ == 3) break;
						total++;	
						var duration = player.Powers.BuffIsActive(484270)? 10d : 5d;						
						var elapsed = (Hud.Game.CurrentGameTick - Phantasms[actor.AnnId]) / 60d;
						if (elapsed < (duration + 0.1) )
						{
							BrushSolid.DrawWorldEllipse(10, -1, actor.FloorCoordinate);	
							if  (ShowExplosionCircle && (elapsed > CircleSeconds))
							{
								BrushDash.DrawWorldEllipse(15, -1, actor.FloorCoordinate);
							}
							if (actor.IsOnScreen)
							{
								var timeleft = duration - elapsed; 	var x = actor.FloorCoordinate.ToScreenCoordinate().X; var y = actor.FloorCoordinate.ToScreenCoordinate().Y; 
								var radiusc = radius_countdown / 1200.0f * Hud.Window.Size.Height;
								
								BrushCounter.DrawEllipse(x , y , radiusc , radiusc);				
								DrawTimeLeftClock(new RectangleF(x - radiusc/2, y - radiusc/2 , radiusc, radiusc), elapsed , timeleft );
								
								var layout = FontText.GetTextLayout( (timeleft < 0)?"0.0":timeleft.ToString((timeleft > 1)?"F0":"F1") );
								FontText.DrawText(layout, x - layout.Metrics.Width/2 , y - layout.Metrics.Height/2 - 1);	
							}							
						}
					}									
				}
				if (ShowCounter) 
				{
					var uiRect = Hud.Render.GetUiElement("Root.NormalLayer.game_dialog_backgroundScreenPC.game_progressBar_manaBall").Rectangle;							
					PhantomCountDecorator.Paint(uiRect.Left + uiRect.Width * 0.20f, uiRect.Top - uiRect.Height * 0.20f, uiRect.Width * 0.60f, uiRect.Height * 0.15f, "Phantom:" + total);
				}
			}			
		}
    }
}