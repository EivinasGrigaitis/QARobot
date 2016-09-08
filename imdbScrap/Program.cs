using System;
using System.Collections.Generic;
using System.Data.SqlServerCe;
using System.Diagnostics;
using OpenQA.Selenium.Firefox;

namespace QARobot
{
    class Program
    {
        //TODO pasidaryti 1 universalia uzklausa skirtingiem aktoriam uzklausas
        //private static string BradAndJolie = @"
        //    SELECT  f.name, f.year, f.rating
        //    FROM FilmaiToActor AS fa
        //    INNER JOIN Film AS f ON fa.filmId = f.filmId
        //    INNER JOIN actor AS a ON fa.ActorId=a.actorId
        //    WHERE (a.name = \'Brad\' AND a.surname = \'Pitt\')  OR (a.name = \'Angelina\' AND a.surname = \'Jolie\')
        //    GROUP BY  f.name,f.year, f.rating  
        //    HAVING COUNT(*) >1";

        //private static string OrlandoBloomKeiraAndDepp = @"
        //    SELECT  f.name, f.year, f.rating
        //    FROM FilmaiToActor AS fa
        //    INNER JOIN Film AS f ON fa.filmId = f.filmId
        //    INNER JOIN actor AS a ON fa.ActorId=a.actorId 
        //    WHERE (a.name = \'Johnny\' AND a.surname = \'Depp\')  OR (a.name = \'Orlando\' AND a.surname = \'Bloom\') OR (a.name = \'Keira\' AND a.surname = \'Knightley\')
        //    GROUP BY  f.name, f.year, f.rating
        //    HAVING COUNT(*) >1";

        //private static string TomHardyAndDiCaprio = @"
        //    SELECT  f.name, f.year, f.rating
        //    FROM FilmaiToActor AS fa
        //    INNER JOIN Film AS f ON fa.filmId = f.filmId
        //    INNER JOIN actor AS a ON fa.ActorId=a.actorId 
        //    WHERE (a.name = \'Leonardo\' AND a.surname = \'DiCaprio\')  OR (a.name = \'Tom\' AND a.surname = \'Hardy\') 
        //    GROUP BY  f.name, f.year, f.rating 
        //    HAVING COUNT(*) >1";

        static void Main()
        {

            Console.WriteLine(@"Welcome. Please enter number of which user agent you want to be used on scrapper:
                              1) Chrome on Linux;
                              2) Internet explorer on iOS;
                              3) Safari on Mac;");
            var input = Console.ReadKey().KeyChar.ToString();
            int selectionKey;
            int.TryParse(input, out selectionKey);
            bool done = false;

            while (!done)
            {
                switch (selectionKey)
                {
                    case 1:
                        Console.WriteLine("\r\nYou have selected Chrome on Linux");
                        ProfileManager.ChangeProfileUserAgent(ProfileManager.ChromeOnLinux);
                        input = Console.ReadKey().KeyChar.ToString();
                        int.TryParse(input, out selectionKey);
                        done = true;
                        break;
                    case 2:
                        Console.WriteLine("\r\nYou have selected Internet explorer on iOS");
                        ProfileManager.ChangeProfileUserAgent(ProfileManager.IeOniOs);
                        input = Console.ReadKey().KeyChar.ToString();
                        int.TryParse(input, out selectionKey);
                        done = true;
                        break;
                    case 3:
                        Console.WriteLine("\r\nYou have selected Safari on Mac");
                        ProfileManager.ChangeProfileUserAgent(ProfileManager.SafariOnMac);
                        input = Console.ReadKey().KeyChar.ToString();
                        int.TryParse(input, out selectionKey);
                        done = true;
                        break;
                    default:
                        Console.WriteLine("\r\nIncorrect value. Please enter 1,2 or 3.");
                        input = Console.ReadKey().KeyChar.ToString();
                        int.TryParse(input, out selectionKey);
                        break;
                }
            }

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

            var scraper = new ActorScraper();
            scraper.ScrapeActors(new List<string> { "Brad Pitt", "Angelina Jolie" });


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
                DatabaseQuery.AddActorToDb(actor.Name, actor.Surname, actor.Born);
            }

            foreach (var movie in scraper.UniqueFilms)
            {
                DatabaseQuery.AddMovieToDb(movie.Name, movie.Rating, movie.Genre, movie.Year);
            }

            Console.WriteLine("Actors and films query");
            foreach (var actor in scraper.UniqueActors)
            {
                int actorId = DatabaseQuery.SelectActorId(actor.Name, actor.Surname);
                foreach (var film in actor.Films)
                {
                    int movieId = DatabaseQuery.SelectFilmId(film.Name);
                    DatabaseQuery.SelectActorFilmId(actorId, movieId);
                }
            }
            swatch.Stop();
            Console.WriteLine(swatch.Elapsed);
            Console.WriteLine("Completed :)");
            Console.WriteLine("Press any key to start query");
            Console.ReadLine();
            Console.Write("Actor with most films : ");
            DatabaseQuery.GetActorWithMostFilms();
            Console.ReadLine();
            Console.Write("Actor with biggest film rating : ");
            DatabaseQuery.GetFilmWithBiggestRating();
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
            DatabaseQuery.CloseConnections();
        }
    }
}


