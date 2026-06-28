using Microsoft.AspNetCore.SignalR;
using System.Threading;
using System.Threading.Tasks;
using System;

namespace TourGuideApp.Hubs
{
    public class VisitorHub : Hub
    {
        private static int _visitorCount = 0;

        public async Task JoinAsVisitor()
        {
            Interlocked.Increment(ref _visitorCount);
            Context.Items["IsVisitor"] = true;
            await Clients.All.SendAsync("UpdateVisitorCount", _visitorCount);
        }

        public async Task GetCurrentCount()
        {
            await Clients.Caller.SendAsync("UpdateVisitorCount", _visitorCount);
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            if (Context.Items.TryGetValue("IsVisitor", out var isVisitor) && (bool)isVisitor)
            {
                Interlocked.Decrement(ref _visitorCount);
                await Clients.All.SendAsync("UpdateVisitorCount", _visitorCount);
            }
            await base.OnDisconnectedAsync(exception);
        }
    }
}
