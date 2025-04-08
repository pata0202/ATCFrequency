public class ChannelData
{
    public double SNRThreshold { get; set; } // SNR 閾值
    public string ScanningText { get; set; }
    public List<Channels> Channels { get; set; }
}

public class Channels
{
    public string Name { get; set; }
    public double Frequency { get; set; } // 頻率以 MHz 為單位
}
