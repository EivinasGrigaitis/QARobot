using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using Newtonsoft.Json.Linq;
using Console = Colorful.Console;

namespace QARobot
{
    class Program
    {
        static void Main()
        {
            var profile = ProfileManager.GetProfile();
            var actorDict = EnterActors();

            var scraper = new ActorScraper(profile);
            
            scraper.ScrapeActors(actorDict);
            
            SqlQueries.ActorObjList = scraper.UniqueActors.ToList();

            Database.FillDatabaseInfo(scraper);
            Console.WriteLine("Data transfer complete.", ProfileManager.SuccessColor);
            Console.Write("\r\nActor with most films: ", ProfileManager.ResultColor);
            Database.GetActorWithMostFilms();
            Console.Write("\r\nActor with biggest film rating: ", ProfileManager.ResultColor);
            Database.GetFilmWithBiggestRating();

            if (SqlQueries.ActorObjList.Count > 1)
            {
                Console.WriteLine($"\r\nThe {SqlQueries.ActorObjList.Count} actors Co-Star in all these films:", ProfileManager.ResultColor);
                Database.ActorsAndMovies(SqlQueries.UniversalString());

                if (SqlQueries.ActorObjList.Count > 2)
                {
                    Database.ActorsAndMovies(SqlQueries.CoStarMethod());
                }
            }

            Database.CloseConnections();

            Console.WriteLine("\r\nPress any key to exit...", ProfileManager.InfoColor);
            Console.ReadKey();
        }



        private static Dictionary<string, string> EnterActors()
        {
            var readQuantity = "";
            while (!SqlQueries.IsStringIntRange(readQuantity, 1, 10))
            {
                Console.WriteLine("\r\nHow many actors would you like to scrape? (Up to 10)", ProfileManager.InfoColor);
                readQuantity = Console.ReadLine();
            }
            var quantity = int.Parse(readQuantity);

            var actorDict = new Dictionary<string, string>();

            var i = 0;
            while (i < quantity)
            {
                Console.WriteLine($"\r\nPlease enter name of actor #{i + 1}: ", ProfileManager.InfoColor);
                var actorName = Console.ReadLine();
                try
                {
                    var nameNumPair = ConfirmActorPrompt(actorName);
                    actorDict.Add(nameNumPair.Key, nameNumPair.Value);
                    i++;
                }
                catch (Exception)
                {
                    Console.WriteLine($"Sorry, couldn't find `{actorName}`. Please try again.", ProfileManager.ErrorColor);
                }
            }
            return actorDict;
        }

        private static KeyValuePair<string, string> ConfirmActorPrompt(string actor)
        {
            var client = new WebClient();
            var suggestionJsonStr = client.DownloadString(string.Format(ActorScraper.imdbApiTemplate, string.Join("+", actor.Split(' '))));

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
                            Console.Write($"\r\nDid you mean: {currentName} ({currentContext})? y/n: ", ProfileManager.InfoColor);
                            var input = Console.ReadLine();
                            if (input.ToLower() == "y")
                            {
                                return new KeyValuePair<string, string>(currentName, currentId);
                            }

                            if (input.ToLower() == "n")
                            {
                                confirmedChoice = true;
                            }
                        }
                    }
                }
            }
            catch (Exception )
            {
                Console.WriteLine($"\r\nSorry, couldn't find {actor} on IMDB... Try another one!", ProfileManager.ErrorColor);
            }

            throw new Exception("Actor not found.");
        }
    }
}


