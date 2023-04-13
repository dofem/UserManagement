using System.Net;

namespace UserManagement.API.Dto
{
    public class ApiResponse
    {
           public HttpStatusCode StatusCode { get; set; } = HttpStatusCode.OK;
            public bool IsSuccess { get; set; } = false;
            public string Messages { get; set; } = string.Empty;
            public object Result { get; set; } 
    }
}
