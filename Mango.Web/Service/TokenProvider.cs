using Mango.Web.Service.IService;
using Mango.Web.Utils;
using Newtonsoft.Json.Linq;

namespace Mango.Web.Service
{
    public class TokenProvider : ITokenProvider
    {
        private readonly IHttpContextAccessor _contextAcessor;

        public TokenProvider(IHttpContextAccessor contextAccessor)
        {
            _contextAcessor = contextAccessor;
        }

        public void ClearToken()
        {
            _contextAcessor.HttpContext?.Response.Cookies.Delete(SD.TokenCookie);
        }

        public string? GetToken()
        {
            string? token = null;
            bool? hasToken = _contextAcessor.HttpContext?.Request.Cookies.TryGetValue(SD.TokenCookie, out token);
            return hasToken is true ? token : null;
        }

        public void SetToken(string token)
        {
            _contextAcessor.HttpContext?.Response.Cookies.Append(SD.TokenCookie, token);
        }
    }
}
