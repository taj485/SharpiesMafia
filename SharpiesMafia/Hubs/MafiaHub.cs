using System;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace SharpiesMafia.Hubs
{
    public class MafiaHub : Hub
    {
        //This was the example method from the chatroom article example
        public async Task SendMessage(string user, int code)
        {
            await Clients.All.SendAsync("ReceiveMessage", user, code);
        }

        public async Task StartGame(string user)
        {
            await Clients.All.SendAsync("ReceiveMessage", user, GenerateCode());
        }

        public int GenerateCode()
        {
            int _min = 1000;
            int _max = 9999;
            Random _rdm = new Random();  
            return _rdm.Next(_min, _max);
        }
    }
}