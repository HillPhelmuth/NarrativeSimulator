using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components;

namespace AINarrativeSimulator.Components;
// This class provides an example of how JavaScript functionality can be wrapped
// in a .NET class for easy consumption. The associated JavaScript module is
// loaded on demand when first needed.
//
// This class can be registered as scoped DI service and then injected into Blazor
// components for use.

public class ResizableGridJsInterop : IAsyncDisposable
{
    private readonly Lazy<Task<IJSObjectReference>> moduleTask;

    public ResizableGridJsInterop(IJSRuntime jsRuntime)
    {
        moduleTask = new(() => jsRuntime.InvokeAsync<IJSObjectReference>(
            "import", "./_content/AINarrativeSimulator.Components/resizableGrid.js").AsTask());
    }

    // Existing sample method (template leftover)
    public async ValueTask<string> Prompt(string message)
    {
        var module = await moduleTask.Value;
        return await module.InvokeAsync<string>("showPrompt", message);
    }

    // Added interop methods for exported JS functions in resizableGrid.js
    public async ValueTask InitResizableGrid(ElementReference gridElement)
    {
        var module = await moduleTask.Value;
        await module.InvokeVoidAsync("initResizableGrid", gridElement);
    }

    public async ValueTask ReinitGrid(ElementReference gridElement, bool isTwoColumn)
    {
        var module = await moduleTask.Value;
        await module.InvokeVoidAsync("reinitGrid", gridElement, isTwoColumn);
    }

    public async ValueTask ScrollToBottom(string elementId)
    {
        var module = await moduleTask.Value;
        await module.InvokeVoidAsync("scrollToBottom", elementId);
    }

    public async ValueTask DisposeAsync()
    {
        if (moduleTask.IsValueCreated)
        {
            var module = await moduleTask.Value;
            await module.DisposeAsync();
        }
    }
}