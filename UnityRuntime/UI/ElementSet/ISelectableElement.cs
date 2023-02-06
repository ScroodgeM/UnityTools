//this empty line for UTF-8 BOM header

namespace UnityTools.UnityRuntime.UI.ElementSet
{
    public interface ISelectableElement
    {
        bool IsSelected { get; }
        void SetSelected(bool selected);
    }
}
