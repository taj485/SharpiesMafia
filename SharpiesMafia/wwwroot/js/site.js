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


connection.on("LoadUsersToKill", function (users)
{
    var targetDiv = $('#mafiaGame');
    targetDiv.load("/Home/UsersToKill", function (responseTxt, statusTxt, xhr)
    {
        if (statusTxt == "success")
            users.forEach(function (element) {
                var button = document.createElement("BUTTON");
                var t = document.createTextNode(element.name);
                button.appendChild(t);
                document.getElementById("userList").appendChild(button)
            });
        if(statusTxt == "error")
            alert("Error: " + xhr.status + ": " + xhr.statusText);
 
    });
});



document.getElementById("TestButton").addEventListener("click", function (event) {
    connection.invoke("ListUsersToKill").catch(function (err) {
        return console.error(err.toString());
    });
    event.preventDefault();
});


