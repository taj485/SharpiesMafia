"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/mafiaHub").build();

connection.on("StartPageUserList", function (users)
{
    var targetDiv = $('#mafiaGame');
 
    targetDiv.load("/Home/StartGameScreen"); //dollar
    users.forEach(function(element) {
        var li = document.createElement("li");
        li.textContent = element.name;
        document.getElementById("userList").appendChild(li);
    })

});

connection.start().then(function(){
    document.getElementById("newGameStartBtn").disabled = false;
}).catch(function (err) {
    return console.error(err.toString());
});

document.getElementById("newGameStartBtn").addEventListener("click", function (event) {
    var user = document.getElementById("nameInputStart").value;
    connection.invoke("StartGame", user).catch(function (err) {
        return console.error(err.toString());
    });

    //'event prevent default' stops user being added to db
    event.preventDefault();
});



