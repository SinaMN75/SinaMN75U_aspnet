namespace SinaMN75U.Hubs;

using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

public class CallHub : Hub
{
	public async Task SendOffer(string userId, object offer)
		=> await Clients.User(userId).SendAsync("ReceiveOffer", offer);

	public async Task SendAnswer(string userId, object answer)
		=> await Clients.User(userId).SendAsync("ReceiveAnswer", answer);

	public async Task SendIce(string userId, object candidate)
		=> await Clients.User(userId).SendAsync("ReceiveIce", candidate);
}