using System;
using System.Data;
using System.Reflection.PortableExecutable;
using MySql.Data.MySqlClient;

class Program
{
    static void Main()
    {
        // Methode wird aufgerufen um eine Verbindung zur DB herzustellen
        var sqlconnection = ConnectionAufbauen();

        bool weiterAbfragen = true;
        while (weiterAbfragen)
        {

            Console.WriteLine("Welchen Cocktail wollen Sie zubereiten?");
            string input = Console.ReadLine();

            bool cocktailExistiert = ÜberprüfungCocktaiLExistenz(sqlconnection, input);

            if (cocktailExistiert)
            {
                AbfrageVonCocktail(sqlconnection, input);
                AbfrageVonGlas(sqlconnection, input);
                AbfrageVonZutatenVonCocktail(sqlconnection, input);
            }
            
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Der eingegebene Cocktail kann nicht gefunden werden!");
                Console.ResetColor();
            }
            

            // Abfrage für weitere Datenbankabfragen
            Console.WriteLine("\n\nMöchten Sie einen weiteren Cocktail zubereiten? (Ja/Nein)");
            string weiterAbfragenAntwort = Console.ReadLine();
            // Wenn es kein ja oder ä. ist wird das Programm beendet
            weiterAbfragen = weiterAbfragenAntwort.Equals("Ja", StringComparison.OrdinalIgnoreCase)
                            || weiterAbfragenAntwort.Equals("ja", StringComparison.OrdinalIgnoreCase)
                            || weiterAbfragenAntwort.Equals("j", StringComparison.OrdinalIgnoreCase)
                            || weiterAbfragenAntwort.Equals("Yes", StringComparison.OrdinalIgnoreCase)
                            || weiterAbfragenAntwort.Equals("yes", StringComparison.OrdinalIgnoreCase)
                            || weiterAbfragenAntwort.Equals("y", StringComparison.OrdinalIgnoreCase);
        }

        // Verbindung zur Datenbank schließen
        sqlconnection.Close();
    }



    static void AbfrageVonCocktail(MySqlConnection connection, string input)
    {
        // SQL-Abfrage vorbereiten
        string query = @"SELECT *
                        FROM tblcocktail
                        WHERE Cocktail = @input";

        MySqlCommand command = new MySqlCommand(query, connection);
        command.Parameters.AddWithValue("@input", input);

        // Daten aus der Datenbank abrufen
        using (MySqlDataReader reader = command.ExecuteReader())
        {

            while (reader.Read())
            {
                // Daten werden aus der Datenbank ausgelesen
                string cocktailName = reader.GetString("Cocktail");
                string zubereitung = reader.GetString("Zubereitung");
                object bemerkungObject = reader["Bemerkung"];
                string bemerkung = "";

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("\n\nCocktail:".ToUpper());
                Console.ResetColor();
                Console.WriteLine(cocktailName);
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("\nZubereitung:".ToUpper());
                Console.ResetColor();
                Console.WriteLine(zubereitung);



                // Überprüfen, ob eine Bemerkung vorhanden ist
                if (bemerkungObject != DBNull.Value)
                {
                    bemerkung = (string)bemerkungObject;
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("\nBemerkung:".ToUpper());
                    Console.ResetColor();
                    Console.WriteLine(bemerkung);
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("\nBemerkung:".ToUpper());
                    Console.ResetColor();
                    Console.WriteLine("Es ist keine Bemerkung vorhanden!");
                }

            }
        }
    }

    static void AbfrageVonGlas(MySqlConnection connection, string input)
    {
        // SQL-Abfrage vorbereiten
        string query = @"SELECT *
                        FROM tblglas
                        INNER JOIN tblcocktail ON tblcocktail.GlasNr = tblglas.GlasNr
                        WHERE Cocktail = @input";

        MySqlCommand command = new MySqlCommand(query, connection);
        command.Parameters.AddWithValue("@input", input);

        // Daten aus der Datenbank abrufen
        using (MySqlDataReader reader = command.ExecuteReader())
        {
            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    // Daten werden aus der Datenbank ausgelesen
                    string glas = reader.GetString("Glas");

                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("\nGlas:".ToUpper());
                    Console.ResetColor();
                    Console.WriteLine(glas);
                }
            }
            else
            {
                // wenn kein Glas in der Datenbank vorhanden ist
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("\nGlas:".ToUpper());
                Console.ResetColor();
                Console.WriteLine("Wähle Sie ein Glas Ihrer Wahl!");
            }
        }

    }
    static void AbfrageVonZutatenVonCocktail(MySqlConnection connection, string input)
    {
        // SQL-Abfrage vorbereiten
        string query = @"SELECT z.Zutat, cz.Menge, e.Einheit
                                FROM tblcocktail c
                                INNER JOIN tblcocktailzutaten cz ON c.CocktailNr = cz.CocktailNr
                                INNER JOIN tblzutat z ON cz.ZutatenNr = z.ZutatenNr
                                INNER JOIN tbleinheiten e ON cz.EinheitenNr = e.EinheitenNr
                                WHERE c.Cocktail = @input";

        MySqlCommand command = new MySqlCommand(zweitequery, connection);
        command.Parameters.AddWithValue("@input", input);


        // die Werte werden in Listen eingetragen, da es mehr als ein Wert sein kann
        List<string> zutaten = new List<string>();
        List<int> mengen = new List<int>();
        List<string> einheiten = new List<string>();

        using (MySqlDataReader reader = command.ExecuteReader())
        {
            while (reader.Read())
            {
                string zutat = reader.GetString("Zutat");
                int menge = reader.GetInt16("Menge");
                string einheit = reader.GetString("Einheit");

                zutaten.Add(zutat);
                mengen.Add(menge);
                einheiten.Add(einheit);
            }
        }


        int maxLength = Math.Max(Math.Max(zutaten.Count, mengen.Count), einheiten.Count);
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\nZutaten:".ToUpper());
        Console.ResetColor();

        // Werte aus der Liste werden ausgegeben
        for (int i = 0; i < maxLength; i++)
        {
            if (i < zutaten.Count)
            {
                Console.Write(zutaten[i] + " ");
            }

            if (i < mengen.Count)
            {
                Console.Write(mengen[i] + "");
            }

            if (i < einheiten.Count)
            {
                Console.Write(einheiten[i] + "\n");
            }
        }
    }

    static MySqlConnection ConnectionAufbauen()
    {
        // Verbindungszeichenfolge zur Datenbank (ggfs. User und PW anpassen)
        string connectionString = "Server=127.0.0.1;Database=cocktail;Uid=read;Pwd=onlyread";

        MySqlConnection connection = new MySqlConnection(connectionString);
        {
            connection.Open();

            return connection;
        }
    }

    static bool ÜberprüfungCocktaiLExistenz(MySqlConnection connection, string input)
    {
        // SQL-Abfrage vorbereiten
        string query = @"SELECT COUNT(*) 
                       FROM tblcocktail 
                       WHERE Cocktail = @input";

        MySqlCommand command = new MySqlCommand(query, connection);
        command.Parameters.AddWithValue("@input", input);

        // Überprüft ob in der Datenbank der Cocktail vorhanden ist
        int count = Convert.ToInt32(command.ExecuteScalar());

        return count > 0;
    }
}



