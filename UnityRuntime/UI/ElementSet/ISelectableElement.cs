namespace UnityTools.UnityRuntime.UI.ElementSet
{
    public interface ISelectableElement
    {
        bool IsSelected { get; }
        void SetSelected(bool selected);
    }
}
