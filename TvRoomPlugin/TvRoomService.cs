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
            Console.WriteLine($"new: {newChannel} old: {oldChannel}");
            Console.WriteLine($"new: {newChannel.VoiceChannel?.Id} old: {oldChannel.VoiceChannel?.Id}");

            if (newChannel.VoiceChannel != null && oldChannel.VoiceChannel != null && newChannel.VoiceChannel.Id == oldChannel.VoiceChannel.Id)
            {
                if (TvRooms.ContainsKey(newChannel.VoiceChannel.Id))
                {
                    await TvRooms[newChannel.VoiceChannel.Id].UpdateUser(user);
                }
            }
            else
            {
                if (newChannel.VoiceChannel != null && TvRooms.ContainsKey(newChannel.VoiceChannel.Id))
                {
                    Console.WriteLine($"added new user: {user.Username}");
                    await TvRooms[newChannel.VoiceChannel.Id].NewUser(user);
                }
                if (newChannel.VoiceChannel != null && TvRoomLobbies.ContainsKey(newChannel.VoiceChannel.Id))
                {
                    var newTvRoom = await newChannel.VoiceChannel.Guild.CreateVoiceChannelAsync($"{user.Username} - TvRoom");
                    TvRooms.Add(newTvRoom.Id, new TvRoom(user, newTvRoom));
                    Console.WriteLine($"added channel {newTvRoom.Id}");
                    await (user as IGuildUser)?.ModifyAsync(x =>
                    {
                        x.Channel = newTvRoom;
                    });
                    Console.WriteLine($"Made {newTvRoom.Name} Channel at position {newTvRoom.Position} suppose to be at {newChannel.VoiceChannel.Position}");
                    await newTvRoom.ModifyAsync(x =>
                    {
                        x.Position = newChannel.VoiceChannel.Position;
                    });
                }
                if (oldChannel.VoiceChannel != null && TvRooms.ContainsKey(oldChannel.VoiceChannel.Id))
                {
                    Console.WriteLine(oldChannel.VoiceChannel.Users.Count);
                    if (oldChannel.VoiceChannel.Users.Count == 0)
                    {
                        await TvRooms[oldChannel.VoiceChannel.Id].RemoveRoom();
                    }
                }
            }
        }
        public async Task MakeTvRoomAsync(ICommandContext context)
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
                var position = 0;
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
                Console.WriteLine($"Made {channel.Name} Channel at position {channel.Position} suppose to be at {position-1}");
            }
        }
    }
}
