﻿using Newtonsoft.Json.Linq;
using Ritsukage.Library.Bilibili.Model;
using Ritsukage.Library.Data;
using Ritsukage.Library.Subscribe.CheckResult;
using Ritsukage.Tools.Console;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Ritsukage.Library.Subscribe.CheckMethod
{
    public class BilibiliLiveCheckMethod : Base.SubscribeCheckMethod
    {
        const string type = "bilibili live";

        public int RoomId { get; init; }

        public BilibiliLiveCheckMethod(int roomid)
        {
            RoomId = roomid;
        }

        public override async Task<CheckResult.Base.SubscribeCheckResult> Check()
        {
            LiveRoom room = null;
            User user = null;
            try
            {
                room = LiveRoom.Get(RoomId);
            }
            catch (Exception e)
            {
                ConsoleLog.Error("Bilibili Live Checker", ConsoleLog.ErrorLogBuilder(e));
                return new BilibiliLiveCheckResult();
            }
            try
            {
                user = room?.GetUserInfo();
            }
            catch (Exception e)
            {
                ConsoleLog.Error("Bilibili Live Checker", ConsoleLog.ErrorLogBuilder(e));
                return new BilibiliLiveCheckResult();
            }
            var t = await Database.Data.Table<SubscribeStatusRecord>().ToListAsync();
            if (t != null && t.Count > 0)
            {
                var record = t.Where(x => x.Type == type && x.Target == RoomId.ToString()).FirstOrDefault();
                if (record != null)
                {
                    JObject status = null;
                    try
                    {
                        status = JObject.Parse(record.Status);
                    }
                    catch (Exception e)
                    {
                        ConsoleLog.Error("Bilibili Live Checker", ConsoleLog.ErrorLogBuilder(e));
                    }
                    if (status != null)
                    {
                        BilibiliLiveUpdateType updated = BilibiliLiveUpdateType.None;
                        if ((int)status["LiveStatus"] != (int)room.LiveStatus)
                        {
                            updated = BilibiliLiveUpdateType.LiveStatus;
                            status["LiveStatus"] = (int)room.LiveStatus;
                            status["Title"] = room.Title;
                        }
                        else if (room.LiveStatus == LiveStatus.Live && (string)status["Title"] != room.Title)
                        {
                            updated = BilibiliLiveUpdateType.Title;
                            status["Title"] = room.Title;
                        }
                        if (updated != BilibiliLiveUpdateType.None)
                        {
                            record.Status = status.ToString();
                            await Database.Data.UpdateAsync(record);
                            return new BilibiliLiveCheckResult()
                            {
                                Updated = true,
                                UpdateType = updated,
                                RoomId = room.Id,
                                Title = room.Title,
                                User = user.Name,
                                Area = room.ParentAreaName + "·" + room.AreaName,
                                Online = room.Online,
                                Status = room.LiveStatus,
                                Cover = string.IsNullOrWhiteSpace(room.UserCoverUrl) ? room.KeyFrame : room.UserCoverUrl,
                                Url = room.Url
                            };
                        }
                        else
                            return new BilibiliLiveCheckResult();
                    }
                }
            }
            await Database.Data.InsertAsync(new SubscribeStatusRecord()
            {
                Type = type,
                Target = RoomId.ToString(),
                Status = new JObject()
                {
                    { "LiveStatus", (int)room.LiveStatus },
                    { "Title", room.Title }
                }.ToString()
            });
            return new BilibiliLiveCheckResult()
            {
                Updated = true,
                UpdateType = BilibiliLiveUpdateType.Initialization,
                RoomId = room.Id,
                Title = room.Title,
                User = user.Name,
                Area = room.ParentAreaName + "·" + room.AreaName,
                Online = room.Online,
                Status = room.LiveStatus,
                Cover = string.IsNullOrWhiteSpace(room.UserCoverUrl) ? room.KeyFrame : room.UserCoverUrl,
                Url = room.Url
            };
        }
    }
}
