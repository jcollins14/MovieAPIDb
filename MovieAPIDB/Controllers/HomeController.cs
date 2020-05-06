using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieAPIDB.Models;
using Newtonsoft.Json;

namespace MovieAPIDB.Controllers
{
    public class HomeController : Controller
    {
        private readonly MovieAPIDBContext _context = new MovieAPIDBContext();
        public HomeController()
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
            if(user is object)
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
                using var context = new MovieAPIDBContext();
                var user = new User()
                {
                    Username = model.Username,
                    Password = model.Password
                };
                context.Users.Add(user);
                context.SaveChanges();

                return RedirectToAction("Index");
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

        public IActionResult TitleError()
        {
            return View();
        }

        public async Task<IActionResult> APISearch(string title, int year)
        {
            string apikey = "ae3ee0fe";
            if (title is null)
            {
                return RedirectToAction("TitleError");
            }
            title = title.Replace(' ', '+');
            string endpoint = "?s=" + title + "&y=" + year + "&type=movie&r=json&apikey=" + apikey;
            //omits the year from the endpoint if not in a current time
            if (year < 1850 || year > 2020)
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
            //redirects to error page if no results found
            if (results.TotalResults == 0)
            {
                return RedirectToAction("NoResult");
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
            //redirects to summary page if exactly one result found
            if (results.Results.Count == 1)
            {
                return RedirectToAction("DetailedMovieView", new { imdb = results.Results[0].ImdbID});
            }
            return View("ListView", results);
        }
        public IActionResult NoResult()
        {
            return View();
        }

        public IActionResult AddFavorite(string imdb)
        {
            int userID = (int)HttpContext.Session.GetInt32("UserId");
            Favorite add = new Favorite()
            {
                UserID = userID,
                IMDBID = imdb
            };
            var check = _context.Favorites.Where(x => x.UserID == userID && x.IMDBID == imdb).FirstOrDefault();
            if (check is null)
            {
                _context.Favorites.Add(add);
                _context.SaveChanges();
            }
            return RedirectToAction("Favorites");
        }

        public IActionResult RemoveFavorite(string imdb)
        {
            int userID = (int)HttpContext.Session.GetInt32("UserId");
            Favorite add = new Favorite()
            {
                UserID = userID,
                IMDBID = imdb
            };
            var check = _context.Favorites.Where(x => x.UserID == userID && x.IMDBID == imdb).FirstOrDefault();
            if (check != null)
            {
                _context.Favorites.Remove(add);
                _context.SaveChanges();
            }
            return RedirectToAction("Favorites");
        }

        public async Task<IActionResult> Favorites()
        {
            int userID = (int)HttpContext.Session.GetInt32("UserId");
            List<Favorite> data = _context.Favorites.Where(x => x.UserID == userID).ToList();
            string apikey = "ae3ee0fe";
            var client = new HttpClient
            {
                BaseAddress = new Uri("http://www.omdbapi.com/")
            };
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (compatible; GrandCircus/1.0)");
            Search favorites = new Search()
            {
                TotalResults = (int)data.Count,
                Results = new List<SearchResult>()
            };
            foreach (Favorite item in data)
            {
                string endpoint = "?i=" + item.IMDBID + "&type=movie&apikey=" + apikey + "&r=json";
                var result = await client.GetStringAsync(endpoint);
                Movie results = JsonConvert.DeserializeObject<Movie>(result);
                SearchResult add = results.MapToSearchResult();
                favorites.Results.Add(add);
            }
            return View("ListView",favorites);
        }
    }
}
