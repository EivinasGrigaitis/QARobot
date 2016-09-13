using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace QARobot
{
    class SqlQueries
    {
        public static Dictionary<string, string> ChoosenActorsDictionary = new Dictionary<string, string>();
        public static List<Actor> actorObjList = new List<Actor>();
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

        public static string UniversalString(List<Actor> actorList = null)
        {
            if (actorList == null) actorList = actorObjList;
            var cmd = new SqlCommand();
            var sqlBuilder = new StringBuilder();
            sqlBuilder.Append("SELECT  f.name, f.year, f.rating  " +
                              "FROM FilmaiToActor AS fa " +
                              "INNER JOIN Film AS f ON fa.filmId = f.filmId " +
                              "INNER JOIN actor AS a ON fa.ActorId=a.actorId ");
            if (actorList.Count > 1)
            {
                var i = 1;
                foreach (var item in actorList)
                {
                    sqlBuilder.Append(i == 1 ? " WHERE " : " OR ");
                    var paramName = item.Name;
                    var paramSurname = item.Surname;
                    sqlBuilder.AppendFormat("(a.Name ='{0}' AND a.Surname = '{1}' )", paramName, paramSurname);
                    cmd.Parameters.AddWithValue(paramName, "%" + item + "%");
                    cmd.Parameters.AddWithValue(paramSurname, "%" + item + "%");
                    i++;
                }
            }

            return cmd.CommandText = sqlBuilder + "GROUP BY  f.name,f.year, f.rating " +
                                     "HAVING COUNT(*) >=" + actorList.Count;
        }

        public static string CoStarMethod()
        {
            ActorsList = actorObjList.Select(a => a.Name + " " + a.Surname).ToList();
            
            Console.WriteLine("Choose Co-star actors :");
            for (int i = 0; i < actorObjList.Count; i++)
            {
                Console.WriteLine("Actor - " + actorObjList[i].Fullname + " Actor Index - " + i);
            }

            var chosenActors = new List<Actor>();
            int quantity = -1;
            while (!(quantity > 0 && quantity <= actorObjList.Count))
            {
                Console.WriteLine($"\r\n How many actors you would like to Co-Star? (No more than {actorObjList.Count})");
                var readQuantity = Console.ReadKey().KeyChar.ToString();
                int.TryParse(readQuantity, out quantity);
            }
                
            for (var i = 0; i < quantity; i++)
            {
                Console.WriteLine($"\r\nPlease enter index of actor #{i + 1}: ");
                try
                {
                    var actorIndex = Convert.ToInt32(Console.ReadLine());
                    chosenActors.Add(actorObjList[actorIndex]);
                }
                catch (Exception)
                {
                    i--;
                }
            }
            return UniversalString(chosenActors);
        }

        /// <summary>
        /// Inclusive int.parse(string) check in boundary [lower, upper]
        /// </summary>
        /// <returns>True if in boundary, false if not (or if incorrect string supplied)</returns>
        static public bool isStringIntRange(string intString, int lower, int upper)
        {
            try
            {
                var integer = Int32.Parse(intString);
                if (integer >= lower && integer <= upper) return true;
                return false;
            }
            catch (Exception e)
            {
                //Debug
                Console.WriteLine($"Could not convert string to integer: {e}");
                return false;
            }
        }
    }
}
