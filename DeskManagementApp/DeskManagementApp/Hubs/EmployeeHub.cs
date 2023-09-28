using Microsoft.AspNetCore.SignalR;

namespace DeskManagementApp.Hubs
{
    public class EmployeeHub : Hub
    {
        public async Task SendMessage(string user, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }
    }
}
