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


            var scraper = new ActorScraper(ProfileManager.GetProfile());

            Console.WriteLine("\r\nHow many actors would you like to scrap?");
 
            scraper.ScrapeActors(ActorScraper.EnterActors());
            
            SqlQueries.Dict = ActorScraper.EnterActors();

            var swatch = new Stopwatch();
            swatch.Start();

            Console.WriteLine("Data transfer to database");

            Database.FillDatabaseInfo(scraper);

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
            Console.WriteLine("Universal Query result (actors movies) :");
            Database.ActorsAndMovies(SqlQueries.UniversalString());
            Database.CloseConnections();
            Console.ReadLine();
        }
    }
}


