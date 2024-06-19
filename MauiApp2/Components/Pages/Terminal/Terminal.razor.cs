using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Schema;
using MauiApp2.TcpIP;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace MauiApp2.Components.Pages.Terminal;



public partial class Terminal : ComponentBase
{


    [Parameter] 
    public string Host { get; set; } = "";
    [Parameter]
    public int Port { get; set; }
    private List<string> _messages = new List<string>();
    private string _buffer = "";
    private bool _newMessageReceived = false;
    public static Terminal Instance { get; private set; }
    

        

    
    [JSInvokable]
    public static async Task SendCommandToServer(string command)
    {
        await Instance.SendMessage(command);
    }

    private void OnInput(ChangeEventArgs e)
    {
        _buffer = e.Value.ToString();
    }
    
    private List<string> _history = new List<string>();
    MuTcpClient? _client;
    protected override void OnInitialized()
    {
        Instance = this;
        _client = new MuTcpClient(Host, Port, MessageReceived);
    }
    
    public async Task SendMessage(string message)
    {
        await InvokeAsync(() => _client.SendMessage(message));
    }
    
    private async void MessageReceived(string message)
    {
        // Check if the message contains a .less file URL it could be http or https
        
        var match = Regex.Match(message, @"F!@style:url:(http[s]?://[^\.]+\.[^\s]+)");
        
        if (match.Success)
        {
            var url = match.Groups[1].Value;

            // Download the .less file
            var httpClient = new HttpClient();
            // if the url is http change to https
            if (url.StartsWith("http://"))
            {
                url = url.Replace("http://", "https://");
            }
            Console.WriteLine(url);
            var lessContent = await httpClient.GetStringAsync(url);

            // Compile the .less file into CSS
            var cssContent = dotless.Core.Less.Parse(lessContent);

            // Add the CSS content to a style element on the page
            await JSRuntime.InvokeVoidAsync("addCssStyle", cssContent);
            await InvokeAsync(StateHasChanged);
            return;
        }
        
        _messages.Add(message);
        _newMessageReceived = true;
        await InvokeAsync(StateHasChanged);
    }
    
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (_newMessageReceived)
        {
            await JSRuntime.InvokeVoidAsync("addClickEvents");
            await JSRuntime.InvokeVoidAsync("scrollToBottom", _client.Id.ToString());
            _newMessageReceived = false;
        }
    }
    
    
    public RenderFragment Render() => builder =>
    {
        builder.OpenComponent(0, typeof(Terminal));
        builder.AddAttribute(1, "Host", Host);
        builder.AddAttribute(2, "Port", Port);
        builder.CloseComponent();
    };
    
    private async Task OnKeyUp(KeyboardEventArgs e)
    {
        if (e.Key == "Enter")
        {
            // Get the message from the textarea
            // This assumes you have a two-way data binding set up for the textarea
            await SendMessage(_buffer);
            _history.Add("Message Sent:" + _buffer);
            await InvokeAsync(() => {
                _buffer = "";
                StateHasChanged();
            });
            await InvokeAsync(StateHasChanged);
        } 
    }
}