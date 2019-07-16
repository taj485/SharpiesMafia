﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using SharpiesMafia.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Server.Kestrel.Core;

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
            if (UserExists(userName))
            {
                Console.WriteLine(userName);
                throw new Exception("User already exists");
            }
            else
            {
                var gameId = GenerateCode();
                var user = new User() { name = userName, connection_id = Context.ConnectionId, game_id = gameId, is_dead = false, role = "villager"};
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
                await Clients.Group("gameOwner").SendAsync("StartPageUserList", GetSpecificGameUsers(gameId), GetGameId()); 
            }
        }
        
        public async Task JoinGame(string userName, int gameId)
        {
            if (UserExists(userName))
            {
                Console.WriteLine(userName);
                throw new Exception("User already exists");
            }
            else
            {
                var user = new User() { name = userName, connection_id = Context.ConnectionId, game_id = gameId, is_dead = false, role = "villager" };
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
                await Clients.Group("gameMember").SendAsync("JoinPageUserList", GetSpecificGameUsers(gameId));
                await Clients.Group("gameOwner").SendAsync("StartPageUserList", GetSpecificGameUsers(gameId), GetGameId());
            }
        }
        
        public bool UserExists(string userName)
        {
            var users = GetAllUsers();
            foreach (var user in users)
            {
                if (userName == user.name)
                {
                    return true;
                }
            }

            return false;
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
        
        public List<User> GetSpecificGameUsers(long gameId)
        {
            var users = _context.Users.Where(user=>user.game_id == gameId).ToList();
            return users;
        }
         
        public List<User> GetAliveUsers()
        {
            var aliveUsers = _context.Users.Where(x => x.is_dead == false && x.game_id == GetGameId().FirstOrDefault()).ToList();
            return aliveUsers;
        }

        public Task AddUserToGroup(string groupName)
        {
            return Groups.AddToGroupAsync(Context.ConnectionId,groupName);
        }

        public void ListUsersToKill()
        {
            Clients.Group("villager").SendAsync("LoadMafiaNight", GetAliveUsers());
            Clients.Group("mafia").SendAsync("LoadUsersToKill", GetAliveUsers());
        }

        public void LoadMafiaChoicePage()
        {
            Clients.Group("mafia").SendAsync("UsersToKillPage");
        }

        public async Task ListEveryOneToKill()
        {
            await Clients.All.SendAsync("EveryoneKillChoice", GetAliveUsers());
        }

        public void voteToKill(string userName)
        {
            var chosenUser = _context.Users.Where(x => x.name == userName).FirstOrDefault();
            chosenUser.vote_count += 1;
            _context.Users.Update(chosenUser);
            _context.SaveChanges();
        }

        public async Task totalVotes()
        {
            var gameId = GetGameId().FirstOrDefault();

            int mostVotes = _context.Users
                .Where(x => x.game_id == gameId)
                .Select(user => user.vote_count)
                .DefaultIfEmpty(0).Max();

            var chosenUser = _context.Users.Where(user => user.vote_count == mostVotes).FirstOrDefault();
            await KillPlayer(chosenUser.name, "villager");
        }

        public async Task KillPlayer(string userName, string role)
        {
            var deadUser = _context.Users.Where(x => x.name == userName).FirstOrDefault();
            
            var deadUserConnectionId = deadUser.connection_id;
            await Clients.Groups("mafia", "villager").SendAsync("UpdateVictimGroup", deadUserConnectionId);
             
            deadUser.is_dead = true;
            _context.Users.Update(deadUser);
            _context.SaveChanges();

            var rolesCount = TotalRoles();

            if(role == "mafia")
            {
                await Clients.All.SendAsync("LoadNight");
                await Clients.Groups("mafia", "villager").SendAsync("LoadDayPage");
                await Clients.Group("lastVictim").SendAsync("YouDiedPageDelayed");
                await Clients.AllExcept(deadUserConnectionId).SendAsync("EveryoneKillChoice", GetAliveUsers());
            }
            else
            {
                await Clients.AllExcept(deadUserConnectionId).SendAsync("LoadResult",deadUser.name, deadUser.role, rolesCount);
                await Clients.Group("lastVictim").SendAsync("YouDiedPageInstant");
            }
            await Clients.All.SendAsync("DeleteVictimGroup", deadUserConnectionId);
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
            await Clients.Groups("mafia", "villager").SendAsync("NightPage");
        }

        public void MafiaAssignment()
        {
            var currentUser = _context.Users
                        .Where(x => x.connection_id == Context.ConnectionId).FirstOrDefault();
            var gameId = currentUser.game_id;
                        
            var users = GetSpecificGameUsers(gameId);
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

        public async Task ResultsScreen(string deathRole)
        {
            await Clients.Group("gameOwner").SendAsync("ResultsScreen", deathRole, true);
            await Clients.Group("gameMember").SendAsync("ResultsScreen", deathRole, false);
        }

        public async Task ResetGame()
        {
            var gameId = GetGameId().FirstOrDefault();
            var users = _context.Users.Where(user => user.game_id == gameId);

            foreach (var user in users)
            {
                if (user.is_dead == true)
                {
                    user.is_dead = false;
                }

                if (user.role == "mafia")
                {
                    user.role = "villager";
                }
            }

            await _context.SaveChangesAsync();

            await Clients.Group("gameOwner").SendAsync("StartPageUserList", users, Convert.ToInt32(gameId));
            await Clients.Group("gameMember").SendAsync("JoinPageUserList", GetSpecificGameUsers(Convert.ToInt32(gameId)));
        }

        public Task AddUserByIdToGroup(string groupName, string connectionId)
        {
            return Groups.AddToGroupAsync(connectionId, groupName);
        }

        public Task RemoveUserByIdFromGroup(string groupName, string connectionId)
        {
            return Groups.RemoveFromGroupAsync(connectionId, groupName);
        }

        public async Task WinnerPage(string role)
        {
            if (role == "mafia")
            {
                await Clients.All.SendAsync("VillagerWin");
            }
            else
            {
                await Clients.All.SendAsync("MafiaWin");
            }
        }
    }
}