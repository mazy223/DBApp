using System;
using System.Diagnostics;
using System.IO;
using IniParser;
using IniParser.Model;
using Microsoft.Data.SqlClient;

class Program
{
    static void Main()
    { 

        string iniPath = "db_config.ini";
        var parser = new FileIniDataParser();
        IniData data = parser.ReadFile(iniPath);

        string server = data["Database"]["Server"];
        string database = data["Database"]["Database"];
        string user = data["Database"]["User"];
        string password = data["Database"]["Password"];

        // Localde sertifika olmadığı için connectionString'e TrustServerCertificate=true; ifadesi ekledim.
        // İsterseniz kaldırılabilir.
        string connectionString = $"Server={server};Database={database};User Id={user};Password={password};TrustServerCertificate=True;";


        string dbFilesPath = "db_files";  // SQL dosyalarının olduğu klasör
        string logFilePath = "logs.log";   // Log dosyası

        Console.WriteLine($"Conn string: {connectionString}");

        if (!Directory.Exists(dbFilesPath))
        {
            Console.WriteLine("Hata: 'db_files' klasörü bulunamadı!");
            return;
        }

        if (!File.Exists(logFilePath))
        {
            File.WriteAllText(logFilePath, "Logs:\n\n");
        }

        // Sadece .sql uzantılı dosyalar
        string[] sqlFiles = Directory.GetFiles(dbFilesPath, "*.sql");

        if (sqlFiles.Length == 0)
        {
            Console.WriteLine("Hata: 'db_files' klasöründe SQL dosyası bulunamadı!");
            return;
        }

        foreach (string sqlFile in sqlFiles)
        {
            ExecuteSqlFile(sqlFile, connectionString, logFilePath);
        }

        Console.WriteLine("Tüm SQL dosyaları çalıştırıldı,log dosyasını kontrol edin");
        Console.ReadKey();
    }

    static void ExecuteSqlFile(string filePath, string connectionString, string logFilePath)
    {
        try
        {
            string sqlQuery = File.ReadAllText(filePath);
            string fileName = Path.GetFileName(filePath);

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                {
                    command.ExecuteNonQuery();
                }
            }

            stopwatch.Stop();
            long elapsedTime = stopwatch.ElapsedMilliseconds;

            string logMessage = $"{fileName} {elapsedTime}ms";
            File.AppendAllText(logFilePath, logMessage + Environment.NewLine);

            Console.WriteLine(logMessage);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Hata: {filePath} çalıştırılırken hata oluştu -> {ex.Message}");
            Environment.Exit(1);

            //Console.WriteLine(ex.ToString());
        }
    }
}
