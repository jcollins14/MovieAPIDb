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

        [HttpPost]
        public IActionResult LoginUser(User model)
        {
            var user = _context.Users.Where(x => x.Username == model.Username).FirstOrDefault();
            if(!ReferenceEquals(user, null))
            {
                HttpContext.Session.SetInt32("UserId", user.ID);
                HttpContext.Session.SetString("Username", user.Username);
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

        [HttpPost]
        public IActionResult RegisterUser([Bind("Username, Password")] User model)
        {
            if (ModelState.IsValid)
            {
                using (var context = new MovieAPIDBContext())
                {
                    var user = new User()
                    {
                        Username = model.Username,
                        Password = model.Password
                    };
                    context.Users.Add(user);
                    context.SaveChanges();

                    return RedirectToAction("Index");
                }
            }
            return RedirectToAction("Register");
        }

        public IActionResult MovieSearch()
        {
            return View();
        }

        public async Task<IActionResult> DetailedMovieView(string imdb)
        {
            string apikey = "ae3ee0fe";
            string endpoint = "?i=" + imdb + "&type=movie&apikey=" + apikey + "&r=json";
            var client = new HttpClient
            {
                BaseAddress = new Uri("http://www.omdbapi.com/")
            };
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (compatible; GrandCircus/1.0)");
            var data = await client.GetStringAsync(endpoint);
            var result = JsonConvert.DeserializeObject<Movie>(data);
            return View(result);
        }

        public async Task<IActionResult> APISearch(string title, int year)
        {
            string apikey = "ae3ee0fe";
            title = title.Replace(' ', '+');
            string endpoint = "?s=" + title + "&y=" + year + "&type=movie&r=json&apikey=" + apikey;
            //omits the year from the endpoint if not in a current time
            if (year < 1900 || year > 2020)
            {
                endpoint = "?s=" + title + "&type=movie&r=json&apikey=" + apikey;
            }
            var client = new HttpClient
            {
                BaseAddress = new Uri("http://www.omdbapi.com/")
            };
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (compatible; GrandCircus/1.0)");
            string data = await client.GetStringAsync(endpoint);
            Search results = JsonConvert.DeserializeObject<Search>(data);
            //calls the api again if more than one page of results
            if (results.TotalResults > 10)
            {
                endpoint += "&page=2";
                data = await client.GetStringAsync(endpoint);
                Search page2 = JsonConvert.DeserializeObject<Search>(data);
                foreach (SearchResult result in page2.Results)
                {
                    results.Results.Add(result);
                }
            }
            //remove duplicates from list by imdbID
            for (int i = 0; i < results.Results.Count; i++)
            {
                for (int a = 0; a < results.Results.Count; a++)
                {
                    if (results.Results[i].ImdbID == results.Results[a].ImdbID && i != a)
                    {
                        results.Results.RemoveAt(a);
                    }
                }
            }
            if (results.Results.Count == 1)
            {
                 DetailedMovieView(results.Results[0].ImdbID);
            }
            return View("ListView", results);
        }

        public IActionResult MovieView(Movie movie)
        {
            return View(movie);
        }
    }
}
