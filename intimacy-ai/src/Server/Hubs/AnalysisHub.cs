using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace IntimacyAI.Server.Hubs
{
    public sealed class AnalysisHub : Hub
    {
        public Task JoinSession(string sessionId)
        {
            return Groups.AddToGroupAsync(Context.ConnectionId, sessionId);
        }

        public Task LeaveSession(string sessionId)
        {
            return Groups.RemoveFromGroupAsync(Context.ConnectionId, sessionId);
        }
    }
}

