using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging; //If this becomes an issue ...
using MovieAPIDB.Models;
using Newtonsoft.Json;

namespace MovieAPIDB.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        private MovieAPIDBContext _context = new MovieAPIDBContext();

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult Login(User model)
        {
            var user = _context.Users.Where(x => x.Username == model.Username).FirstOrDefault();
            if(!ReferenceEquals(User, null))
            {
                HttpContext.Session.SetInt32("UserId", user.ID);
                HttpContext.Session.SetString("UserName", user.Username);
                HttpContext.Session.SetString("Password",user.Password);
                return RedirectToAction("Index");
            }
            else
            {
                return RedirectToAction("Login");
            }
        }

        public IActionResult Register()
        {
            return View();
        }

        public IActionResult MovieSearch()
        {
            return View();
        }

        public async Task<IActionResult> APICall(string title, int year)
        {
            string apikey = "ae3ee0fe";
            title = title.Replace(' ', '+');
            string endpoint = "?t=" + title + "&y=" + year + "&apikey=" + apikey + "&r=json";
            //if(title == null)
            //{
            //    NullReferenceException
            //}
            if (year < 1900 || year > 2020)
            {
                endpoint = "?t=" + title + "&apikey=" + apikey + "&r=json";
            }
            var client = new HttpClient();
            client.BaseAddress = new Uri("http://www.omdbapi.com/");
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (compatible; GrandCircus/1.0)");
            var data = await client.GetStringAsync(endpoint);
            var result = JsonConvert.DeserializeObject<Movie>(data);
            return View("MovieView", result);
        }
        public IActionResult MovieView(Movie movie)
        {
            return View(movie);
        }
    }
}
