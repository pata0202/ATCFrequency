using System;
using System.IO;
using System.Linq;
using System.Threading;
using Newtonsoft.Json;

class Program
{
    static void Main()
    {
        Console.WriteLine($"SDRSharp SNR Log 解析工具 v1.0.0\n" +
                          $"作者: 2025/04/08 by Pata");

        string currentPath = Directory.GetCurrentDirectory();
        string csvFilePath = Path.Combine(currentPath, "Files", "SDRSharp_SNR_Log.csv");
        string txtFreqFilePath = Path.Combine(currentPath, "Files", "currentFreq.txt");
        string txtNameFilePath = Path.Combine(currentPath, "Files", "currentName.txt");
        string jsonPath = Path.Combine(currentPath, "Files", "ChannelNames.json");
        Console.WriteLine("===================================");
        Console.WriteLine($"SDRSharp SNR Log 檔案位置請放在: {csvFilePath}");
        Console.WriteLine($"ChannelNames.json 檔案位置請放在: {jsonPath}");
        Console.WriteLine("===================================");
        Console.WriteLine(@$"最新頻率檔案: {txtFreqFilePath}");
        Console.WriteLine(@$"頻道名稱檔案: {txtNameFilePath}");
        Console.WriteLine("===================================");
        Console.WriteLine($"{DateTime.Now:yyyy/MM/dd HH:mm:ss} 正在執行...");

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
                            File.WriteAllText(txtFreqFilePath, $"{frequencyString} MHz"); // 顯示為 "125.1 MHz" 格式

                            bool scanning = false;
                            if (double.TryParse(lastColumns[2], out double snrSignal))
                            {
                                if (snrSignal >= GetSNRThreshold(jsonPath))
                                {
                                    scanning = true;
                                }
                                else
                                {
                                    scanning = false;
                                }
                            }
                            string channelName = GetChannelName(frequencyInMHz, scanning, jsonPath);
                            File.WriteAllText(txtNameFilePath, channelName); // 寫入頻道名稱
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

    static double GetSNRThreshold(string jsonPath)
    {
        // 讀取 JSON 檔案並解析 SNR 閾值
        string json = File.ReadAllText(jsonPath);
        var snrData = JsonConvert.DeserializeObject<ChannelData>(json);
        return snrData?.SNRThreshold ?? 0.0; // 預設為 0.0
    }

    static string GetChannelName(double frequencyInMHz, bool scanning, string jsonPath)
    {
        string channelName = frequencyInMHz.ToString();
        // 依照JSON取得對應的頻道名稱
        string json = File.ReadAllText(jsonPath);
        var channelData = JsonConvert.DeserializeObject<ChannelData>(json);
        if (scanning)
        {
            return channelData.ScanningText ?? channelName; // 如果正在掃描，則顯示掃描文字
        }
        else
        {
            channelName = channelData?.Channels.FirstOrDefault(f => f.Frequency == frequencyInMHz)?.Name ?? channelName;
            return channelName;
        }
    }
}
