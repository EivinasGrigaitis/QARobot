using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace QARobot
{
    class SqlQueries
    {
        public static Dictionary<string, string> choosenActorsDictionary = new Dictionary<string, string>();
        public static Dictionary<string, string> Dict = new Dictionary<string, string>();
        public static List<string> ActorsList;
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

        public static string UniversalString(Dictionary<string, string> dict)
        {
            var cmd = new SqlCommand();
            var sqlBuilder = new StringBuilder();
            sqlBuilder.Append("SELECT  f.name, f.year, f.rating  " +
                              "FROM FilmaiToActor AS fa " +
                              "INNER JOIN Film AS f ON fa.filmId = f.filmId " +
                              "INNER JOIN actor AS a ON fa.ActorId=a.actorId ");
            if (dict.Keys.Count >1)
            {
                var i = 1;
                foreach (var item in dict.Keys)
                {
                    sqlBuilder.Append(i == 1 ? " WHERE " : " OR ");
                    var paramName = item.Split(' ')[0];
                    var paramSurname = item.Split(' ')[1];
                    sqlBuilder.AppendFormat("(a.Name ='{0}' AND a.Surname = '{1}' )", paramName, paramSurname);
                    cmd.Parameters.AddWithValue(paramName, "%" + item + "%");
                    cmd.Parameters.AddWithValue(paramSurname, "%" + item + "%");
                    i++;
                }
            }

            return cmd.CommandText = sqlBuilder + "GROUP BY  f.name,f.year, f.rating " +
                                     "HAVING COUNT(*) >1";
        }

        public static string CoStarMethod()
        {
            ActorsList = Dict.Keys.ToList();
            Console.WriteLine("NONSENSE TEST");
            foreach (var actor in ActorsList)
            {
                int index = ActorsList.IndexOf(actor);
                Console.WriteLine("Actor - " + actor + " Actor Index - " + index);
            }
            Console.WriteLine("\r\nHow many actors would you like to scrape?");

            var readQuantity = Console.ReadKey().KeyChar.ToString();
            int quantity;
            int.TryParse(readQuantity, out quantity);
            for (var i = 0; i < quantity; i++)
            {
                Console.WriteLine($"\r\nPlease enter index of actor #{i + 1}: ");
                var actor = Console.ReadLine();
                try
                {
                    if (ActorsList.Contains(ActorsList[Convert.ToInt32(actor)]))
                    {
                        choosenActorsDictionary.Add(ActorsList[Convert.ToInt32(actor)], quantity.ToString());
                    }



                }
                catch (Exception)
                {

                    i--;
                }
            }
            return UniversalString(choosenActorsDictionary);
        }




    }
}
