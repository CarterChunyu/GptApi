using Dapper;
using GptApi.Data;
using GptApi.DBServices;
using GptApi.Helpers;
using GptApi.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GptApi.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly GptContext _context;
        private readonly AuthService _authService;

        public AuthController(GptContext context, AuthService authService)
        {
            _context = context;
            _authService = authService;
        }

        [HttpPost]
        public async Task<TokenCommonReponse?> GetToken(TokenRequest request)
        {
            TokenCommonReponse? response = null;
            try
            {
                UserInfo userinfo = await _authService.GetUser(request.UserID);
                if (userinfo == null)
                {
                    var ucc = new UserChatCnt { userid = request.UserID, present_cnt = 0, have_cnt = 100 };
                    _context.UserChatCnts.Add(ucc);
                    _context.SaveChanges();
                    userinfo = await _authService.GetUser(request.UserID);
                }
                if (userinfo?.present_cnt >= userinfo?.have_cnt)
                    response = new TokenCommonReponse
                    {
                        StatusCode = 400,
                        Message = userinfo == null ? "查無此帳號" : "聊天次數已大於帳號擁有最高次數"
                    };
                else
                {
                    var role = "GptTester";
                    var token = JwtHelper.GenerateJsonWebToken(userinfo, role);
                    response = new TokenCommonReponse { StatusCode = 200, Message = "success", Token = token };
                }
            }
            catch (Exception ex)
            {
                response = new TokenCommonReponse() { StatusCode = 403, Message = ex.Message };
            }
            return response;
        }
    }
}
