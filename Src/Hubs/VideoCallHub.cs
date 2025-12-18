namespace SinaMN75U.Hubs;

using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

public class SignalRHub : Hub {
	// Method to send a message to a specific client
	public async Task SendMessageToClient(string connectionId, string type, string payload) {
		await Clients.Client(connectionId).SendAsync("ReceiveMessage", type, payload);
	}

	// Method to broadcast a message to all clients in a group (e.g., a call room)
	public async Task SendMessageToGroup(string groupName, string type, string payload) {
		await Clients.Group(groupName).SendAsync("ReceiveMessage", type, payload);
	}

	// Method for a client to join a call room
	public async Task JoinCall(string userId, string callId) {
		await Groups.AddToGroupAsync(Context.ConnectionId, callId);
		// Optionally, notify others in the group that a new user joined
		await Clients.Group(callId).SendAsync("UserJoined", userId);
		await Clients.Caller.SendAsync("JoinedCall", callId); // Confirm caller joined
	}

	// Method for a client to leave a call room
	public async Task LeaveCall(string userId, string callId) {
		await Groups.RemoveFromGroupAsync(Context.ConnectionId, callId);
		await Clients.Group(callId).SendAsync("UserLeft", userId);
	}

	// Override OnConnectedAsync to potentially track connected users if needed
	public override async Task OnConnectedAsync() {
		await base.OnConnectedAsync();
		// You could store connectionId associated with userId here if needed for direct messages
	}

	// Override OnDisconnectedAsync to handle client disconnections
	public override async Task OnDisconnectedAsync(Exception exception) {
		// You might want to notify others if a user leaves unexpectedly
		// This requires mapping ConnectionId to UserId and CallId, which is more complex
		await base.OnDisconnectedAsync(exception);
	}
}