using Azure.Core;
using GptApi.Data;
using GptApi.DBServices;
using GptApi.Helpers;
using GptApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OpenAI;
using OpenAI.Chat;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;

namespace GptApi.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class GptController : ControllerBase
    {
        private static Dictionary<string, GptResult> _dic = new Dictionary<string, GptResult>();
        private readonly IConfiguration _config;
        private readonly GptContext _context;
        private readonly AuthService _authService;

        public GptController(IConfiguration config, GptContext context, AuthService authService)
        {
            _config = config;
            _context = context;
            _authService = authService;
        }

        [Authorize(Roles = "GptTester")]
        [HttpPost(Name = "BeginGpt")]
        public async Task<BeginReponse> BeginGpt(BeginRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Question))
                    return new BeginReponse(400, "問題不得為空");

                var key = Guid.NewGuid().ToString();
                var response = new BeginReponse(200, "success", key);

                var claims = HttpContext.User.Claims;
                var name = claims.FirstOrDefault(x => x.Type == ClaimTypes.Name)?.Value;
                var userid = claims.FirstOrDefault(x => x.Type == "UserId")?.Value;
                var user = await _authService.GetUser(int.Parse(userid));
                // _context.UserInfos.FirstOrDefault(x => x.userid == int.Parse(userid) && x.username == name );
                if (user?.present_cnt >= user.have_cnt)
                    return new BeginReponse(400, "你的提問已經超過帳號可提問次數");
                UserChatCnt ucc = new UserChatCnt
                {
                    userid = user.userid,
                    present_cnt = user.present_cnt + 1,
                    have_cnt = user.have_cnt,
                };
                _context.UserChatCnts.Update(ucc);
                _context.SaveChanges();
                DesCrytoHelper.TryDesDecrypt(_config["GptConstant:apikey"], _config["Des:k"], _config["Des:iv"], out string gptkey);

                var client = new ChatClient
                    (_config["GptConstant:modelname"], gptkey);

                _dic[key] = new GptResult { IsComplete = false, Body = "" };
                bool flag = false;
                var stream = client.CompleteChatStreamingAsync(request.Question);
                Task.Run(async () =>
                {
                    await foreach (var update in stream)
                    {
                        flag = true;
                        foreach (var content in update.ContentUpdate)
                        {
                            _dic[key].Body += content.Text;
                        }
                    }
                    _dic[key].IsComplete = true;
                }).ContinueWith((task) =>
                {
                    if (!task.IsFaulted)
                        return;
                    _dic.Remove(key);
                    response = new BeginReponse(400, task.Exception?.InnerException?.Message ?? "GPT系統故障");
                    flag = true;
                });

                while (!flag)
                    Thread.Sleep(5);
                return response;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        [HttpPost(Name = "GetMessage")]
        public GetMessageReponse GetMessage(GetMessageRequest request)
        {
            if (string.IsNullOrEmpty(request.key) || !_dic.ContainsKey(request.key))
                return new GetMessageReponse { StatusCode = 400, Message = "key值有問題" };

            var gptResult = _dic[request.key];
            var response = new GetMessageReponse { StatusCode = 200, Message = "success", IsComplete = gptResult.IsComplete, Body = gptResult.Body };
            return response;
        }

        [HttpGet(Name = "RemoveDictionary")]
        public RemoveDictionaryResponse RemoveDictionary(string key)
        {
            if (string.IsNullOrEmpty(key) || !_dic.ContainsKey(key))
                return new RemoveDictionaryResponse { StatusCode = 400, Message = "key值有問題" };
            _dic.Remove(key);
            return new RemoveDictionaryResponse { StatusCode = 200, Message = "移除成功" };
        }
    }
}
