using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SPA.Engine;
using Microsoft.Extensions.Configuration;
using SPA.Models;

namespace SPA.Controllers
{
    [Produces("application/json")]
    [Route("api/SocialData")]
    public class SocialDataController : Controller
    {
        private ulong appId;
        private string login;
        private string password;
        //private string connection;
        private SocialContext _context;

        public SocialDataController(IConfiguration configuration, SocialContext context)
        {
            appId = configuration.GetValue<ulong>("VkApi:AppId");
            login = configuration.GetSection("VkApi")["Login"];
            password = configuration.GetSection("VkApi")["Password"];

            //connection = configuration.GetConnectionString("DefaultConnection");
            _context = context;
        }

        [HttpPost("repost")]
        public IEnumerable<Categorie> FavoriteRepostCategories([FromBody]RepostCategoriesRequest request)
        {
            var logger = new MyLogger();
            logger.Log($"repost start ({request.UserName})");

            var vkProvider = new VKProvider(appId, login, password, logger);

            var result = Main.GetUserFavoriteCategories(vkProvider, _context, request);

            logger.Log($"repost finish ({request.UserName})");
            return result;

        }

        [HttpGet("like/{userId}")]
        public IEnumerable<Categorie> LikeCategories(long userId)
        {
            var vkProvider = new VKProvider(appId, login, password, new MyLogger());

            return Main.GetUserLikeCategories(vkProvider, userId);
        }

        [HttpGet("person/{user}")]
        public Person GetPerson(string user)
        {
            return
                Main.GetPerson(
                    new VKProvider(appId, login, password, new MyLogger()),
                    user);
        }
    }
}