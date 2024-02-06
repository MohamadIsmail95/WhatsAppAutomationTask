using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WhatsAppServices.Helper;
using WhatsAppServices.Interfaces;

namespace WhatsAppServices.Services
{
    public class WhatsAppCookies : IWhatsAppCookies
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public WhatsAppCookies(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string CheckWhatsCookies()
        {
            string cookie = _httpContextAccessor.HttpContext!=null? _httpContextAccessor.HttpContext.Request.Cookies["screenId"]:null;
            if (!string.IsNullOrEmpty(cookie))
            {
                return cookie;
            }

            else
            {
                CookieOptions option = new CookieOptions();
                option.Expires = DateTime.Now.AddDays(10);
                _httpContextAccessor.HttpContext.Response.Cookies.Append("screenId", "WhatsApp" +RandomGenerator.RandomString(7), option);
            }

            return _httpContextAccessor.HttpContext.Request.Cookies["screenId"];
        }
    }
}
