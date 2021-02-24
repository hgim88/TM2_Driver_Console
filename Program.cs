using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace TM2Driver
{
    class Program
    {
        public const int ErrorFlag = 0x80;
        public const int AddressFieldIndex = 0;
        public const int FunctionFieldIndex = 1;
        public const int DataCountFieldIndex = 2;

        private readonly ModBusSerialDriver _commDriver;
        public Program()
        {
            _commDriver = new ModBusSerialDriver();
        }
        static async Task Main(string[] args)
        {
            var program = new Program();
            var driver = program._commDriver;

            driver.ChangeSerialPort("COM3", 9600);
            Console.WriteLine($"Try Connection... {driver.SerialPort.PortName} / {driver.SerialPort.BaudRate}");
            driver.Connect();

            while (!driver.IsConnected())
            {
                await Task.Delay(1500);
                Console.WriteLine($"Try Connection... {driver.SerialPort.PortName} / {driver.SerialPort.BaudRate}");
                driver.Connect();
            }

            while (true)
            {
                await program.ShowAllModuleState();
                await Task.Delay(300);
            }
        }

        private async Task ShowAllModuleState()
        {
            Console.Write($"{ModuleAddress.Addr1.ToString()}({ModuleChannel.CH1.ToString()}) : "
                        + await GetRunningStateAsync(ModuleAddress.Addr1, ModuleChannel.CH1) + "\t");
            Console.Write($"{ModuleAddress.Addr1.ToString()}({ModuleChannel.CH2.ToString()}) : "
                        + await GetRunningStateAsync(ModuleAddress.Addr1, ModuleChannel.CH1) + "\t");
            Console.Write($"{ModuleAddress.Addr2.ToString()}({ModuleChannel.CH1.ToString()}) : "
                        + await GetRunningStateAsync(ModuleAddress.Addr1, ModuleChannel.CH1) + "\t");
            Console.Write($"{ModuleAddress.Addr2.ToString()}({ModuleChannel.CH2.ToString()}) : "
                        + await GetRunningStateAsync(ModuleAddress.Addr1, ModuleChannel.CH1) + "\t");
            Console.Write($"{ModuleAddress.Addr3.ToString()}({ModuleChannel.CH1.ToString()}) : "
                        + await GetRunningStateAsync(ModuleAddress.Addr1, ModuleChannel.CH1) + "\t");
            Console.Write($"{ModuleAddress.Addr3.ToString()}({ModuleChannel.CH2.ToString()}) : "
                        + await GetRunningStateAsync(ModuleAddress.Addr1, ModuleChannel.CH1) + "\t");
            Console.Write($"{ModuleAddress.Addr4.ToString()}({ModuleChannel.CH1.ToString()}) : "
                        + await GetRunningStateAsync(ModuleAddress.Addr1, ModuleChannel.CH1) + "\t");
            Console.Write($"{ModuleAddress.Addr4.ToString()}({ModuleChannel.CH2.ToString()}) : "
                        + await GetRunningStateAsync(ModuleAddress.Addr1, ModuleChannel.CH1) + "\t");
            Console.WriteLine();
        }

        public async Task<bool> GetRunningStateAsync(ModuleAddress module, ModuleChannel channel)
        {
            byte[] requset;
            if (channel == ModuleChannel.CH1)
            {
                requset = MakeRetriveDataFrame((byte)module, RetrieveCommand.GetCh1Control);
            }
            else if (channel == ModuleChannel.CH2)
            {
                requset = MakeRetriveDataFrame((byte)module, RetrieveCommand.GetCh2Control);
            }
            else
            {
                throw new InvalidOperationException();
            }
            var response = await _commDriver.SendByteAsync(requset);

            var address = response[AddressFieldIndex];
            var functionCode = response[FunctionFieldIndex];
            // Error Check
            if ((functionCode & ErrorFlag) == 1)
            {
                var errorCode = (ErrorCodes)response[3];
                throw new Exception($"Error Code: {errorCode.ToString()}");
            }
            else
            {
                var isRunning = ExtractDataFromDataFrame(response) == 0;
                return isRunning;
            }
        }

        private static byte[] MakeRetriveDataFrame(byte address, RetrieveCommand command)
        {
            var dataFrame = new List<byte>();
            dataFrame.Add(address);

            var commandCodes = BitConverter.GetBytes((int)command);
            dataFrame.Add(commandCodes[2]); // Function Code
            dataFrame.Add(commandCodes[1]); // Register Address (High)
            dataFrame.Add(commandCodes[0]); // Register Address (Low)

            dataFrame.Add(0x00); // Register Count (High)
            dataFrame.Add(0x01); // Register Count (Low)

            var crc = CRC16.CalculateCRC(dataFrame.ToArray());
            dataFrame.AddRange(crc);

            return dataFrame.ToArray();
        }

        private static byte[] MakeUpdateDataFrame(byte address, UpdateCommand command, ushort data)
        {
            var dataFrame = new List<byte>();
            dataFrame.Add(address);

            var commandCodes = BitConverter.GetBytes((int)command);
            dataFrame.Add(commandCodes[2]); // Function Code
            dataFrame.Add(commandCodes[1]); // Register Address (High)
            dataFrame.Add(commandCodes[0]); // Register Address (Low)

            var datas = BitConverter.GetBytes(data);
            dataFrame.Add(datas[1]); // data (High)
            dataFrame.Add(datas[0]); // data (Low)

            var crc = CRC16.CalculateCRC(dataFrame.ToArray());
            dataFrame.AddRange(crc);

            return dataFrame.ToArray();
        }

        private static ushort ExtractDataFromDataFrame(byte[] dataFrame)
        {
            var temp = new List<byte>();
            var countOfDateFields = dataFrame[DataCountFieldIndex];
            for (int i = 0; i < countOfDateFields; i++)
            {
                temp.Add(dataFrame[(DataCountFieldIndex + 1) + i]);
            }
            temp.Reverse();
            return BitConverter.ToUInt16(temp.ToArray(), 0);
        }
    }
}
