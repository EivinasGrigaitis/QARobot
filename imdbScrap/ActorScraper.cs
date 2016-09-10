using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;


namespace QARobot
{
    class ActorScraper
    {
        private readonly IWebDriver _driver;
        private readonly string _baseUrl = "http://www.imdb.com";
        readonly NumberFormatInfo _decimalFormat = new NumberFormatInfo();

        static readonly string _imdbApiTemplate = 
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

        public void ScrapeActors(Dictionary<string, string> actorDict)
        {
            foreach (var actor in actorDict)
            {
                var actorFullname = actor.Key;
                var actorName = actorFullname.Split(' ')[0];
                var actorSurname = string.Join(" ", actorFullname.Split(' ').Skip(1));

                var actorNumber = actor.Value;

                _driver.Navigate().GoToUrl(string.Format(_imdbApiTemplate, actorName, actorSurname));

                _driver.Navigate().GoToUrl(_baseUrl + "/name/" + actorNumber);
                string actorBirthday = "";
                try
                {
                    actorBirthday =
                    _driver.FindElement(By.XPath(".//*[@id=\'name-born-info\']/time")).GetAttribute("datetime");
                }
                catch (NoSuchElementException) { }

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
                        catch (NoSuchElementException) { }

                        string filmYear = null;
                        try
                        {
                            var text =
                                filmElem.FindElement(
                                    By.XPath(".//*[contains(@class, \'lister-item-year text-muted unbold\')]")).Text;
                            filmYear = Regex.Match(text, @"\((\d{4})\)").Groups[1].Value;
                        }
                        catch (NoSuchElementException) { }

                        string filmGenre = null;
                        try
                        {
                            filmGenre =
                                filmElem.FindElement(By.XPath(".//*[contains(@class, \'genre\')]")).Text.Trim();
                        }
                        catch (NoSuchElementException) { }

                        var currentFilm = new Film(filmName, filmRating, filmYear, filmGenre);

                        currentActor.Films.Add(currentFilm);
                        UniqueFilms.Add(currentFilm);
                    }

                    try
                    {
                        _driver.FindElement(By.ClassName("next-page")).Click();
                        System.Threading.Thread.Sleep(1500);
                    }
                    catch (NoSuchElementException)
                    {
                        break;
                    }


                }

                UniqueActors.Add(currentActor);
            }
        }

        //private Dictionary<string, string> GetActorsNumbers(List<string> actorNames)
        //{
        //    var actorDict = new Dictionary<string, string>();
        //    var _client = new WebClient();


        //    foreach (var actor in actorNames)
        //    {
        //        var suggestionJsonStr = _client.DownloadString(string.Format(_imdbApiTemplate, string.Join("+", actor.Split(' '))));
        //        var actorsJson = JObject.Parse(suggestionJsonStr);

        //        try
        //        {
        //            var pair = confirmActorPrompt(actorsJson);
        //            actorDict.Add(pair.Key, pair.Value);
        //            //Console.WriteLine("\r\nSorry, no more actors found...");
        //        }
        //        catch (Exception e)
        //        {
        //            Console.WriteLine($"\r\nSorry, couldn't find {actor} on IMDB... ");
        //        }
        //    }

        //    return actorDict;
        //}

        public static KeyValuePair<string, string> confirmActorPrompt(string actor)
        {
            var _client = new WebClient();
            var suggestionJsonStr = _client.DownloadString(string.Format(_imdbApiTemplate, string.Join("+", actor.Split(' '))));
            var actorsJson = JObject.Parse(suggestionJsonStr);


            if (actorsJson.Count == 0)
            {
                throw new Exception("No actors found.");
            }
            try
            {
                foreach (var actorCategory in actorsJson.Children())
                {
                    foreach (var entry in actorCategory.Children())
                    {
                        var currentName = entry.First["name"].Value<string>();
                        var currentContext = entry.First["description"].Value<string>();
                        var currentId = entry.First["id"].Value<string>();

                        bool confirmedChoice = false;
                        while (!confirmedChoice)
                        {
                            Console.Write($"\r\nDid you mean: {currentName} ({currentContext})? y/n: ");
                            var input = Console.ReadLine();
                            if (input.StartsWith("y"))
                            {
                                return new KeyValuePair<string, string>(currentName, currentId);
                            }
                            confirmedChoice = true;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"\r\nSorry, couldn't find {actor} on IMDB... ");
            }


            throw new Exception("Actor not found.");
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

