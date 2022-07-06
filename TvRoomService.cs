using Discord;
using Discord.Commands;
using Discord.WebSocket;
using GlacierByte.Discord.Server.Api;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GlacierByte.Discord.Plugin
{
    public class TvRoomService : ICustomService
    {
        private readonly Dictionary<ulong, IVoiceChannel> TvRoomLobbies;
        private readonly Dictionary<ulong, TvRoom> TvRooms;
        private readonly DiscordSocketClient Client;
        public TvRoomService(DiscordSocketClient client)
        {
            Client = client;
            TvRoomLobbies = new Dictionary<ulong, IVoiceChannel>();
            TvRooms = new Dictionary<ulong, TvRoom>();
            Client.UserVoiceStateUpdated += ProcessVoiceChannelChange;
            Console.WriteLine("TvRoomService Started");
        }

        public async Task ProcessVoiceChannelChange(SocketUser user, SocketVoiceState oldChannel, SocketVoiceState newChannel)
        {
            if (newChannel.VoiceChannel?.Id == oldChannel.VoiceChannel?.Id)
            {
                if (TvRooms.ContainsKey(newChannel.VoiceChannel.Id))
                {
                    await TvRooms[newChannel.VoiceChannel.Id].UpdateUser(user);
                }
            }
            else
            {
                if(newChannel.VoiceChannel != null)
                {
                    if (TvRooms.ContainsKey(newChannel.VoiceChannel.Id))
                    {
                        await TvRooms[newChannel.VoiceChannel.Id].NewUser(user);
                    } else if (TvRoomLobbies.ContainsKey(newChannel.VoiceChannel.Id))
                    {
                        var newTvRoom = await newChannel.VoiceChannel.Guild.CreateVoiceChannelAsync($"{user.Username} - TvRoom");
                        TvRooms.Add(newTvRoom.Id, new TvRoom(user, newTvRoom));
                        await (user as IGuildUser)?.ModifyAsync(x =>
                        {
                            x.Channel = newTvRoom;
                        });
                        await newTvRoom.ModifyAsync(x =>
                        {
                            x.Position = newChannel.VoiceChannel.Position;
                        });
                    }
                }
                if(oldChannel.VoiceChannel != null)
                {
                    if (TvRooms.ContainsKey(oldChannel.VoiceChannel.Id))
                    {
                        if (oldChannel.VoiceChannel.Users.Count == 0)
                        {
                            await TvRooms[oldChannel.VoiceChannel.Id].RemoveRoom();
                        }
                    }
                }
            }
        }
        public async Task MakeTvRoomAsync(ICommandContext context)
        {
            using (context.Channel.EnterTypingState())
            {
                var voiceChannels = await context.Guild.GetVoiceChannelsAsync();
                var channelFound = false;
                foreach (var channel in voiceChannels)
                {
                    if (channel.Name.Equals("StartTvRoom"))
                    {
                        TvRoomLobbies.Add(channel.Id, channel);
                        channelFound = true;
                        break;
                    }
                }
                if (!channelFound)
                {
                    var positions = (await context.Guild.GetChannelsAsync());
                    var highestChannel = 0;
                    foreach (var numPosition in positions)
                    {
                        if (numPosition.Position > highestChannel)
                        {
                            highestChannel = numPosition.Position;
                        }
                    }
                    var channel = await context.Guild.CreateVoiceChannelAsync($"StartTvRoom");
                    await channel.ModifyAsync(x =>
                    {
                        x.Position = highestChannel;
                    });
                    TvRoomLobbies.Add(channel.Id, channel);
                }
            }
        }
        public async Task CloseTvRooms(ICommandContext context)
        {
            var channelVoiceChannels = await context.Guild.GetVoiceChannelsAsync();
            using (context.Channel.EnterTypingState())
            {
                foreach (var room in channelVoiceChannels)
                {
                    if (TvRooms.ContainsKey(room.Id))
                    {
                        await TvRooms[room.Id].RemoveRoom();
                        TvRooms.Remove(room.Id);
                    } else if (TvRoomLobbies.ContainsKey(room.Id))
                    {
                        TvRoomLobbies.Remove(room.Id);
                        await room.DeleteAsync();
                    }
                }
            }
        }
    }
}
