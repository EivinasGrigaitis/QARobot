using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using Newtonsoft.Json.Linq;

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
 
            scraper.ScrapeActors(EnterActors());
            
            SqlQueries.Dict = EnterActors();

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



        private static Dictionary<string, string> EnterActors()
        {
            var readQuantity = Console.ReadKey().KeyChar.ToString();
            int quantity;
            int.TryParse(readQuantity, out quantity);

            var actorDict = new Dictionary<string, string>();


            for (var i = 0; i < quantity; i++)
            {
                Console.WriteLine("\r\nPlease enter actor Name and Surname (separated with space): ");
                var actorName = Console.ReadLine();
                try
                {
                    var nameNumPair = ConfirmActorPrompt(actorName);
                    actorDict.Add(nameNumPair.Key, nameNumPair.Value);
                }
                catch (Exception)
                {
                    Console.WriteLine("Sorry, couldn't find actor.");
                    i--;
                }
            }

            return actorDict;
        }

        private static KeyValuePair<string, string> ConfirmActorPrompt(string actor)
        {
            var _client = new WebClient();
            var suggestionJsonStr = _client.DownloadString(string.Format(ActorScraper._imdbApiTemplate, string.Join("+", actor.Split(' '))));
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
                Console.WriteLine($"\r\nSorry, couldn't find {actor} on IMDB... Try another one!");
            }

            throw new Exception("Actor not found.");
        }
    }
}


