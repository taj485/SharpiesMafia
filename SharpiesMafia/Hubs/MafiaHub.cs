using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace SharpiesMafia.Hubs
{
    public class MafiaHub : Hub
    {
        //This was the example method from the chatroom article example
        //public async Task SendMessage(string user, string message)
        //{
        //    await Clients.All.SendAsync("ReceiveMessage", user, message);
        //}
    }
}