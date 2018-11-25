using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace GlacierByte.Discord.Plugin
{
    [Group("tvroom")]
    public class TvRoomPlugin : ModuleBase
    {
        private readonly TvRoomService Service;
        public TvRoomPlugin(TvRoomService service)
        {
            this.Service = service;
        }
        [Command("init")]
        public async Task InitTvRoom()
        {
            Console.WriteLine("making channel");
            await Service.MakeTvRoomAsync(Context);
            await Context.Message.DeleteAsync();
            Console.WriteLine("done");
        }
    }
}
