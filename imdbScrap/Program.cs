using System;
using System.Collections.Generic;
using System.Data.SqlServerCe;
using System.Diagnostics;

namespace imdbScrap
{
    class Program
    {
      
        private static string BradAndJolie =
            "select  f.name, f.year, f.rating\r\nfrom FilmaiToActor as fa\r\ninner join Film as f on fa.filmId = f.filmId \r\ninner join actor as a on fa.ActorId=a.actorId \r\n \r\nwhere (a.name = \'Brad\' and a.surname = \'Pitt\')  or (a.name = \'Angelina\' and a.surname = \'Jolie\')\r\ngroup by  f.name,f.year, f.rating  \r\nhaving count(*) >1";

        private static string OrlandoBloomKeiraAndDepp = "select  f.name, f.year, f.rating\r\nfrom FilmaiToActor as fa\r\ninner join Film as f on fa.filmId = f.filmId \r\ninner join actor as a on fa.ActorId=a.actorId \r\n \r\nwhere (a.name = \'Johnny\' and a.surname = \'Depp\')  or (a.name = \'Orlando\' and a.surname = \'Bloom\') or (a.name = \'Keira\' and a.surname = \'Knightley\')\r\ngroup by  f.name, f.year, f.rating\r\nhaving count(*) >1";

        private static string TomHardyAndDiCaprio =
            "select  f.name, f.year, f.rating\r\nfrom FilmaiToActor as fa\r\ninner join Film as f on fa.filmId = f.filmId \r\ninner join actor as a on fa.ActorId=a.actorId \r\n \r\nwhere (a.name = \'Leonardo\' and a.surname = \'DiCaprio\')  or (a.name = \'Tom\' and a.surname = \'Hardy\') \r\ngroup by  f.name, f.year, f.rating \r\nhaving count(*) >1";
        static void Main()
        {
            var swatch = new Stopwatch();
            var aktoriai = new List<string>
                  {

                     "Angelina Jolie",
                      "Brad Pitt",
                     "Johnny Depp",
                      "Orlando Bloom",
                      "Keira Knightley",
                      "Leonardo DiCaprio",
                      "Tom Hardy"

                  };

            swatch.Start();

            var scraper = new Scraper(aktoriai); // Constructor accepts List<string> with actor names for scraping


            foreach (var elem in scraper.GetActors())
            {
                Console.WriteLine(elem);
            }


            Console.WriteLine($"-- Unique films for all actors - {scraper.GetUniqueFilms().Count} --");
            Console.WriteLine("Data transfer to database");

            using (SqlCeConnection connection = new SqlCeConnection(@"Data Source = database.sdf"))
            {
                try
                {
                    connection.Open();
                    foreach (var actor in scraper.GetActors())
                    {
                        DatabaseQuery.AddActorToDb(actor._name, actor._surname, actor.Born, connection);
                    }
                    foreach (var movie in scraper.GetUniqueFilms())
                    {
                        DatabaseQuery.AddMovieToDb(movie.Name, movie.Rating, movie.Genre, movie.Year,
                            connection);
                    }

                    Console.WriteLine("Actors and films query");


                    foreach (var actor in scraper.GetActors())
                    {
                        int actorId = DatabaseQuery.SelectActorId(actor._name, actor._surname, connection);
                        foreach (var film in actor.Films)
                        {
                            int movieId = DatabaseQuery.SelectFilmId(film.Name, connection);
                            DatabaseQuery.SelectActorFilmId(actorId, movieId, connection);
                        }
                    }
                    swatch.Stop();
                    Console.WriteLine(swatch.Elapsed);
                    Console.WriteLine("Completed :)");
                    Console.WriteLine("Press any key to start query");
                    Console.ReadLine();

                    Console.Write("Actor with most films : ");
                    DatabaseQuery.GetActorWithMostFilms(connection);
                    Console.ReadLine();

                    Console.Write("Actor with biggest film rating : ");
                    DatabaseQuery.GetFilmWithBiggestRating(connection);
                    Console.ReadLine();

                    Console.WriteLine("Brad Pitt and Angelina Jolie Pitt movies :");
                    DatabaseQuery.ActorsAndMovies(BradAndJolie, connection);
                    Console.ReadLine();

                    Console.WriteLine("Orlando Bloom, Keira Knighley, Johny Depp movies :");
                    DatabaseQuery.ActorsAndMovies(OrlandoBloomKeiraAndDepp, connection);
                    Console.ReadLine();

                    Console.WriteLine("Leonardo DiCaprio and Tom Hardy movies ::");
                    DatabaseQuery.ActorsAndMovies(TomHardyAndDiCaprio, connection);
                    Console.ReadLine();
                }
                catch (Exception ex)
                {
                    connection.Close();
                    Console.WriteLine("Error executing SQL query. " + ex.Message);
                }
            }
        }
    }
}

