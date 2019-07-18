using System;
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
                await Clients.Group("gameMember").SendAsync("JoinPageUserList", GetSpecificGameUsers(gameId), gameId);
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
            var aliveUsers = _context.Users
                .Where(x => x.is_dead == false)
                .Where(x => x.game_id == GetGameId().FirstOrDefault())
                .ToList();
            return aliveUsers;
        }

        public Task AddUserToGroup(string groupName)
        {
            return Groups.AddToGroupAsync(Context.ConnectionId,groupName);
        }

        public void ListUsersToKill()
        {
            Clients.Group("villager").SendAsync("LoadMafiaNight");
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

        public async Task voteToKill(string userName)
        {
            var chosenUser = _context.Users.Where(x => x.name == userName).FirstOrDefault();
            chosenUser.vote_count += 1;
            _context.Users.Update(chosenUser);
            await _context.SaveChangesAsync();
        }

        public async Task totalVotes()
        {
            var gameId = GetGameId().FirstOrDefault();
            
            int mostVotes = _context.Users
                .Where(x => x.game_id == gameId)
                .Select(user => user.vote_count)
                .DefaultIfEmpty(0).Max();
            
            var chosenUser = _context.Users
                .Where(x => x.game_id == gameId)
                .Where(user => user.vote_count == mostVotes)
                .FirstOrDefault();
            
            await KillPlayer(chosenUser.name, "villager");
            //await resetVoteCount();
        }

        public async Task resetVoteCount()
        {
            var gameId = GetGameId().FirstOrDefault();

            var users = _context.Users.Where(x => x.game_id == gameId);
            foreach (var user in users)
            {
                if (user.vote_count != 0)
                {
                    user.vote_count = 0;
                    _context.Users.Update(user);
                }
            }

            await _context.SaveChangesAsync();

        }

        public async Task KillPlayer(string userName, string role)
        {
            var deadUser = _context.Users.Where(x => x.name == userName).FirstOrDefault();
            
            var deadUserConnectionId = deadUser.connection_id;
            await AddUserByIdToGroup("lastVictim", deadUserConnectionId);
            await RemoveUserByIdFromGroup(deadUser.role, deadUserConnectionId);
             
            deadUser.is_dead = true;
            _context.Users.Update(deadUser);
            await _context.SaveChangesAsync();

            var rolesCount = TotalRoles();

            if(role == "mafia")
            {
                await Clients.Groups("mafia", "villager", "lastVictim").SendAsync("LoadNight");
                await Clients.Groups("mafia", "villager", "lastVictim").SendAsync("LoadDayPage");
                await Clients.Group("lastVictim").SendAsync("YouDiedPageDelayed");
                await Clients.Groups("mafia", "villager").SendAsync("LoadVictimResult", deadUser.name);
                await Clients.Groups("mafia", "villager").SendAsync("EveryoneKillChoice", GetAliveUsers());
            }
            else
            {
                await Clients.Groups("mafia", "villager").SendAsync("LoadVoteResult",deadUser.name, deadUser.role, rolesCount);
                await Clients.Group("lastVictim").SendAsync("YouDiedPageInstant");
            }
            await RemoveUserByIdFromGroup("lastVictim", deadUserConnectionId);
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
            var currentUser = _context.Users
                                    .Where(x => x.connection_id == Context.ConnectionId).FirstOrDefault();
            var gameId = currentUser.game_id;
            var users = GetSpecificGameUsers(gameId);
            if (role == "mafia")
            {
                await Clients.All.SendAsync("VillagerWin", users);
            }
            else
            {
                await Clients.All.SendAsync("MafiaWin", users);
            }
        }

        public async Task LoopGame()
        {
            await resetVoteCount();
            await Clients.Groups("mafia", "villager").SendAsync("NightPage");
        }

    }
}