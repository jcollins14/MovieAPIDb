using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MovieAPIDB.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

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
            if (user is object)
            {
                HttpContext.Session.SetInt32("UserId", user.ID);
                HttpContext.Session.SetString("Username", user.Username);
                HttpContext.Session.SetString("Password", user.Password);

                if (model.Password == user.Password)
                {
                    ViewBag.Name = user.Username;
                    return RedirectToAction("WelcomeLogin");
                }

                return RedirectToAction("LoginError");
            }
            else
            {
                return RedirectToAction("Login");
            }
        }

        public IActionResult WelcomeLogin()
        {
            return View();
        }

        public IActionResult Register()
        {
            return View();
        }

        public IActionResult LoginError()
        {
            return View();
        }

        [HttpPost]
        public IActionResult RegisterUser([Bind("Username, Password, ConfirmPassword")] User model)
        {
            var duplicate = _context.Users.Where(x => x.Username == model.Username).FirstOrDefault();
            if (duplicate is object)
            {
                ModelState.AddModelError("Username", "Username must be unique");
            }
            if (ModelState.IsValid)
            {
                if (model.Password == model.ConfirmPassword)
                {
                    var user = new User()
                    {
                        Username = model.Username,
                        Password = model.Password
                    };
                    _context.Users.Add(user);
                    _context.SaveChanges();

                    LoginUser(user);
                    return RedirectToAction("Index");
                }
            }
            return RedirectToAction("Register");
        }

        public IActionResult MovieSearch()
        {
            //login check
            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                return RedirectToAction("Login");
            }
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
            //check if movie is favorite to determine which button to display
            int userID = (int)HttpContext.Session.GetInt32("UserId");
            var check = _context.Favorites.Where(x => x.UserID == userID && x.IMDBID == result.IMDBID).FirstOrDefault();
            if (check is object)
            {
                ViewBag.Fav = true;
            }
            else
            {
                ViewBag.Fav = false;
            }
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
                return RedirectToAction("DetailedMovieView", new { imdb = results.Results[0].ImdbID });
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
            //duplicate checking for favorites
            if (check is null)
            {
                _context.Favorites.Add(add);
                _context.SaveChanges();
            }
            //refresh page
            return RedirectToAction("DetailedMovieView", new { imdb = add.IMDBID });
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
            //checking if favorite is present
            if (check != null)
            {
                _context.Favorites.Remove(check);
                _context.SaveChanges();
            }
            //return to favorites list
            return RedirectToAction("Favorites");
        }

        public async Task<IActionResult> Favorites()
        {
            //rerout back to index page if not logged in
            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                return RedirectToAction("Login");
            }
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
                //instantiate results list so future results can be added
                Results = new List<SearchResult>()
            };
            //call api to gather information on each favorite
            foreach (Favorite item in data)
            {
                string endpoint = "?i=" + item.IMDBID + "&type=movie&apikey=" + apikey + "&r=json";
                var result = await client.GetStringAsync(endpoint);
                Movie results = JsonConvert.DeserializeObject<Movie>(result);
                SearchResult add = results.MapToSearchResult();
                favorites.Results.Add(add);
            }
            return View("Favorites", favorites);
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index");
        }
    }
}