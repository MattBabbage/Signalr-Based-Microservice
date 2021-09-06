using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Dapr.Client;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using DaprSignalrBeta;

namespace Hubs
{

    public class ChatHub : Hub
    {
        public async Task NewChatMessage(string message)
        {
            try
            {
                var client = new DaprClientBuilder().Build();
                // Invokes a POST method named "convert" that takes input of type "Order"
                var data = new Order { value = 56, unitType = "Length", unitStart = "m", unitEnd = "mm" };
                var account = await client.InvokeMethodAsync<Order, double>("unitconvertormain", "convert", data);

                await Clients.All.SendAsync("ChatMessageReceived", (message + " Invoked Function: " + account.ToString()));
            }
            catch
            {
                await Clients.All.SendAsync("ChatMessageReceived", message);
            }

        }
    }
}