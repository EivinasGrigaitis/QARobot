using System;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlServerCe;

namespace QARobot
{
    class DatabaseQuery
    {
        private static readonly SqlCeConnection Connection = new SqlCeConnection(@"Data Source = database.sdf");
        public static SqlCeConnection Connect
        {
            get
            {
                if (Connection.State == ConnectionState.Closed)
                    Connection.Open();

                return Connection;
            }
        }


        public static void CloseConnections()
        {
            if (Connect.State == ConnectionState.Open)
                Connect.Close();
        }
        public static void AddActorToDb(String name, String surname, String born)
        {
            try
            {
                var sql = "INSERT INTO actor(Name, Surname, Born) VALUES(@name, @surname, @born)";
                var cmd = new SqlCeCommand(sql, Connect);
                cmd.Parameters.Add("@name", SqlDbType.NVarChar).Value = name;
                cmd.Parameters.Add("@surname", SqlDbType.NVarChar).Value = surname;
                cmd.Parameters.Add("@born", SqlDbType.NVarChar).Value = born;
                cmd.ExecuteNonQuery();
            }
            catch (Exception)
            {
                CloseConnections();
                Console.WriteLine("Count not insert." + name + ", " + surname + ", " + born);
            }
        }

        public static void AddMovieToDb(String name, Decimal rating, String genre, String year)
        {
            try
            {
                var sql = "INSERT INTO Film(Name, Rating, Genre, Year) VALUES(@name, @rating, @genre, @year)";
                var cmd = new SqlCeCommand(sql, Connect);
                cmd.Parameters.Add("@name", SqlDbType.NVarChar).Value = name;
                cmd.Parameters.Add("@rating", SqlDbType.Decimal, 2).Value = rating;
                cmd.Parameters.Add("@genre", SqlDbType.NVarChar).Value = genre;
                cmd.Parameters.Add("@year", SqlDbType.NVarChar).Value = year;
                cmd.ExecuteNonQuery();
            }
            catch (Exception)
            {
                CloseConnections();
                Console.WriteLine("Count not insert." + name + ", " + rating + "," + genre + "," + year);
            }
        }


        public static int SelectActorId(String name, String surname)
        {
            var value = 0;
            try
            {
                using (
                    var command =
                        new SqlCeCommand(
                            "SELECT [actorID] FROM [actor] WHERE [name]='" + name + "' AND [surname]='" + surname +
                            "'", Connect))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            for (var i = 0; i < reader.FieldCount; i++)
                            {
                                value = Convert.ToInt32(reader.GetValue(i));
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                CloseConnections();
                Console.WriteLine("Could not select " + name + " " + surname);
            }
            return value;
        }


        public static int SelectFilmId(string name)
        {
            var value = 0;
            try
            {
                using (var command = new SqlCeCommand(
                    "SELECT [filmId] FROM [Film] WHERE [name]=@name", Connect))
                {
                    command.Parameters.Add(new SqlCeParameter("Name", name));
                    var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        value = reader.GetInt32(0);
                    }
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception)
            {
                CloseConnections();
                Console.WriteLine("Could not select " + name);
            }
            return value;
        }

        public static void SelectActorFilmId(int actorId, int filmId)
        {
            try
            {
                var sql = "INSERT INTO FilmaiToActor(actorId, filmId) VALUES(@actorId, @filmId)";
                var cmd = new SqlCeCommand(sql, Connect);
                cmd.Parameters.Add("@actorId", SqlDbType.NVarChar).Value = actorId;
                cmd.Parameters.Add("@filmId", SqlDbType.NVarChar).Value = filmId;
                cmd.ExecuteNonQuery();
            }
            catch (Exception)
            {
                CloseConnections();
                Console.WriteLine("Count not insert " + actorId + ", " + filmId);
            }
        }

        public static void GetActorWithMostFilms()
        {
            try
            {
                using (var command = new SqlCeCommand(@"
                    SELECT  a.Name,a.surname, COUNT(f.name) AS filmuSk
                    FROM FilmaiToActor AS fa 
                    INNER JOIN Film AS f ON fa.filmId = f.filmId
                    INNER JOIN actor AS a ON fa.ActorId = a.actorId 
                    GROUP BY a.Name, a.surname
                    ORDER BY filmuSk DESC ", Connect))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            for (var i = 0; i < reader.FieldCount; i++)
                            {
                                Console.Write(reader.GetValue(i) + " | ");
                            }
                            Console.WriteLine();
                            break;
                        }
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception)
            {
                CloseConnections();
                Console.WriteLine("Could not select actor with most films");
            }
        }

        public static void GetFilmWithBiggestRating()
        {
            try
            {
                using (var command = new SqlCeCommand(@"
                    SELECT a.Name, a.surname, f.name, MAX(f.rating) AS filmuRating
                    FROM FilmaiToActor AS fa 
                    INNER JOIN Film AS f ON fa.filmId = f.filmId  
                    INNER JOIN actor AS a ON fa.ActorId=a.actorId 
                    GROUP BY a.Name, a.surname, f.name 
                    ORDER BY filmuRating DESC ", Connect))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            for (var i = 0; i < reader.FieldCount; i++)
                            {
                                Console.Write(reader.GetValue(i) + " | ");
                            }
                            Console.WriteLine();
                            break;
                        }
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception)
            {
                CloseConnections();
                Console.WriteLine("Could not select actor with most films");
            }
        }

        public static void ActorsAndMovies(string query)
        {
            try
            {
                using (var command = new SqlCeCommand(
                    query, Connect))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            for (var i = 0; i < reader.FieldCount; i++)
                            {
                                Console.Write(reader.GetValue(i) + " | ");
                            }
                            Console.WriteLine();
                        }
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception)
            {
                CloseConnections();
                Console.WriteLine("Could not select actors");
            }
        }
    }
}
