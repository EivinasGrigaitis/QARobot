using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace QARobot
{
    //public class Scraper
    //{
    //    private IWebDriver _driver;
    //    private readonly string _baseUrl = "http://www.imdb.com";
    //    HtmlWeb htmlWeb = new HtmlWeb();
    //    NumberFormatInfo formatinfo = new NumberFormatInfo();
    //    WebDriverWait wait;

    //    private static bool nextPagePresent;

    //    private FirefoxProfile NoImagesProfile;

    //    readonly List<string> _actorList = new List<string>
    //    {
    //        "Johnny Depp",
    //        "Al Pacino"

    //    };

    //    public Scraper(List<string> actorList = null)
    //    {
    //        Actors = new HashSet<Actor>();
    //        Films = new HashSet<Film>();
    //        _scraped = false;

    //        formatinfo.NumberDecimalSeparator = ".";

    //        NoImagesProfile = new FirefoxProfile();
    //        NoImagesProfile.SetPreference("permissions.default.stylesheet", 2);
    //        NoImagesProfile.SetPreference("permissions.default.image", 2);
    //        NoImagesProfile.SetPreference("dom.ipc.plugins.enabled.libflashplayer.so", "false");

    //        //_driver = new FirefoxDriver(NoImagesProfile);
    //        _driver = new FirefoxDriver();

            

    //        wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(8));

    //        nextPagePresent = false;

    //        if (actorList != null)
    //            _actorList = actorList;
    //    }

    //    public HashSet<Actor> Actors;

    //    public HashSet<Film> Films;

    //    private bool _scraped;

    //    public List<Actor> GetActors()
    //    {
    //        if (!_scraped)
    //        {
    //            Scrape();
    //        }

    //        return Actors.ToList();
    //    }

    //    public List<Film> GetUniqueFilms()
    //    {
    //        if (!_scraped)
    //        {
    //            Scrape();
    //        }

    //        return Films.ToList();
    //    }



    //    private void ToActorFeatureFilms(string actorNum)
    //    {
    //        _driver.Navigate()
    //            .GoToUrl(
    //                $"http://www.imdb.com/filmosearch?explore=title_type&role={actorNum}&ref_=filmo_ref_typ&sort=user_rating,desc&mode=detail&page=1&title_type=movie");
    //    }

    //    private void Scrape()
    //    {
    //        foreach (string actor in _actorList)
    //        {
    //            Console.WriteLine($"-- Scraping movies for {actor}. . . --");

    //            string name = actor.Split(' ')[0];
    //            string surname = actor.Split(' ')[1];

    //            var currentActor = new Actor(name, surname);


    //            //_driver.Navigate().GoToUrl(_baseUrl + "/?ref_=nv_home");
    //            //GoToActorPage(actor);

    //            string responseJson =
    //                new WebClient().DownloadString("http://www.imdb.com/xml/find?json=1&nr=1&nm=on&q=" + name + "+" + surname);
    //            JObject parsedJson = JObject.Parse(responseJson);
    //            string actorNum = parsedJson.First.First.First.First.First.Value<string>(); // Don't ask...

    //            //string url;

    //            //wait.Until(ExpectedConditions.UrlContains("name/nm"));

    //            //url = _driver.Url;
    //            //actorNum = url.Split('/')[4];

    //            Task getBornDateTask = Task.Factory.StartNew(() => SetActorsBornDate(htmlWeb.Load("http://www.imdb.com/name/" + actorNum), currentActor));
    //            Task toFeatureFilmsTask = Task.Factory.StartNew(() => ToActorFeatureFilms(actorNum));

    //            Task.WaitAll(getBornDateTask, toFeatureFilmsTask);

    //            // Scrape the pages
    //            while (true)
    //            {
    //                HtmlDocument document = new HtmlDocument();//htmlWeb.Load(_driver.Url);
    //                document.LoadHtml(_driver.PageSource);

    //                Task scrapePageTask = Task.Factory.StartNew(() => ScrapePage(document, currentActor));
    //                Task goToNextPageTask = Task.Factory.StartNew(TryNextPage);

    //                Task.WaitAll(scrapePageTask, goToNextPageTask);

    //                if (!nextPagePresent) break;
    //            }

    //            Actors.Add(currentActor);
    //            Console.WriteLine($"-- Done scraping for {actor} --");
    //        }
            
    //        _driver.Quit();
    //        _scraped = true;

    //    }

    //    private void ScrapePage(HtmlDocument document, Actor currentActor)
    //    {
    //        foreach (var node in document.DocumentNode.SelectNodes("//*[contains(@class,\'lister-item-content\')]"))
    //        {
    //            var film = new Film(GetFilmName(node), GetFilmRating(node), GetFilmYear(node)) { Genre = GetFilmGenre(node) };
    //            Films.Add(film);
    //            currentActor.Films.Add(film);
    //        }
    //    }

    //    private string GetFilmYear(HtmlNode node)
    //    {
    //        var text = node.SelectSingleNode(".//*[contains(@class, \'lister-item-year text-muted unbold\')]").InnerText;
    //        var match = Regex.Match(text, @"\((\d{4})\)").Groups[1];
    //        return match.Value;
    //    }

    //    private string GetFilmName(HtmlNode node)
    //    {
    //        return
    //            node.SelectSingleNode(".//*[contains(@class, \'lister-item-header\')]")
    //                .Descendants("a")
    //                .First()
    //                .InnerText;
    //    }

    //    private void TryNextPage()
    //    {
    //        try
    //        {
    //            var url = _driver.Url;
    //            _driver.FindElement(By.ClassName("next-page")).Click();
    //            while (true)
    //            {
    //                if (url != _driver.Url)
    //                    break;
    //            }
    //            nextPagePresent = true;
    //        }
    //        catch (NoSuchElementException)
    //        {
    //            nextPagePresent = false;
    //        }
    //    }

    //    private string GetFilmGenre(HtmlNode node)
    //    {
    //        try
    //        {
    //            return node.SelectSingleNode(".//*[contains(@class, \'genre\')]").InnerText.Trim();
    //        }
    //        catch (NullReferenceException)
    //        {
    //            return "";
    //        }
    //    }

    //    private decimal GetFilmRating(HtmlNode node)
    //    {


    //        try
    //        {
    //            var originalRating = node.SelectSingleNode("//*[contains(@class, \'ratings-imdb-rating\')]")
    //                .Descendants("strong")
    //                .First()
    //                .InnerText.Replace(',', '.');

    //            return
    //                decimal.Parse(originalRating, NumberStyles.Float, formatinfo);
    //        }
    //        catch (NullReferenceException)
    //        {
    //            return 0m;
    //        }
    //    }
    //    private void GoToActorPage(string actor)
    //    {
    //        _driver.FindElement(By.Id("navbar-query")).Click();
    //        _driver.FindElement(By.Id("navbar-query")).Clear();
    //        _driver.FindElement(By.Id("navbar-query")).SendKeys(actor);
    //        wait.Until(ExpectedConditions.TextToBePresentInElement(_driver.FindElement(By.ClassName("suggestionlabel")), actor));
    //        _driver.FindElement(By.ClassName("suggestionlabel")).Click();
    //    }

    //    private void SetActorsBornDate(HtmlDocument document, Actor actor)
    //    {
    //        try
    //        {
    //            //HtmlDocument document = htmlWeb.Load(_driver.Url);
    //            actor.Born = document.DocumentNode.SelectSingleNode(".//*[@id=\'name-born-info\']/time").Attributes["datetime"].Value;
    //        }
    //        catch (Exception e)
    //        {
    //            Console.WriteLine($"Can't set born date of actor: `{e}`");
    //        }
    //    }
    //}




