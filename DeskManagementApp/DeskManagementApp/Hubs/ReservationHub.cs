using Microsoft.AspNetCore.SignalR;

namespace DeskManagementApp.Hubs
{
    public class ReservationHub : Hub
    {
        public async Task SendMessage(string user, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }
    }
}
