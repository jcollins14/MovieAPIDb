using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MovieAPIDB.Models;
using Newtonsoft.Json;

namespace MovieAPIDB.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

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

        public IActionResult Login()
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
