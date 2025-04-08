using System;
using System.IO;
using System.Linq;
using System.Threading;


class Program
{
    static void Main()
    {
        string csvFilePath = @"C:\Users\a2829\Desktop\ATC\SDRSharp_20250331_022457Z_SNR.csv";
        string txtFilePath = @"C:\Users\a2829\Desktop\ATC\Batch\currentFreq.txt";
        string txtNameFilePath = @"C:\Users\a2829\Desktop\ATC\Batch\currentName.txt";

        while (true)
        {
            try
            {
                string[] lines;
                while (true)
                {
                    try
                    {
                        lines = File.ReadAllLines(csvFilePath);
                        break;
                    }
                    catch (IOException)
                    {
                        Thread.Sleep(100); // 等待 100ms 再次嘗試
                    }
                }

                if (lines.Length > 1)
                {
                    // 取得最後一行並解析 Frequency 欄位
                    string[] lastColumns = lines.Last().Split(',');
                    if (lastColumns.Length > 1)
                    {
                        // 轉換為 MHz
                        if (double.TryParse(lastColumns[1], out double frequency))
                        {
                            double frequencyInMHz = frequency / 1000000;
                            string frequencyString = frequencyInMHz.ToString("F1"); // 格式化為小數點後一位
                            File.WriteAllText(txtFilePath, $"{frequencyString} MHz"); // 顯示為 "125.1 MHz" 格式
                            // string channelName = GetChannelName(frequencyInMHz);
                            // string channelName = lastColumns[0].Trim(); // 取得頻道名稱
                            // File.WriteAllText(txtNameFilePath, channelName); // 寫入頻道名稱
                        }
                        else
                        {
                            Console.WriteLine("無法解析頻率。");
                        }
                    }

                    int keepRow = 20;

                    // 保留最後 5 行 + 標題行
                    if (lines.Length > keepRow + 1)
                    {
                        File.WriteAllLines(csvFilePath, lines.Take(1).Concat(lines.Skip(lines.Length - keepRow)));
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"發生錯誤: {ex.Message}");
            }

            Thread.Sleep(500);
        }
    }
}
