using Microsoft.AspNetCore.SignalR;
using System.Threading;
using System.Threading.Tasks;
using System;

namespace TourGuideApp.Hubs
{
    public class VisitorHub : Hub
    {
        private static int _visitorCount = 0;

        public async Task JoinAsVisitor(int multiplier = 1)
        {
            Interlocked.Add(ref _visitorCount, multiplier);
            Context.Items["IsVisitor"] = true;
            Context.Items["Multiplier"] = multiplier;
            await Clients.All.SendAsync("UpdateVisitorCount", _visitorCount);
        }

        public async Task GetCurrentCount()
        {
            await Clients.Caller.SendAsync("UpdateVisitorCount", _visitorCount);
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            if (Context.Items.TryGetValue("IsVisitor", out var isVisitor) && isVisitor is true)
            {
                int multiplier = Context.Items.TryGetValue("Multiplier", out var m) ? (int)m : 1;
                Interlocked.Add(ref _visitorCount, -multiplier);
                await Clients.All.SendAsync("UpdateVisitorCount", _visitorCount);
            }
            await base.OnDisconnectedAsync(exception);
        }
    }
}
