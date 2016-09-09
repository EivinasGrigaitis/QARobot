using System;
using System.Collections.Generic;
using System.Data.SqlServerCe;
using System.Diagnostics;
using OpenQA.Selenium.Firefox;

namespace QARobot
{
    class Program
    {
        static void Main()
        {

            Console.WriteLine(@"Welcome. Please select profile to use for scrapper:
                              1) Chrome on Linux;
                              2) Internet explorer on iOS;
                              3) Safari on Mac;
                              4) User default;
                              5) No profile;");
            var input = Console.ReadKey().KeyChar.ToString();
            int selectionKey;
            int.TryParse(input, out selectionKey);
            bool done = false;
            var _profile = new FirefoxProfile();
            while (!done)
            {
                switch (selectionKey)
                {
                    case 1:
                        Console.WriteLine("\r\nYou have selected Chrome on Linux profile.");
                        _profile = ProfileManager.ChangeProfileUserAgent(ProfileManager.ChromeOnLinuxProfile);
                        done = true;
                        break;
                    case 2:
                        Console.WriteLine("\r\nYou have selected Internet explorer on iOS profile.");
                        _profile = ProfileManager.ChangeProfileUserAgent(ProfileManager.IeOniOsProfile);
                        done = true;
                        break;
                    case 3:
                        Console.WriteLine("\r\nYou have selected Safari on Mac profile.");
                        _profile = ProfileManager.ChangeProfileUserAgent(ProfileManager.SafariOnMacProfile);
                        done = true;
                        break;
                    case 4:
                        Console.WriteLine("\r\nYou have selected user default profile.");
                        _profile = ProfileManager.CookieProfiles(ProfileManager.DefaultProfile);
                        done = true;
                        break;
                    case 5:
                        Console.WriteLine("\r\nYou have selected selenium profile.");
                        _profile = ProfileManager.CookieProfiles(ProfileManager.EmptyProfile);
                        done = true;
                        break;
                    default:
                        Console.WriteLine("\r\nIncorrect value. Please enter 1,2,3,4 or 5.");
                        input = Console.ReadKey().KeyChar.ToString();
                        int.TryParse(input, out selectionKey);
                        break;
                }
            }

            var scraper = new ActorScraper(_profile);
            Console.WriteLine("\r\nHow many actors would you like to scrap?");
            var readQuantity = Console.ReadKey().KeyChar.ToString();
            int quantity;
            int.TryParse(readQuantity, out quantity);

            var actorDict = new Dictionary<string, string>();

            
            for (int i = 0; i < quantity; i++)
            {
                Console.WriteLine("\r\nPlease enter actor Name and Surname (Separated with space): ");
                var actorName = Console.ReadLine();
                try
                {
                    var nameNumPair = ActorScraper.confirmActorPrompt(actorName);
                    actorDict.Add(nameNumPair.Key, nameNumPair.Value);
                }
                catch (Exception)
                {
                    Console.WriteLine("Sorry, couldn't find actor.");
                    i--;
                }
            }

            scraper.ScrapeActors(actorDict);

            var swatch = new Stopwatch();
            //var aktoriai = new List<string>
            //      {

            //         "Angelina Jolie",
            //          "Brad Pitt",
            //         //"Johnny Depp",
            //         // "Orlando Bloom",
            //         // "Keira Knightley",
            //         // "Leonardo DiCaprio",
            //         // "Tom Hardy"

            //      };

            swatch.Start();

            //Kiekviena karta, kai useris enterina aktoriu, padaryti:
            


            //var scraper = new Scraper(aktoriai); // Constructor accepts List<string> with actor names for scraping


            //foreach (var elem in scraper.GetActors())
            //{
            //    Console.WriteLine(elem);
            //}
            //Console.WriteLine($"-- Unique films for all actors - {scraper.GetUniqueFilms().Count} --");

            //var uniqueFilms = new List<Film>();//scraper.GetUniqueFilms();
            //var uniqueActors = new List<Actor>();

            Console.WriteLine("Data transfer to database");

            foreach (var actor in scraper.UniqueActors)
            {
                Database.AddActorToDb(actor.Name, actor.Surname, actor.Born);
            }

            foreach (var movie in scraper.UniqueFilms)
            {
                Database.AddMovieToDb(movie.Name, movie.Rating, movie.Genre, movie.Year);
            }

            Console.WriteLine("Actors and films query");
            foreach (var actor in scraper.UniqueActors)
            {
                int actorId = Database.SelectActorId(actor.Name, actor.Surname);
                foreach (var film in actor.Films)
                {
                    int movieId = Database.SelectFilmId(film.Name);
                    Database.SelectActorFilmId(actorId, movieId);
                }
            }
            swatch.Stop();
            Console.WriteLine(swatch.Elapsed);
            Console.WriteLine("Completed :)");
            Console.WriteLine("Press any key to start query");
            Console.ReadLine();
            Console.Write("Actor with most films : ");
            Database.GetActorWithMostFilms();
            Console.ReadLine();
            Console.Write("Actor with biggest film rating : ");
            Database.GetFilmWithBiggestRating();
            Console.ReadLine();

            //TODO pasidaryti universalias uzklausas
            //Console.WriteLine("Brad Pitt and Angelina Jolie Pitt movies :");
            //DatabaseQuery.ActorsAndMovies(BradAndJolie);
            //Console.ReadLine();
            //Console.WriteLine("Orlando Bloom, Keira Knighley, Johny Depp movies :");
            //DatabaseQuery.ActorsAndMovies(OrlandoBloomKeiraAndDepp);
            //Console.ReadLine();
            //Console.WriteLine("Leonardo DiCaprio and Tom Hardy movies ::");
            //DatabaseQuery.ActorsAndMovies(TomHardyAndDiCaprio);
            //Console.ReadLine();
            Database.CloseConnections();
        }
    }
}


