﻿using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GlacierByte.Discord.Plugin
{
    internal class TvRoom
    {
        private readonly SocketUser Owner;
        public readonly IVoiceChannel Channel;
        private readonly Dictionary<ulong, SocketUser> Waiting;
        private readonly Dictionary<ulong, SocketUser> Allowed;
        private readonly DateTime Started;
        public TvRoom(SocketUser owner, IVoiceChannel channel)
        {
            Owner = owner;
            Channel = channel;
            Waiting = new Dictionary<ulong, SocketUser>();
            Allowed = new Dictionary<ulong, SocketUser>();
            Started = DateTime.Now;
        }

        public async Task RemoveRoom()
        {
            await Channel.DeleteAsync();
        }

        public async Task NewUser(SocketUser newUser)
        {
            if (newUser.Id == Owner.Id || (Allowed.ContainsKey(newUser.Id)))
            {
                return;
            } else if (DateTime.Now.Subtract(Started).TotalSeconds < 30)
            {
                await AllowUser(newUser);
            } else
            {
                await NotAllowUser(newUser);
            }
        }

        private async Task NotAllowUser(SocketUser newUser)
        {
            if (Allowed.ContainsKey(newUser.Id))
            {
                Allowed.Remove(newUser.Id);
            }
            if (!Waiting.ContainsKey(newUser.Id))
            {
                Waiting.Add(newUser.Id, newUser);
            }
            await (newUser as IGuildUser)?.ModifyAsync(x =>
            {
                x.Mute = true;
            });
        }

        private Task AllowUser(SocketUser newUser)
        {
            if (Waiting.ContainsKey(newUser.Id))
            {
                Waiting.Remove(newUser.Id);
            }
            Allowed.TryAdd(newUser.Id, newUser);
            return Task.CompletedTask;
        }
        public async Task UpdateUser(SocketUser socketUser)
        {
            if(Waiting.ContainsKey(socketUser.Id) && !(socketUser as SocketGuildUser)?.IsMuted == true)
            {
                await AllowUser(socketUser);
            } else if (Allowed.ContainsKey(socketUser.Id) && (socketUser as SocketGuildUser)?.IsMuted == true)
            {
                await NotAllowUser(socketUser);
            }
        }
    }
}
