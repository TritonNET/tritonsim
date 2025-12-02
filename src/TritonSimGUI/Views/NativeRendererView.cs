namespace TritonSimGUI.Views
{
    public class NativeRendererView : View 
    {
        public void Start() =>
            Handler?.Invoke(nameof(Start), null);

        public void Stop() =>
            Handler?.Invoke(nameof(Stop), null);
    }
}
