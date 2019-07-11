"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/mafiaHub").build();

connection.on("StartPageUserList", function (user, code) {
    var encodedMsg = user;
    var li = document.createElement("li");
    li.textContent = encodedMsg;
    document.getElementById("userList").appendChild(li);
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

document.getElementById("testButton").addEventListener("click", function (event) {
    var $testDiv = $('#mafiaGame'), url = $(this).data("url");
    console.log($testDiv);
    console.log(url);
    $.get(url, function (data) {
        $testDiv.replaceWith(data);
    });
});



