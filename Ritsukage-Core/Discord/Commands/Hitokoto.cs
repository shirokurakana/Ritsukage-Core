﻿using Discord.Commands;
using System.Threading.Tasks;

namespace Ritsukage.Discord.Commands
{
    public class Hitokoto : ModuleBase<SocketCommandContext>
    {
        [Command("一言"), Alias("hitokoto")]
        public async Task Normal()
        {
            var msg = await ReplyAsync("``数据检索中……``");
            try
            {
                var h = Tools.Hitokoto.Get();
                await msg.ModifyAsync(x => x.Content = h.ToString());
            }
            catch
            {
                await msg.ModifyAsync(x => x.Content = "一言获取失败，请稍后再试");
            }
        }

        [Command("毒一言"), Alias("anotherhitokoto")]
        public async Task Another()
        {
            var msg = await ReplyAsync("``数据检索中……``");
            try
            {
                var h = Tools.Hitokoto.GetAnother();
                await msg.ModifyAsync(x => x.Content = h);
            }
            catch
            {
                await msg.ModifyAsync(x => x.Content = "毒一言获取失败，请稍后再试");
            }
        }
    }
}
