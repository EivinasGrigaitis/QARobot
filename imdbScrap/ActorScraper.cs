using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;


namespace QARobot
{
    class ActorScraper
    {
        private readonly IWebDriver _driver;
        private readonly string _baseUrl = "http://www.imdb.com";
        readonly NumberFormatInfo _decimalFormat = new NumberFormatInfo();

        public static readonly string imdbApiTemplate = 
            "http://www.imdb.com/xml/find?json=1&nr=1&nm=on&q={0}";

        readonly string _imdbActorFilmsTemplate =
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

        /// <summary>
        /// Fills the ActorScraper object with info by using a dict of actorName, actorImbdNumber strings.
        /// </summary>
        /// <param name="actorDict"></param>
        public void ScrapeActors(Dictionary<string, string> actorDict)
        {
            foreach (var actor in actorDict)
            {
                var actorFullname = actor.Key;
                var actorName = actorFullname.Split(' ')[0];
                var actorSurname = string.Join(" ", actorFullname.Split(' ').Skip(1));

                var actorNumber = actor.Value;

                _driver.Navigate().GoToUrl(_baseUrl + "/name/" + actorNumber);
                string actorBirthday = string.Empty;
                try
                {
                    actorBirthday =
                    _driver.FindElement(By.XPath(".//*[@id=\'name-born-info\']/time")).GetAttribute("datetime");
                }
                catch (NoSuchElementException) { }

                _driver.Navigate().GoToUrl(string.Format(_imdbActorFilmsTemplate, actorNumber));

                var currentActor = new Actor(actorName, actorSurname, actorBirthday);

                // Scrape films from page
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

                        string filmYear = string.Empty;
                        try
                        {
                            var text =
                                filmElem.FindElement(
                                    By.XPath(".//*[contains(@class, \'lister-item-year text-muted unbold\')]")).Text;
                            filmYear = Regex.Match(text, @"\((\d{4})\)").Groups[1].Value;
                        }
                        catch (NoSuchElementException) { }

                        string filmGenre = string.Empty;
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
            _driver.Quit();
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

        public string Fullname => Name + " " + Surname;

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

