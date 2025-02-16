using System.ComponentModel.DataAnnotations;

namespace GptApi.Models
{
    public class UserInfo
    {
        public int userid { get; set; }

        public string username { get; set; }

        public string password { get; set; }
        
        public int present_cnt { get; set; }

        public int have_cnt { get; set; }
    }

    public class UserChatCnt
    {
        [Key]
        public int userid { get; set; }

        public int present_cnt { get; set; }

        public int have_cnt { get; set; }
    }

    public class BeginRequest
    {
        public string? Question { get; set; }
    }

    public class GetMessageRequest
    {
        public string key { get; set; }
    }

    public class GptResult
    {
        public bool IsComplete { get; set; }
        public string? Body { get; set; }
    }
}
