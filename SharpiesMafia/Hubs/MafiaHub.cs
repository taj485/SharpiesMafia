using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using SharpiesMafia.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

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
            var gameId = GenerateCode();
            var user = new User() { name = userName, connection_id = Context.ConnectionId, game_id = gameId, is_dead = false};
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            await Clients.Group("gameOwner").SendAsync("StartPageUserList", GetAllUsers());
        }

        public int GenerateCode()
        {
            int _min = 1000;
            int _max = 9999;
            Random _rdm = new Random();  
            return _rdm.Next(_min, _max);
        }

        public List<User> GetAllUsers()
        {
            var users = _context.Users.ToList();
            return users; 
        }

        public Task AddUserToGroup(string groupName)
        {
            return Groups.AddToGroupAsync(Context.ConnectionId,groupName);
        }

        public async Task ListUsersToKill()
        {
            await AddUserToGroup("mafia");
            await Clients.Group("mafia").SendAsync("LoadUsersToKill", GetAllUsers());
        }

        public async Task KillPlayer(string userName)
        {
            var deadUser = _context.Users.Where(x => x.name == userName).FirstOrDefault();
             
            deadUser.is_dead = true;
            _context.Users.Update(deadUser);
            _context.SaveChanges();     
            await Clients.All.SendAsync("LoadNight");
        }

    }
}