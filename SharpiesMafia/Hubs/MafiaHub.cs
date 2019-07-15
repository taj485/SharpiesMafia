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
        public async Task SendMessage(string user, int gameId)
        {
            await Clients.All.SendAsync("ReceiveMessage", user, gameId);
        }

        public async Task StartGame(string userName)
        {
            var gameId = GenerateCode();
            var user = new User() { name = userName, connection_id = Context.ConnectionId, game_id = gameId, is_dead = false, role = "villager"};
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            await Clients.Group("gameOwner").SendAsync("StartPageUserList", GetAllUsers(), GetGameId());
        }

        public async Task JoinGame(string userName, int gameId)
        {
            var user = new User() { name = userName, connection_id = Context.ConnectionId, game_id = gameId, is_dead = false };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            await Clients.Group("gameOwner").SendAsync("JoinPageUserList", GetSpecificGameUsers(gameId));
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

        public IQueryable<long> GetGameId()
        {
            var code = from user in _context.Users
                       where user.connection_id == Context.ConnectionId
                       select user.game_id;

            return code;
        }
        
        public List<User> GetSpecificGameUsers(int gameId)
        {
            var users = _context.Users.Where(user=>user.game_id == gameId).ToList();
            return users;
        }
         
        public List<User> GetAliveUsers()
        {
            var aliveUsers = _context.Users.Where(x => x.is_dead == false).ToList();
            return aliveUsers;
        }

        public Task AddUserToGroup(string groupName)
        {
            return Groups.AddToGroupAsync(Context.ConnectionId,groupName);
        }

        public async Task ListUsersToKill()
        {
            await Clients.Group("mafia").SendAsync("LoadUsersToKill", GetAliveUsers());
        }

        public async Task ListEveryOneToKill()
        {
            await Clients.All.SendAsync("EveryoneKillChoice", GetAliveUsers());
        }

        public async Task KillPlayer(string userName, string role)
        {
            var deadUser = _context.Users.Where(x => x.name == userName).FirstOrDefault();
             
            deadUser.is_dead = true;
            _context.Users.Update(deadUser);
            _context.SaveChanges();

            var rolesCount = TotalRoles();

            if(role == "mafia")
            {
                await Clients.All.SendAsync("LoadNight");
            }
            else
            {
                await Clients.All.SendAsync("LoadResult",deadUser.name, deadUser.role, rolesCount);
            }
           
        }

        public List<int> TotalRoles()
        {
            List<int> rolesCount = new List<int>();
            var aliveUsers = GetAliveUsers();
            int aliveMafia = 0;
            int aliveVillagers = 0;
            foreach (var user in aliveUsers)
            {
                if (user.role == "mafia")
                {
                    aliveMafia++;
                }
                else
                {
                    aliveVillagers++;
                }
            }
            rolesCount.Add(aliveMafia);
            rolesCount.Add(aliveVillagers);
            return rolesCount;
        }
        
        public Task AddUserToRole(string groupName, string connectionId)
        {
            return Groups.AddToGroupAsync(connectionId,groupName);
        }

        public async Task BeginGame()
        {
            MafiaAssignment();
            var users = GetAllUsers();
            foreach (var user in users)
            {
                await AddUserToRole(user.role, user.connection_id);
            }
            await Clients.Group("villager").SendAsync("VillagerPage");
            await Clients.Group("mafia").SendAsync("MafiaPage");
        }

        public void MafiaAssignment()
        {
            var users = GetAllUsers();
            int numberOfUsers = users.Count;
            int amountMafia = numberOfUsers / 4;
            if (amountMafia < 1)
            {
                amountMafia = 1;
            }
            
            List<int> dupeChecker = new List<int>();
            var random = new Random();

            for (int i = 0; i < amountMafia; i++)
            {
                int index = random.Next(0, numberOfUsers - 1);
                if (!dupeChecker.Contains(index))
                {
                    var randomUser = users[index];
                    randomUser.role = "mafia";
                    _context.Users.Update(randomUser);
                    dupeChecker.Add(index);
                }
                else
                {
                    amountMafia++;
                }
            }
            _context.SaveChanges();
        }
    }
}