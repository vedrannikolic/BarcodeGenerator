using Microsoft.AspNetCore.SignalR;
using QRCodeGen.Services;

namespace QRCodeGenerator.Hubs
{
    public class AuthenticationHub:Hub
    {
        public AuthenticationHub()
        {
        }

        public async Task JoinGroup(string guid)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, guid);
        }

        public async Task LeaveGroup(string guid)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, guid);
        }
    }
}
