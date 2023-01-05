
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UnityTools.UnityRuntime.UI
{
    public class NoGraphicsImage : MaskableGraphic, IEventSystemHandler
    {
        public override void SetMaterialDirty() { return; }
        public override void SetVerticesDirty() { return; }

        protected override void OnPopulateMesh(VertexHelper toFill)
        {
            toFill.Clear();
        }
    }
}
