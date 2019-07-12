"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/mafiaHub").build();

connection.on("StartPageUserList", function (users, gameId) {     var targetDiv = $('#mafiaGame');
    console.log("HEY THERE")
            console.log(gameId[0]);
    targetDiv.load("/Home/StartGameScreen", function (responseTxt, statusTxt, xhr)
    {
        if (statusTxt == "success")

        document.getElementById("gameId").innerHtml = gameId[0];

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



