using System.Linq;

using Turbo.Plugins;
using Turbo.Plugins.Default;

namespace Turbo.plugins.PAIN
{
    public sealed class PainEnhancerHelper : BasePlugin, IInGameWorldPainter
    {
        private WorldDecoratorCollection PlayerLabel { get; set; }

        private WorldDecoratorCollection BleedRadiusDecorator { get; set; }

        private WorldDecoratorCollection NoBleedDecorator { get; set; }

        private IAttribute m_PowerBuff1;
        private uint m_PainEnhancerPrimarySno;

        public PainEnhancerHelper()
        {
            Enabled = false;
        }

        public override void Load(IController hud)
        {
            base.Load(hud);

            m_PowerBuff1 = Hud.Sno.Attributes.Power_Buff_1_Visual_Effect_None;
            m_PainEnhancerPrimarySno = Hud.Sno.SnoPowers.PainEnhancerPrimary.Sno;

            NoBleedDecorator = new WorldDecoratorCollection(
                new GroundCircleDecorator(Hud)
                {
                    Brush = Hud.Render.CreateBrush(255, 0, 255, 0, 20)
                });

            PlayerLabel = new WorldDecoratorCollection(
                new GroundLabelDecorator(Hud)
                {
                    TextFont = Hud.Render.CreateFont("tahoma", 6.5f, 255, 0, 255, 0, false, false, false),
                    BackgroundBrush = Hud.Render.CreateBrush(255, 0, 0,0, 0)
                });

            BleedRadiusDecorator = new WorldDecoratorCollection(new GroundCircleDecorator(Hud)
            {
                Brush = Hud.Render.CreateBrush(155, 255, 0, 0, 2.0f),
                Radius = 20,
                Enabled = true
            });
        }

        public void PaintWorld(WorldLayer layer)
        {
            var player = Hud.Game.Me;

            if (player.IsInTown)
                return;

            if (player.Powers.UsedLegendaryGems.PainEnhancerPrimary?.Active != true)
                return;

            var monsters = Hud.Game.AliveMonsters.ToList();
            var noBleedMonsters = monsters.Where(m => m.GetAttributeValueAsInt(m_PowerBuff1, m_PainEnhancerPrimarySno) != 1).ToList();
            var bleedCount = monsters.Except(noBleedMonsters).Count(m => m.NormalizedXyDistanceToMe <= 20);
            foreach (var m in noBleedMonsters)
            {
                NoBleedDecorator.Paint(layer, m, m.FloorCoordinate, string.Empty);
            }

            PlayerLabel.Paint(layer, player, player.FloorCoordinate, $"Bleeding: {bleedCount}\nAttack Speed: {bleedCount * 3}%");
            BleedRadiusDecorator.Paint(layer, player, player.FloorCoordinate, string.Empty);
        }
    }
}
 