using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QARobot
{
    class SqlQueries
    {
        public static string DbConnection = @"
                    Data Source = database.sdf";

        public static string AddActorToDb = @"
                    INSERT INTO actor(Name, Surname, Born) 
                    VALUES(@name, @surname, @born)";

        public static string AddMovieToDb = @"
                    INSERT INTO Film(Name, Rating, Genre, Year)
                    VALUES(@name, @rating, @genre, @year)";

        public static string SelectMovieId = @"
                    SELECT [filmId]
                    FROM [Film] 
                    WHERE [name] = @name";

        public static string SelectActorMovieId = @"
                    INSERT INTO FilmaiToActor(actorId, filmId)
                    VALUES(@actorId, @filmId)";

        public static string GetActorWithMostFilms = @"
                    SELECT  a.Name,a.surname, COUNT(f.name) AS filmuSk
                    FROM FilmaiToActor AS fa 
                    INNER JOIN Film AS f ON fa.filmId = f.filmId
                    INNER JOIN actor AS a ON fa.ActorId = a.actorId 
                    GROUP BY a.Name, a.surname
                    ORDER BY filmuSk DESC";

        public static string GetFilmWithBiggestRating = @"
                    SELECT a.Name, a.surname, f.name, MAX(f.rating) AS filmuRating
                    FROM FilmaiToActor AS fa 
                    INNER JOIN Film AS f ON fa.filmId = f.filmId  
                    INNER JOIN actor AS a ON fa.ActorId=a.actorId 
                    GROUP BY a.Name, a.surname, f.name 
                    ORDER BY filmuRating DESC";

        public static string SelectActorId(string name, string surname)
        {
            return @"
                SELECT [actorID] 
                FROM [actor]
                WHERE [name]='" + name + "' " +
                   "AND [surname]='" + surname + "'";
        }
        //TODO universal ActorsAndMovies query
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
    }
}
