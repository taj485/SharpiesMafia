"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/mafiaHub").build();

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

connection.on("StartPageUserList", function (users, gameId) {     var targetDiv = $('#mafiaGame');
    targetDiv.load("/Home/StartGameScreen", function (responseTxt, statusTxt, xhr)
    {
        if (statusTxt == "success")

            $("#gameId").html("Join Code: " + gameId[0]);

            users.forEach(function (element) {
               var li = document.createElement("li");
               li.textContent = element.name;
               document.getElementById("userList").appendChild(li)
            });
        if(statusTxt === "error")
            alert("Error: " + xhr.status + ": " + xhr.statusText);
    });
});

connection.on("JoinPageUserList", function (users)
{
    var targetDiv = $('#mafiaGame');
    targetDiv.load("/Home/JoinGameScreen", function (responseTxt, statusTxt, xhr)
    {
        if (statusTxt == "success")
            users.forEach(function (element) {
                var li = document.createElement("li");
                li.textContent = element.name;
                document.getElementById("joinUserList").appendChild(li)
            });
        if(statusTxt == "error")
            alert("Error: " + xhr.status + ": " + xhr.statusText);
    });
});

connection.start().then(function(){
    document.getElementById("newGameStartBtn").disabled = false;
}).catch(function (err) {
    return console.error(err.toString());
});

document.getElementById("newGameStartBtn").addEventListener("click", function (event) {
    connection.invoke("AddUserToGroup", "gameOwner").catch(function (error)
    {
        return console.error(error.toString());
    });
    var user = document.getElementById("nameInputStart").value;
    connection.invoke("StartGame", user).catch(function (err) {
        return console.error(err.toString());
    });
    event.preventDefault();
});


$('#joinGameBtn').on("click", function () {
    connection.invoke("AddUserToGroup", "gameMember").catch(function (error)
    {
        return console.error(error.toString());
    });
    var user = $('#nameInputJoin').val();
    var gameId = $('#codeInputJoin').val();
    connection.invoke("JoinGame", user, gameId).catch(function (err) {
              return console.error(err.toString());
    });
    event.preventDefault();
});

connection.on("LoadUsersToKill", function (users)
{
    var targetDiv = $('#mafiaGame');
    targetDiv.load("/Home/UsersToKill", function (responseTxt, statusTxt, xhr)
    {
        if (statusTxt == "success")
            users.forEach(function (element) {
                var br = document.createElement("br");
                var button = document.createElement("BUTTON");
                var t = document.createTextNode(element.name);
                button.appendChild(t);
                button.classList.add("btn")
                button.classList.add("btn-outline-danger")
                button.onclick = function () { killPerson(element.name); };
                document.getElementById("userList").appendChild(button)
                document.getElementById("userList").appendChild(br)
            });
        if(statusTxt == "error")
            alert("Error: " + xhr.status + ": " + xhr.statusText);
 
    });
});


// Need to hook up to the timer rather than a test button.
document.getElementById("TestButton").addEventListener("click", function (event) {
    connection.invoke("ListUsersToKill").catch(function (err) {
        return console.error(err.toString());
    });
    event.preventDefault();
});


function killPerson(user){
    connection.invoke("KillPlayer", user).catch(function (err) {
        return console.error(err.toString());
    });
}

connection.on("LoadNight", function ()
{
    var targetDiv = $('#mafiaGame');
    targetDiv.load("/Home/LoadNightScreen");
});
 function GetNextPage(HomeControllerMethod) {     var targetDiv = $('#mafiaGame');     targetDiv.load(HomeControllerMethod, function () {     }); }

