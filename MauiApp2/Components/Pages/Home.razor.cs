using MauiApp2.Components.Pages.Terminal;
using Microsoft.AspNetCore.Components;


namespace MauiApp2.Components.Pages;



partial class Home : ComponentBase
{
    private List<Terminal.Terminal> _terminals = new List<Terminal.Terminal>();

    protected override async Task OnInitializedAsync()
    {
       var terminal = new Terminal.Terminal();
       terminal.Host = "code.deanpool.net";
       terminal.Port = 1701;
       
       _terminals.Add(terminal);
       await InvokeAsync(StateHasChanged);
    }
    
}