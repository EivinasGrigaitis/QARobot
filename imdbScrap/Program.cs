using System;
using System.Collections.Generic;
using System.Data.SqlServerCe;
using System.Diagnostics;

namespace QARobot
{
    class Program
    {

        private static string BradAndJolie = @"
            SELECT  f.name, f.year, f.rating
            FROM FilmaiToActor AS fa
            INNER JOIN Film AS f ON fa.filmId = f.filmId
            INNER JOIN actor AS a ON fa.ActorId=a.actorId
            WHERE (a.name = \'Brad\' AND a.surname = \'Pitt\')  OR (a.name = \'Angelina\' AND a.surname = \'Jolie\')
            GROUP BY  f.name,f.year, f.rating  
            HAVING COUNT(*) >1";

        private static string OrlandoBloomKeiraAndDepp = @"
            SELECT  f.name, f.year, f.rating
            FROM FilmaiToActor AS fa
            INNER JOIN Film AS f ON fa.filmId = f.filmId
            INNER JOIN actor AS a ON fa.ActorId=a.actorId 
            WHERE (a.name = \'Johnny\' AND a.surname = \'Depp\')  OR (a.name = \'Orlando\' AND a.surname = \'Bloom\') OR (a.name = \'Keira\' AND a.surname = \'Knightley\')
            GROUP BY  f.name, f.year, f.rating
            HAVING COUNT(*) >1";

        private static string TomHardyAndDiCaprio = @"
            SELECT  f.name, f.year, f.rating
            FROM FilmaiToActor AS fa
            INNER JOIN Film AS f ON fa.filmId = f.filmId
            INNER JOIN actor AS a ON fa.ActorId=a.actorId 
            WHERE (a.name = \'Leonardo\' AND a.surname = \'DiCaprio\')  OR (a.name = \'Tom\' AND a.surname = \'Hardy\') 
            GROUP BY  f.name, f.year, f.rating 
            HAVING COUNT(*) >1";

        static void Main()
        {
            var swatch = new Stopwatch();
            var aktoriai = new List<string>
                  {

                     "Angelina Jolie",
                      "Brad Pitt",
                     //"Johnny Depp",
                     // "Orlando Bloom",
                     // "Keira Knightley",
                     // "Leonardo DiCaprio",
                     // "Tom Hardy"

                  };

            swatch.Start();

            var scraper = new Scraper(aktoriai); // Constructor accepts List<string> with actor names for scraping


            foreach (var elem in scraper.GetActors())
            {
                Console.WriteLine(elem);
            }


            Console.WriteLine($"-- Unique films for all actors - {scraper.GetUniqueFilms().Count} --");
            Console.WriteLine("Data transfer to database");


            foreach (var actor in scraper.GetActors())
            {
                DatabaseQuery.AddActorToDb(actor._name, actor._surname, actor.Born);
            }
            foreach (var movie in scraper.GetUniqueFilms())
            {
                DatabaseQuery.AddMovieToDb(movie.Name, movie.Rating, movie.Genre, movie.Year);
            }

            Console.WriteLine("Actors and films query");


            foreach (var actor in scraper.GetActors())
            {
                var actorId = DatabaseQuery.SelectActorId(actor._name, actor._surname);
                foreach (var film in actor.Films)
                {
                    var movieId = DatabaseQuery.SelectFilmId(film.Name);
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

            Console.WriteLine("Brad Pitt and Angelina Jolie Pitt movies :");
            DatabaseQuery.ActorsAndMovies(BradAndJolie);
            Console.ReadLine();

            Console.WriteLine("Orlando Bloom, Keira Knighley, Johny Depp movies :");
            DatabaseQuery.ActorsAndMovies(OrlandoBloomKeiraAndDepp);
            Console.ReadLine();

            Console.WriteLine("Leonardo DiCaprio and Tom Hardy movies ::");
            DatabaseQuery.ActorsAndMovies(TomHardyAndDiCaprio);
            Console.ReadLine();


        }
    }
}


