using Dapper;
using GptApi.Data;
using GptApi.Models;
using Microsoft.EntityFrameworkCore;

namespace GptApi.DBServices
{
    public class AuthService
    {
        private readonly GptContext _context;

        public AuthService(GptContext context)
        {
            _context = context;
        }

        public async Task<UserInfo?> GetUser(int userid)
        {
            var conn = _context.Database.GetDbConnection();
                return await conn.QueryFirstOrDefaultAsync<UserInfo>(@"select u.userid,u.username,u.password,uc.present_cnt,uc.have_cnt 
from users u inner join userchatcnts uc on u.userid = uc.userid 
where u.userid = @userid", new { userid = userid });
        }
    }
}
