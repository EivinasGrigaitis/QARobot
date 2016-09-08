using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using System.Diagnostics;

namespace QARobot
{
    class ActorScraper
    {
        private readonly IWebDriver _driver;
        private readonly string _baseUrl = "http://www.imdb.com";
        readonly NumberFormatInfo _decimalFormat = new NumberFormatInfo();

        private readonly string _imdbApiTemplate = 
            "http://www.imdb.com/xml/find?json=1&nr=1&nm=on&q={0}";

        private readonly string _imdbActorFilmsTemplate =
            "http://www.imdb.com/filmosearch?explore=title_type&role={0}&title_type=movie";

        public HashSet<Actor> UniqueActors = new HashSet<Actor>();
        public HashSet<Film> UniqueFilms = new HashSet<Film>();

        public ActorScraper(FirefoxProfile profile)
        {
            _driver = new FirefoxDriver(profile);
            _driver.Manage().Window.Maximize();

            _decimalFormat.NumberDecimalSeparator = ".";
        }

         ~ActorScraper()
        {
            _driver.Quit();
        }

        public List<Actor> GetScrapedActors()
        {
            return UniqueActors.ToList();
        }

        public List<Film> GetScrapedFilms()
        {
            return UniqueFilms.ToList();
        }

        public void ScrapeActors(List<string> actorNames)
        {
            var variable = GetActorsNumbers(actorNames);

            foreach (var actor in actorNames)
            {
                var actorName = actor.Split(' ')[0];
                var actorSurname = string.Join(" ", actor.Split(' ').Skip(1));

                // Get unique actor imdb number
                _driver.Navigate().GoToUrl(string.Format(_imdbApiTemplate, actorName, actorSurname));

                string jsonResponse = _driver.FindElement(By.TagName("body")).Text;
                var parsedJson = JObject.Parse(jsonResponse);

                string actorNumber = string.Empty;

                try
                {
                    actorNumber = parsedJson["name_popular"][0]["id"].Value<string>();
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Could not find actor {actorName} {actorSurname}: {e}");
                }

                _driver.Navigate().GoToUrl(_baseUrl + "/name/" + actorNumber);
                string actorBirthday =
                    _driver.FindElement(By.XPath(".//*[@id=\'name-born-info\']/time")).GetAttribute("datetime");

                _driver.Navigate().GoToUrl(string.Format(_imdbActorFilmsTemplate, actorNumber));

                var currentActor = new Actor(actorName, actorSurname, actorBirthday);

                // Scrape films
                while (true)
                {
                    foreach (var filmElem in _driver.FindElements(By.XPath("//*[contains(@class,\'lister-item-content\')]")))
                    {
                        string filmName =
                            filmElem.FindElement(By.ClassName("lister-item-header")).FindElement(By.TagName("a")).Text;

                        decimal filmRating = 0;
                        try
                        {
                            string filmRatingStr =
                                filmElem.FindElement(By.ClassName("ratings-imdb-rating")).FindElement(By.TagName("strong")).Text.Replace(',', '.');
                            filmRating = decimal.Parse(filmRatingStr, _decimalFormat);
                        }
                        catch (NoSuchElementException) { Console.WriteLine(); }

                        string filmYear = null;
                        try
                        {
                            var text =
                                filmElem.FindElement(
                                    By.XPath(".//*[contains(@class, \'lister-item-year text-muted unbold\')]")).Text;
                            filmYear = Regex.Match(text, @"\((\d{4})\)").Groups[1].Value;
                        }
                        catch (NoSuchElementException) { Console.WriteLine(); }

                        string filmGenre = null;
                        try
                        {
                            filmGenre =
                                filmElem.FindElement(By.XPath(".//*[contains(@class, \'genre\')]")).Text.Trim();
                        }
                        catch (NoSuchElementException) { Console.WriteLine(); }

                        var currentFilm = new Film(filmName, filmRating, filmYear, filmGenre);
                        Console.WriteLine(currentFilm);

                        currentActor.Films.Add(currentFilm);
                        UniqueFilms.Add(currentFilm);
                    }

                    try
                    {
                        _driver.FindElement(By.ClassName("next-page")).Click();
                        System.Threading.Thread.Sleep(3000);
                    }
                    catch (NoSuchElementException)
                    {
                        break;
                    }


                }

                UniqueActors.Add(currentActor);
            }
        }

        private Dictionary<string, string> GetActorsNumbers(List<string> actorNames)
        {
            var actorDict = new Dictionary<string, string>();
            var _client = new WebClient();

            var sw = new Stopwatch();
            sw.Start();

            foreach (var actor in actorNames)
            {
                var suggestionJsonStr = _client.DownloadString(string.Format(_imdbApiTemplate, string.Join("+", actor.Split(' '))));
                //actorDict.Add(actor, "0");
            }

            sw.Stop();
            Console.WriteLine($"Scraping actor ids took: {sw.Elapsed}");

            return actorDict;
        } 
    }

    public class Actor
    {
        public string Name;
        public string Surname;
        public HashSet<Film> Films;
        public string Born;

        public Actor(string name, string surname, string birthday)
        {
            Name = name;
            Surname = surname;
            Born = birthday;
            Films = new HashSet<Film>();
        }

        public override string ToString()
        {
            return $"{Name} {Surname} ({Born}) - {Films.Count} films";
        }

        public override bool Equals(object obj)
        {
            return ToString() == obj.ToString();
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }
    }

    public class Film
    {
        public string Name;
        public decimal Rating;
        public string Genre;
        public string Year;

        public Film(string name, decimal rating, string year, string genre)
        {
            Name = name;
            Rating = rating;
            Year = year;
            Genre = genre;
        }

        public override string ToString()
        {
            return $"{Name} ({Year}) - {Rating}: {Genre}";
        }

        public override bool Equals(object obj)
        {
            return ToString() == obj.ToString();
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }
    }
}

