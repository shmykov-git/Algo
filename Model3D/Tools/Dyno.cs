using System.Collections.Generic;
using System.Linq;
using Model;
using Model.Extensions;

namespace Model3D.Tools
{

    public class Dyno
    {
        // vec2 + SurfaceFunc - точка
        // graph - основа
        // rules??
        // animation
    }



    public delegate Vector2 DynoFunc(int frameCount, Vector2 pos);

    public class Dyno<TDynoItem> where TDynoItem : IDynoItem
    {
        private List<DynoNode> nodes = new List<DynoNode>();

        public Dyno() { }

        public Dyno(IEnumerable<(TDynoItem item, DynoFunc[] rules)> itemRules)
        {
            AddItemRulesRange(itemRules);
        }

        public void Animate(int maxFrameCount)
        {
            for (var frameCount = 0; frameCount < maxFrameCount; frameCount++)
            {
                foreach (var node in nodes)
                {
                    node.Pos = node.rules.Select(rule => rule(frameCount, node.Pos)).Sum();
                }
            }
        }

        public void AddItemRules(TDynoItem item, params DynoFunc[] rules)
        {
            nodes.Add(new DynoNode
            {
                item = item,
                rules = rules
            });
        }

        public void AddItemRulesRange(IEnumerable<(TDynoItem item, DynoFunc[] rules)> itemRules)
        {
            foreach (var itemRule in itemRules)
                AddItemRules(itemRule.item, itemRule.rules);
        }

        class DynoNode
        {
            public TDynoItem item;
            public DynoFunc[] rules;

            public Vector2 Pos
            {
                get => item.pos;
                set => item.pos = value;
            }
        }
    }


}
