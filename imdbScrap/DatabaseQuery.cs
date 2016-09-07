using System;
using System.Data;
using System.Data.SqlServerCe;

namespace imdbScrap
{
    class DatabaseQuery
    {

        public static void AddActorToDb(String name, String surname, String born, SqlCeConnection connection)
        {
            try
            {
                string sql = "insert into actor(Name, Surname, Born) values(@name, @surname, @born)";
                SqlCeCommand cmd = new SqlCeCommand(sql, connection);
                cmd.Parameters.Add("@name", SqlDbType.NVarChar).Value = name;
                cmd.Parameters.Add("@surname", SqlDbType.NVarChar).Value = surname;
                cmd.Parameters.Add("@born", SqlDbType.NVarChar).Value = born;
                cmd.ExecuteNonQuery();
            }
            catch (Exception)
            {
                Console.WriteLine("Count not insert." + name + ", " + surname + ", " + born);
            }
        }

        public static void AddMovieToDb(String name, Decimal rating, String genre, String year, SqlCeConnection connection)
        {
            try
            {
                string sql = "insert into Film(Name, Rating, Genre, Year) values(@name, @rating, @genre, @year)";
                SqlCeCommand cmd = new SqlCeCommand(sql, connection);
                cmd.Parameters.Add("@name", SqlDbType.NVarChar).Value = name;
                cmd.Parameters.Add("@rating", SqlDbType.Decimal, 2).Value = rating;
                cmd.Parameters.Add("@genre", SqlDbType.NVarChar).Value = genre;
                cmd.Parameters.Add("@year", SqlDbType.NVarChar).Value = year;
                cmd.ExecuteNonQuery();
            }
            catch (Exception)
            {
                Console.WriteLine("Count not insert." + name + ", " + rating + "," + genre + "," + year);
            }

        }


        public static int SelectActorId(String name, String surname, SqlCeConnection connection)
        {
            int value = 0;
            try
            {
                using (
                    SqlCeCommand command =
                        new SqlCeCommand(
                            "SELECT [actorID] FROM [actor] where [name]='" + name + "' and [surname]='" + surname +
                            "'", connection))
                {
                    using (SqlCeDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                value = Convert.ToInt32(reader.GetValue(i));
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                Console.WriteLine("Could not select " + name + " " + surname);
            }
            return value;
        }


        public static int SelectFilmId(String name, SqlCeConnection connection)
        {
            int value = 0;
            try
            {
                using (SqlCeCommand command = new SqlCeCommand(
                    "SELECT [filmId] FROM [Film] where [name]=@name", connection))
                {
                    command.Parameters.Add(new SqlCeParameter("Name", name));
                    SqlCeDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        value = reader.GetInt32(0);

                    }
                    command.ExecuteNonQuery();

                }

            }
            catch (Exception exc)
            {
                Console.WriteLine("Could not select " + name);
            }
            return value;
        }

        public static void SelectActorFilmId(int actorId, int filmId, SqlCeConnection connection)
        {
            try
            {
                string sql = "insert into FilmaiToActor(actorId, filmId) values(@actorId, @filmId)";

                SqlCeCommand cmd = new SqlCeCommand(sql, connection);
                cmd.Parameters.Add("@actorId", SqlDbType.NVarChar).Value = actorId;
                cmd.Parameters.Add("@filmId", SqlDbType.NVarChar).Value = filmId;
                cmd.ExecuteNonQuery();
            }
            catch (Exception exc)
            {

                Console.WriteLine("Count not insert " + actorId + ", " + filmId);
            }
        }

        public static void GetActorWithMostFilms(SqlCeConnection connection)
        {
            try
            {
                using (SqlCeCommand command = new SqlCeCommand(
                    "select  a.Name,a.surname, count(f.name) as filmuSk " +
                    "from FilmaiToActor as fa " +
                    "inner join Film as f on fa.filmId = f.filmId " +
                    "inner join actor as a on fa.ActorId = a.actorId " +
                    "group by   a.Name, a.surname " +
                    "ORDER BY filmusk desc ", connection))
                {
                    SqlCeDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            Console.Write(reader.GetValue(i) + " | ");

                        }
                        Console.WriteLine();
                        break;

                    }

                    command.ExecuteNonQuery();

                }

            }
            catch (Exception exc)
            {
                Console.WriteLine("Could not select actor with most films");
            }
        }

        public static void GetFilmWithBiggestRating(SqlCeConnection connection)
        {
            try
            {
                using (SqlCeCommand command = new SqlCeCommand(
                    "select  a.Name,a.surname,f.name, max(f.rating) as filmuRating\r\n" +
                    "from FilmaiToActor as fa\r\n" +
                    "inner join Film as f on fa.filmId = f.filmId \r\n" +
                    "inner join actor as a on fa.ActorId=a.actorId \r\n" +
                    "group by   a.Name,a.surname,f.name \r\n" +
                    "ORDER BY filmuRating desc ", connection))
                {
                    SqlCeDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            Console.Write(reader.GetValue(i) + " | ");

                        }
                        Console.WriteLine();
                        break;

                    }

                    command.ExecuteNonQuery();

                }

            }
            catch (Exception exc)
            {
                Console.WriteLine("Could not select actor with most films");
            }
        }

        public static void ActorsAndMovies(string query, SqlCeConnection connection)
        {
            try
            {
                using (SqlCeCommand command = new SqlCeCommand(
                    query, connection))
                {
                    SqlCeDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            Console.Write(reader.GetValue(i) + " | ");

                        }
                        Console.WriteLine();

                    }

                    command.ExecuteNonQuery();

                }

            }
            catch (Exception exc)
            {
                Console.WriteLine("Could not select actors");
            }
        }
    }
}
