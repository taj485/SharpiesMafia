using System;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using SharpiesMafia.Models;

namespace SharpiesMafia.Hubs
{
    public class MafiaHub : Hub
    {
        private readonly MafiaContext _context;

        public MafiaHub (MafiaContext context)
        {
            _context = context;
        }

        //This was the example method from the chatroom article example
        public async Task SendMessage(string user, int code)
        {
            await Clients.All.SendAsync("ReceiveMessage", user, code);
        }

        public async Task StartGame(string userName)
        {
            var user = new User() { name = userName };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            await Clients.All.SendAsync("ReceiveMessage", Context.ConnectionId, GenerateCode());
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