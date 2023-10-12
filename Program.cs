using System.Security.Cryptography.X509Certificates;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Sora;
using Sora.Entities;
using Sora.Entities.Info;
using Sora.Entities.Segment;
using Sora.Entities.Segment.DataModel;
using Sora.Enumeration.EventParamsType;
using Sora.Interfaces;
using Sora.Net.Config;
using Sora.Util;
using Tqq;
using YukariToolBox.LightLog;

try
{


    // 设置log等级
    Log.LogConfiguration
       .EnableConsoleOutput()
       .SetLogLevel(LogLevel.Debug);
    var serviceProvider = new ServiceCollection().AddHttpClient().BuildServiceProvider();
    var httpClientFactory = serviceProvider.GetService<IHttpClientFactory>();
    var keyWord = Configuration.GetConfiguration("keyWord");
    async Task SendEmailForSay(string emailTo, int type, string qqNum, string qqName, string groupNum, string groupName, string remark)
    {

        var nWs = "收|卖|出|藏|空投".Split("|");
        var flag = false;
        foreach (var item in nWs)
        {
            if (remark.Contains(item))
            {
                flag = true;
            }
        }
        if (flag)
        {
            var emailModel = new SendModel($"{emailTo}@qq.com", type, qqNum, qqName, groupNum, groupName, remark, qqName);
            var json = JsonConvert.SerializeObject(emailModel);
            var data = new StringContent(json, Encoding.UTF8, "application/json");
            var url = "https://haowanhuaijiuapi.kumili.net/api/Yjj/SendEmailForSay";
            using var client = httpClientFactory?.CreateClient();
            if (client != null)
            {
                client.Timeout = TimeSpan.FromMinutes(3);
                await client.PostAsync(url, data);
                Log.Debug("Email|SendEmailForSay===>", "ok");
            }
            else
            {
                await Task.Run(() =>
                {
                    Log.Debug("Email|SendEmailForSay===>", "client is null");
                });
            }
        }
    }

    async Task SendEmailForChange(string emailTo, int type, string qqNum, string qqName, string groupNum, string groupName, string remark)
    {
        var emailModel = new SendModel($"{emailTo}@qq.com", type, qqNum, qqName, groupNum, groupName, remark, qqName);
        var json = JsonConvert.SerializeObject(emailModel);
        var data = new StringContent(json, Encoding.UTF8, "application/json");
        var url = "https://haowanhuaijiuapi.kumili.net/api/Yjj/SendEmailForChange";
        using var client = httpClientFactory?.CreateClient();
        if (client != null)
        {
            client.Timeout = TimeSpan.FromMinutes(3);
            await client.PostAsync(url, data);
            Log.Debug("Email|SendEmailForChange===>", "ok");
        }
        else
        {
            await Task.Run(() =>
            {
                Log.Debug("Email|SendEmailForChange===>", "client is null");
            });
        }
    }

    //实例化Sora服务
    ISoraService service = SoraServiceFactory.CreateService(new ServerConfig
    {
        EnableSocketMessage = false,
        ThrowCommandException = false,
        Port = 9200
    });

    #region 事件处理

    //连接事件
    service.ConnManager.OnOpenConnectionAsync += (connectionId, eventArgs) =>
    {
        Log.Debug("Sora_Test|OnOpenConnectionAsync",
            $"connectionId = {connectionId} type = {eventArgs.Role}");
        return ValueTask.CompletedTask;
    };
    //连接关闭事件
    service.ConnManager.OnCloseConnectionAsync += (connectionId, eventArgs) =>
    {
        Log.Debug("Sora_Test|OnCloseConnectionAsync",
            $"uid = {eventArgs.SelfId} connectionId = {connectionId} type = {eventArgs.Role}");
        return ValueTask.CompletedTask;
    };
    //连接成功元事件
    service.Event.OnClientConnect += (_, eventArgs) =>
    {
        Log.Debug("Sora_Test|OnClientConnect",
            $"uid = {eventArgs.LoginUid}");
        return ValueTask.CompletedTask;
    };

    //群聊消息事件
    service.Event.OnGroupMessage += async (_, eventArgs) =>
    {
        var msg = eventArgs.Message.RawText;
        var qqNum = eventArgs.Sender.Id;
        var userInfo = (await eventArgs.Sender.SoraApi.GetUserInfo(qqNum)).userInfo;
        var groupNum = eventArgs.SourceGroup.Id;
        var groupInfo = (await eventArgs.SourceGroup.SoraApi.GetGroupInfo(groupNum)).groupInfo;
        Log.Debug("群聊=>", $"昵称:{userInfo.Nick} | qq号:{qqNum} | 群号:{groupNum} | 群名:{groupInfo.GroupName} | 说:{msg}");
        // await SendEmailForSay(qqNum.ToString(), 1, qqNum.ToString(), userInfo.Nick, groupNum.ToString(), groupInfo.GroupName, msg);
        if (msg == "ok")
        {
            // await eventArgs.SoraApi.SendTemporaryMessage(qqNum, groupNum, $"欢迎👏🏻👏🏻👏🏻 {userInfo.Nick} 加入「极速数藏」有问题找管理 请勿轻易相信任何人 以防被骗 💦");
            // await eventArgs.SoraApi.SendPrivateMessage(qqNum, $"私聊欢迎👏🏻👏🏻👏🏻 {userInfo.Nick} 加入「极速数藏」有问题找管理 请勿轻易相信任何人 以防被骗 💦");
            await eventArgs.Sender.SendPrivateMessage("^唇*`*动^与你长相伴^^让你人生更精彩 😍😘 佺國每个城市都有丄門安全 ^6哖老荟所今晚做我的男人&&向前一步 🤗 来吧 😜 ##资源^领取：（加小妹QQ领取  2048139546） 25岁以下勿扰~~~🤥");
            #region 转发
            var customNodes = new List<CustomNode>();
            customNodes.Add(new CustomNode(userInfo.Nick, qqNum, SoraSegment.Record($"/Users/topbrids/qq/Tqq/images/1.mp3")));
            customNodes.Add(new CustomNode(userInfo.Nick, qqNum, "^唇*`*动^与你长相伴^^让你人生更精彩 😍😘 佺國每个城市都有丄門安全 ^6哖老荟所今晚做我的男人&&向前一步 🤗 来吧 😜 ##资源^领取：（加小妹QQ领取  2048139546） 25岁以下勿扰~~~🤥"));

            customNodes.Add(new CustomNode(userInfo.Nick, qqNum, SoraSegment.Record($"/Users/topbrids/qq/Tqq/images/2.mp3")));
            customNodes.Add(new CustomNode(userInfo.Nick, qqNum, SoraSegment.Image($"/Users/topbrids/qq/Tqq/images/mv.png")));

            await eventArgs.SourceGroup.SendGroupForwardMsg(customNodes);
            #endregion
            await eventArgs.SourceGroup.SendGroupMessage(SoraSegment.Record($"/Users/topbrids/qq/Tqq/images/1.mp3"));
            await eventArgs.SourceGroup.SendGroupMessage("^唇*`*动^与你长相伴^^让你人生更精彩 😍😘 佺國每个城市都有丄門安全 ^6哖老荟所今晚做我的男人&&向前一步 🤗 来吧 😜 ##资源^领取：（加小妹QQ领取  2048139546） 25岁以下勿扰~~~🤥");
            await eventArgs.SourceGroup.SendGroupMessage(SoraSegment.Record($"/Users/topbrids/qq/Tqq/images/2.mp3"));
            await eventArgs.SourceGroup.SendGroupMessage(SoraSegment.Image($"/Users/topbrids/qq/Tqq/images/mv.png"));
        }


    };
    service.Event.OnSelfGroupMessage += (_, eventArgs) =>
    {
        Log.Warning("test", $"self group msg {eventArgs.Message.MessageId}[{eventArgs.IsSelfMessage}]");
        return ValueTask.CompletedTask;
    };
    //私聊消息事件
    service.Event.OnPrivateMessage += async (_, eventArgs) =>
    {
        var senderInfo = eventArgs.SenderInfo;
        await eventArgs.Reply($"嗯 {senderInfo.Nick} 现在有事 晚点详聊.. 💦");
    };
    //成员变动
    service.Event.OnGroupMemberChange += async (_, eventArgs) =>
    {
        var qqNum = eventArgs.ChangedUser.Id;
        var groupNum = eventArgs.SourceGroup.Id;
        var action = eventArgs.SubType == MemberChangeType.Approve ? "管理员同意入群" : eventArgs.SubType == MemberChangeType.Leave ? "主动退群" : $"{MemberChangeType.Approve.ToString()}";
        if (!string.IsNullOrWhiteSpace(action))
        {
            var userInfo = (await eventArgs.SoraApi.GetUserInfo(qqNum)).userInfo;
            var groupInfo = (await eventArgs.SoraApi.GetGroupInfo(groupNum)).groupInfo;
            Log.Debug("成员变动=>", $"昵称 | {userInfo.Nick} | qq号:{qqNum} | {action} | 群号:{groupNum} | 群名:{groupInfo.GroupName}");
            // await SendEmailForChange(qqNum.ToString(), 1, qqNum.ToString(), userInfo.Nick, groupNum.ToString(), groupInfo.GroupName, $"昵称 | {userInfo.Nick} | qq号:{qqNum} | {action} | 群号:{groupNum} | 群名:{groupInfo.GroupName}");
            if (groupNum.ToString() == "389550509" || groupNum.ToString() == "750556258" && eventArgs.SubType == MemberChangeType.Approve)
            {
                await eventArgs.SourceGroup.SendGroupMessage($"欢迎👏🏻👏🏻👏🏻 {userInfo.Nick} 加入「极速数藏」有问题找管理 请勿轻易相信任何人 以防被骗 💦");
            }
        }
    };
    // service.Event.OnSelfGroupMessage
    service.Event.OnSelfPrivateMessage += (_, eventArgs) =>
    {
        Log.Warning("test", $"self private msg {eventArgs.Message.MessageId}[{eventArgs.IsSelfMessage}]");
        return ValueTask.CompletedTask;
    };
    // //处理好友添加请求
    // service.Event.OnFriendAdd += async (_, friendAddEventArgs) =>
    // {
    //     await friendAddEventArgs.NewFriend.SoraApi.SetFriendAddRequest($"1", true, $"2");
    //     await friendAddEventArgs.NewFriend.SendPrivateMessage("^唇*`*动^与你长相伴^^让你人生更精彩 😍😘 佺國每个城市都有丄門安全 ^6哖老荟所今晚做我的男人&&向前一步 🤗 来吧 😜 ##资源^领取：（加小妹QQ领取  2048139546） 25岁以下勿扰~~~🤥");
    // };

    //处理好友主动加我请求
    service.Event.OnFriendRequest += async (_, eventArgs) =>
    {
        await eventArgs.Accept();

        await eventArgs.Sender.SendPrivateMessage(SoraSegment.Record($"/Users/topbrids/qq/Tqq/images/1.mp3"));
        await eventArgs.Sender.SendPrivateMessage("^唇*`*动^与你长相伴^^让你人生更精彩 😍😘 佺國每个城市都有丄門安全 ^6哖老荟所今晚做我的男人&&向前一步 🤗 来吧 😜 ##资源^领取：（加小妹QQ领取  2048139546） 25岁以下勿扰~~~🤥");
        await eventArgs.Sender.SendPrivateMessage(SoraSegment.Record($"/Users/topbrids/qq/Tqq/images/2.mp3"));
        await eventArgs.Sender.SendPrivateMessage(SoraSegment.Image($"/Users/topbrids/qq/Tqq/images/mv.png"));
        // friendRequestEventArgs.Sender.SendPrivateMessage("^唇*`*动^与你长相伴^^让你人生更精彩 😍😘 佺國每个城市都有丄門安全 ^6哖老荟所今晚做我的男人&&向前一步 🤗 来吧 😜 ##资源^领取：（加小妹QQ领取  2048139546） 25岁以下勿扰~~~🤥");
        // return ValueTask.CompletedTask;
    };
    //动态向管理器注册指令
    service.Event.CommandManager.RegisterGroupDynamicCommand(
        new[] { "2" },
        async eventArgs =>
        {
            await eventArgs.Reply("shit");
            eventArgs.IsContinueEventChain = false;
        });

    #endregion

    //启动服务并捕捉错误
    await service.StartService()
                 .RunCatch(e => Log.Error("Sora Service", Log.ErrorLogBuilder(e)));

    await Task.Delay(-1);

}
catch (System.Exception ex)
{
    System.Console.WriteLine(ex);
}