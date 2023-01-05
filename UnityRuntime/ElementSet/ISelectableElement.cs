
namespace UnityTools.UnityRuntime.ElementSet
{
    public interface ISelectableElement
    {
        bool IsSelected { get; }
        void SetSelected(bool selected);
    }
}
