﻿using Microsoft.OpenApi.Writers;

namespace GptApi.Models
{
    public class TokenRequest
    {
        public int UserID { get; set; }
    }

    public class CommonResponse
    {
        public int StatusCode { get; set; }
        public string Message { get; set; }

    }

    public class BeginReponse : CommonResponse
    {
        public BeginReponse()
        {

        }
        public BeginReponse(int statusCode, string message, string? result = null)
        {
            StatusCode = statusCode;
            Message = message;
            Result = result;
        }
        public string? Result { get; set; }
    }

    public class GetMessageReponse : CommonResponse
    {
        public bool? IsComplete { get; set; }
        public string? Body { get; set; }
    }

    public class RemoveDictionaryResponse: CommonResponse
    {

    }

    public class TokenCommonReponse():CommonResponse
    {
        public string? Token { get; set; }
    }
}
