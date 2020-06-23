﻿using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace HES.Core.Hubs
{
    public class WorkstationsHub : Hub
    {
        public async Task UpdatePage(string connectionId)
        {
            await Clients.All.SendAsync("PageUpdated", connectionId);
        }        
    }
}
