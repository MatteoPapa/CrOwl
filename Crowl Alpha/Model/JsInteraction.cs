using System.Windows.Threading;
using System;
using System.Diagnostics;

public class JsInteraction
{
    private readonly Dispatcher _dispatcher;
    private readonly Action<string> _elementClickedCallback;

    public JsInteraction(Dispatcher dispatcher, Action<string> elementClickedCallback)
    {
        _dispatcher = dispatcher;
        _elementClickedCallback = elementClickedCallback;
    }

    // Mark the method as virtual if you need to mock it for testing
    public void OnElementClicked(string dataUid)
    {
        // Use Dispatcher to ensure thread safety
        _dispatcher.Invoke(() =>
        {
            Debug.WriteLine("Is something happening?");
            _elementClickedCallback?.Invoke(dataUid);
        });
    }
}
