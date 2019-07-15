"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/mafiaHub").build();

connection.on("StartPageUserList", function (users)
{
    var targetDiv = $('#mafiaGame');
    targetDiv.load("/Home/StartGameScreen", function (responseTxt, statusTxt, xhr)
    {
        if (statusTxt == "success")
            users.forEach(function (element) {
                var li = document.createElement("li");
                li.textContent = element.name;
                document.getElementById("userList").appendChild(li)
            });
        if(statusTxt == "error")
            alert("Error: " + xhr.status + ": " + xhr.statusText);
    });
});

connection.on("JoinPageUserList", function (users)
{
    var targetDiv = $('#mafiaGame');
    targetDiv.load("/Home/JoinGameScreen", function (responseTxt, statusTxt, xhr)
    {
        // Kept seperate incase a page does not need a countdown display/timer.
        Countdown(30); // In seconds
        Timer(30000, "/Home/NightTimeScreen"); // In miliseconds

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
    connection.invoke("AddUserToGroup", "gameOwner").catch(function (error)
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

function GetNextPage(pageRoute) {
    var targetDiv = $('#mafiaGame');
    targetDiv.load(pageRoute, function () {
    });
}

function Timer(ms, pageRoute) {
    setTimeout(function () {
            GetNextPage(pageRoute);
    }, ms);
}

function Countdown(time) {
    var start = time;
    var second = 1;

    var x = setInterval(function () {
        var seconds = start - second;
        document.getElementById("countdownContainerP").innerHTML = seconds + "s";
        start = seconds;
        if (seconds < 0) {
            clearInterval(x);
        }
    }, 1000);
}


