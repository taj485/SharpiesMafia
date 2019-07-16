"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/mafiaHub").build();

connection.on("LoadNight", function ()
{
    var targetDiv = $('#mafiaGame');
    targetDiv.load("/Home/LoadNightScreen");
});

connection.on("MafiaPage", function ()
{
    var targetDiv = $('#mafiaGame');
    targetDiv.load("/Home/MafiaScreen");
});

connection.on("VillagerPage", function ()
{
    var targetDiv = $('#mafiaGame');
    targetDiv.load("/Home/VillagerScreen");
});

function killPerson(user, role){
    connection.invoke("KillPlayer", user,role).catch(function (err) {
        return console.error(err.toString());
    });
}

connection.on("NightPage", function ()
{
    setTimeout(function () {
        GetNextPage("/Home/LoadNightScreen");
        connection.invoke("ListUsersToKill");
    }, 5000);
});

connection.on("LoadDayPage", function ()
{
    setTimeout(function () {
        GetNextPage("/Home/LoadDayScreen");
    }, 5000);
});

connection.on("UsersToKillPage", function () {
      setTimeout(function () {
        GetNextPage("/Home/UsersToKill");
    }, 5000);
});

connection.on("StartPageUserList", function (users, gameId) {     var targetDiv = $('#mafiaGame');
    targetDiv.load("/Home/StartGameScreen", function (responseTxt, statusTxt, xhr)
    {

        if (statusTxt == "success") {

            $("#gameId").html("Join Code: " + gameId);


            users.forEach(function (element) {
                var li = document.createElement("li");
                li.setAttribute('class', 'list-group-item');
               li.textContent = element.name;
               document.getElementById("userList").appendChild(li)
            });
        }
        if (statusTxt === "error") {
            alert("Error: " + xhr.status + ": " + xhr.statusText);
        }
    });
});

connection.on("LoadResult", function (name, role, rolesCount)
{
    var targetDiv = $('#mafiaGame');
    targetDiv.load("/Home/LoadResultScreen", function (responseTxt, statusTxt, xhr)
    {
        if (statusTxt == "success")
            if (role === "mafia") {
                document.getElementById("resultDisplay").classList.add("text-success");
            } else {
                document.getElementById("resultDisplay").classList.add("text-danger");
            }
        document.getElementById("deadUser").innerHTML = capitalize(name);
        document.getElementById("userRole").innerHTML = capitalize(role);
        document.getElementById("mafiaCount").innerHTML = rolesCount[0];
        document.getElementById("villagerCount").innerHTML = rolesCount[1];
        if(statusTxt == "error") {
            alert("Error: " + xhr.status + ": " + xhr.statusText);
        }
    });
});

connection.on("JoinPageUserList", function (users)
{
    var targetDiv = $('#mafiaGame');
    targetDiv.load("/Home/JoinGameScreen", function (responseTxt, statusTxt, xhr)
    {
        if (statusTxt == "success") {
            users.forEach(function (element) {
                var li = document.createElement("li");
                li.setAttribute('class', 'list-group-item');
                li.textContent = element.name;
                document.getElementById("joinUserList").appendChild(li)
            });
        }
        if(statusTxt == "error") {
            alert("Error: " + xhr.status + ": " + xhr.statusText);
        }
    });
});

connection.start().then(function(){
    document.getElementById("newGameStartBtn").disabled = false;
}).catch(function (err) {
    return console.error(err.toString());
});

$('#joinGameBtn').on("click", function () {
    connection.invoke("AddUserToGroup", "gameMember").catch(function (error)
    {
        return console.error(error.toString());
    });
    var user = $('#nameInputJoin').val();
    var gameId = $('#codeInputJoin').val();
    connection.invoke("JoinGame", user, gameId).catch(function (err) {
        return alert("User already exists");
    });
    event.preventDefault();
});

connection.on("LoadUsersToKill", function (users)
{
  setTimeout(function () {
    var targetDiv = $('#mafiaGame');
    targetDiv.load("/Home/UsersToKill", function (responseTxt, statusTxt, xhr)
    {
        if (statusTxt == "success") {
            createButtons(users, "mafia");
        }
        if(statusTxt == "error") {
            alert("Error: " + xhr.status + ": " + xhr.statusText);
        }
    });
  }, 5000);
});

connection.on("EveryoneKillChoice", function (users)
{
  setTimeout(function () {
    var targetDiv = $('#mafiaGame');
    targetDiv.load("/Home/UsersToKill", function (responseTxt, statusTxt, xhr)
    {
        if (statusTxt == "success") {
            createButtons(users, "villager");
        }
        if(statusTxt == "error") {
            alert("Error: " + xhr.status + ": " + xhr.statusText);
        }
    });
  }, 10000);
});

function createButtons(users, role) {
    users.forEach(function (element) {
        var br = document.createElement("br");
        var button = document.createElement("BUTTON");
        var t = document.createTextNode(element.name);
        button.appendChild(t);
        button.classList.add("btn");
        button.classList.add("btn-outline-danger");
        button.onclick = function () { killPerson(element.name, role); };
        document.getElementById("userList").appendChild(button);
        document.getElementById("userList").appendChild(br);
    });
}

function capitalize(string) {
    return string.charAt(0).toUpperCase() + string.slice(1);
}

function GetNextPage(HomeControllerMethod) {
    var targetDiv = $('#mafiaGame');    
    targetDiv.load(HomeControllerMethod, function () {
    });
}

document.getElementById("newGameStartBtn").addEventListener("click", function (event) {
    connection.invoke("AddUserToGroup", "gameOwner").catch(function (error)
    {
        return console.error(error.toString());
    });
    var user = document.getElementById("nameInputStart").value;
    connection.invoke("StartGame", user).catch(function (err) {
        return alert("User already exists");
    });
    event.preventDefault();
});

// Adds dead user connection ID to a group
connection.on("UpdateVictimGroup", function (connectionId)
{
  connection.invoke("AddUserByIdToGroup", "lastVictim", connectionId).catch(function (error)
    {
        return console.error(error.toString());
    });
});

connection.on("YouDiedPage", function ()
{
    setTimeout(function () {
        GetNextPage("/Home/YouDiedScreen");
    }, 10000);
});

connection.on("DeleteVictimGroup", function (connectionId)
{
  connection.invoke("RemoveUserByIdFromGroup", "lastVictim", connectionId).catch(function (error)
    {
        return console.error(error.toString());
    });
});