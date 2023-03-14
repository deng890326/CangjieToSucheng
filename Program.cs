using System.Text;

namespace CangjieToSucheng
{
    internal static class Program
    {
        static void Main(string[] args)
        {
            try
            {
                FileStream logStream = new(".\\log.txt", FileMode.Create, FileAccess.Write);
                using StreamWriter logWriter = new(logStream, Encoding.Unicode);
                Console.SetOut(logWriter);

                IEnumerable<char> mostUsedWords = GetMostUsedWords(".\\粤语常用字.txt",
                                                                   ".\\常用6763个汉字使用频率表.txt");
                ISet<char> mostUsedWordsSet = mostUsedWords.ToHashSet();
                using FileStream cjStream = new(".\\Cangjie5_SC.txt", FileMode.Open, FileAccess.Read);
                using StreamReader cjReader = new(cjStream);
                using FileStream scStream = new(".\\Sucheng_SC.txt", FileMode.Create, FileAccess.Write);
                using StreamWriter scWriter = new(scStream, Encoding.Unicode);

                using FileStream tbrStream = new(".\\To_Be_Removed.txt", FileMode.Open, FileAccess.Read);
                using StreamReader tbrReader = new(tbrStream);
                IEnumerable<string> tbrLines = GetLines(tbrReader).ToList();

                // 根据仓颉编码的txt文件生成速成编码
                List <(char word, string code)> scList = new();
                while (true)
                {   
                    // 开始处理一行仓颉编码
                    string? line = cjReader.ReadLine();
                    if (line is null or "")
                    {
                        break;
                    }
                    StringReader lineReader = new(line);

                    // 取得汉字
                    char word = (char)lineReader.Read();

                    // 跳过非字母
                    while (!char.IsLetter((char)lineReader.Peek()))
                    {
                        lineReader.Read();
                    }

                    // 取得仓颉码
                    StringBuilder codeBuilder = new();
                    while (true)
                    {
                        char next = (char)lineReader.Read();
                        if (next == -1) break;
                        if (char.IsLetter(next))
                        {
                            codeBuilder.Append(next);
                        }
                        else
                        {
                            break;
                        }
                    }

                    if (codeBuilder.Length == 0) //应该不会有空的code
                    {
                        Console.WriteLine($"Warning, 出现了空的codeBuilder");
                        continue;
                    }
                    // 仓颉码长度大于2的取首尾2码做速成码
                    if (codeBuilder.Length > 2)
                    {
                        codeBuilder.Remove(1, codeBuilder.Length - 2);
                    }

                    string scCode = codeBuilder.ToString();
                    scList.Add((word, scCode));
                }
                // 到此，scList中保存了所有的（字，速成码）

                //scList使用速成码作为key分组
                var lookup = scList.ToLookup(x => x.code);
                foreach (var itemGroup in lookup) //取得每一组
                {
                    // 取得一组中的速成码及对应的所有汉字
                    string code = itemGroup.Key;
                    // 跳过x和z开头的
                    if (code.StartsWith('x') || code.StartsWith('z'))
                    {
                        continue;
                    }
                    var words = (from item in itemGroup
                                select item.word)
                                .ToList();

                    // 先把每组的常用字先写入目标文件
                    foreach (char word in mostUsedWords)
                    {
                        if (words.Contains(word)) {
                            mostUsedWordsSet.Remove(word);
                            words.Remove(word);
                            writeCodeWord(scWriter, code, word);
                        }
                    }

                    // 再把剩下汉字的按照原顺序写入
                    foreach (char word in words)
                    {
                        Console.WriteLine($"编码{code}不在常用字里的字：{word}");
                        writeCodeWord(scWriter, code, word);
                    }

                    void writeCodeWord(StreamWriter scWriter, string code, char word)
                    {
                        string scline = $"{word}\t{code}";
                        // 跳过要删除的行
                        if (!tbrLines.Contains(scline))
                        {
                            scWriter.WriteLine(scline);
                        }
                    }
                }
                Console.WriteLine($"常用字列表剩余：{ToString(mostUsedWordsSet)}");
                scWriter.Flush();
                scWriter.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.GetType());
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }


        }

        private static IEnumerable<string> GetLines(StreamReader tbrReader)
        {
            while (!tbrReader.EndOfStream)
            {
                string? line = tbrReader.ReadLine();
                if (line == null) continue;
                yield return line;
            }
        }

        private static IEnumerable<char> GetMostUsedWords(string path1, string path2)
        {
            List<char> words = new();

            using StreamReader fileReader = new(path1);
            while (true)
            {
                string? line = fileReader.ReadLine();
                if (line is null or "")
                {
                    break;
                }
                foreach (char c in line)
                {
                    if (c is '（' or '）')
                    {
                        continue;
                    }
                    if (char.IsWhiteSpace(c))
                    {
                        break;
                    }
                    words.Add(c);
                }
            }
            Console.WriteLine($"所有的粤语字({words.Count})：{ToString(words)}");

            using StreamReader fileReader2 = new(path2);
            string? line2 = fileReader2.ReadLine() ?? "";
            foreach (char w in line2)
            {
                words.Add(w);
            }
            Console.WriteLine($"所有的字({words.Count})：{ToString(words)}");
            //IEnumerable<char> result = words.Distinct();
            //Console.WriteLine($"Distinct后所有的字({result.Count()})：{ToString(result)}");
            //return result;
            return words;
        }

        private static string ToString(this IEnumerable<char> words) 
        {
            StringBuilder sb = new();
            sb.AppendJoin(',', words);
            return sb.ToString();
        }
    }
}