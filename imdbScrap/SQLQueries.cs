using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace QARobot
{
    class SqlQueries
    {
        public static Dictionary<string, string> Dict = new Dictionary<string, string>();
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

        public static string UniversalString()
        {
            var cmd = new SqlCommand();
            var sqlBuilder = new StringBuilder();
            sqlBuilder.Append("SELECT  f.name, f.year, f.rating  " +
                              "FROM FilmaiToActor AS fa " +
                              "INNER JOIN Film AS f ON fa.filmId = f.filmId " +
                              "INNER JOIN actor AS a ON fa.ActorId=a.actorId ");

            var i = 1;
            foreach (var item in Dict.Keys)
            {
                sqlBuilder.Append(i == 1 ? " WHERE " : " OR ");
                var paramName =  item.Split(' ')[0];
                var paramSurname = item.Split(' ')[1];
                sqlBuilder.AppendFormat("(a.Name ='{0}' AND a.Surname = '{1}' )", paramName, paramSurname);
                cmd.Parameters.AddWithValue(paramName, "%" + item + "%");
                cmd.Parameters.AddWithValue(paramSurname, "%" + item + "%");
                i++;
            }
            return cmd.CommandText = sqlBuilder + "GROUP BY  f.name,f.year, f.rating " +
                                     "HAVING COUNT(*) >1";
     }
        
    }
}
