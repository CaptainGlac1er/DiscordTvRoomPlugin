using Discord.Commands;
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
            await Service.MakeTvRoomAsync(Context);
            await Context.Message.DeleteAsync();
        }
        [Command("shutdown")]
        public async Task ShutDownTvRooms()
        {
            await Service.CloseTvRooms(Context);
            await Context.Message.DeleteAsync();
        }
    }
}
