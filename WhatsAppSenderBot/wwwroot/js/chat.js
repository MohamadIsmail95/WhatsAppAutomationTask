//var transportType = signalR.TransportType.WebSockets;
//can also be ServerSentEvents or LongPolling
//var logger = new signalR.ConsoleLogger(signalR.LogLevel.Information);
var chatConnection = new signalR.HubConnectionBuilder().withUrl("/whatsapp").build();
//var chatConnection = new signalR.HubConnection(chatHub);

chatConnection.onClosed = e => {
    console.log('connection closed');
};



chatConnection.on('Authenticating', (message) => {
    $("#qrCode").show();
    $('#qrCode').attr("src", message);
    $("#chats").hide();

});
chatConnection.on('Authenticated', () => {
    $("#qrCode").hide();
    $("#chats").show();

});
chatConnection.on('UI', (ui) => {

    $("#ui").attr("src", "whatsapp/ui?" + new Date().getTime());

});
chatConnection.on('Started', () => {

    $("#Status").html("Running...");

});
chatConnection.on('Stopped', () => {

    $("#Status").html("Stopped");

});
chatConnection.on('MessagesReceived', (message) => {

    for (var i = 0; i < message.messages.length; i++) {
        $("#chats").append("<li>" + message.messages[i].contact.name + " (" + message.messages[i].contact.id + "): " + message.messages[i].message + "</li>");
    }

});

chatConnection.start().catch(err => {
    console.log('connection error');
});


//function send(message) {
//    chatConnection.invoke('Send', message);

//}