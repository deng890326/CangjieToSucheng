using System.Text;

namespace IniToCsv
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //const int outputFiles = 4;
            //const int linesPerFile = 500;
            //StreamWriter[] csvWriters = new StreamWriter[outputFiles];
            try
            {
                using FileStream iniStream = new("Phrases.ini", FileMode.Open, FileAccess.Read);
                using StreamReader iniReader = new(iniStream);

                //for (int i = 0; i <outputFiles; i++)
                //{
                //    FileStream csvStream = new($"Phrases{i}.csv", FileMode.Create, FileAccess.Write);
                //    csvWriters[i] = new(csvStream, Encoding.Unicode)
                //    {
                //        AutoFlush = true
                //    };
                //}
                using FileStream csvStream = new("Phrases.csv", FileMode.Create, FileAccess.Write);
                //using StreamWriter csvWriter = new(csvStream, Encoding.ASCII)
                using StreamWriter csvWriter = new(csvStream, Encoding.Unicode)
                {
                    AutoFlush = true
                };

                for (int i = 0; !iniReader.EndOfStream; i++)
                {
                    string? line = iniReader.ReadLine();
                    if (string.IsNullOrWhiteSpace(line) || line.StartsWith(';'))
                    {
                        continue;
                    }

                    StringReader lineReader = new(line);

                    StringBuilder code = new();
                    fun(code, ',');

                    StringBuilder order = new();
                    fun(order, '=');

                    StringBuilder text = new();
                    fun(text, -1);

                    void fun(StringBuilder builder, int split)
                    {
                        while (true)
                        {
                            int c = lineReader.Read();
                            if (c != split)
                            {
                                builder.Append((char)c);
                            }
                            else
                            {
                                break;
                            }
                        }
                    }

                    string scvLine = $"{code},{text},{order}";
                    //int page = i / linesPerFile;
                    //csvWriters[page].WriteLine(scvLine);
                    csvWriter.WriteLine(scvLine);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.GetType());
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
            finally
            {
                //foreach (var csvWriter in csvWriters)
                //{
                //    csvWriter.Close();
                //    csvWriter.Dispose();
                //}
            }
        }
    }
}