namespace TM2Driver
{
    public enum RetrieveCommand
    {
        GetCh1PresentValue = 0x0403E8, // Current value of CH1
        GetCh2PresentValue = 0x0403EE, // Current value of CH2
        GetCh1Control = 0x030064, // Get On/Off status of CH1
        GetCh2Control = 0x03044C, // Get On/Off status of CH2
        GetCh1Value = 0x030000, // Get Target value of CH1
        GetCh2Value = 0x0303E8, // Get Target value of CH2
    }

    public enum UpdateCommand
    {
        SetCh1Control = 0x060064, // Set On/Off status of CH1 
        SetCh2Control = 0x06044C, // Set On/Off status of CH2
        SetCh1Value = 0x060000, // Set Target value of CH1
        SetCh2Value = 0x0603E8, // Set Target value of CH2
    }
}
