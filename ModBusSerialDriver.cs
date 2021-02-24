using System.Collections.Generic;

namespace TM2Driver
{
    public class ModBusSerialDriver : SyncSerialDriver<byte[]>
    {
        public override bool ExtractData(byte[] buf, out byte[] receivedData)
        {
            // 데이터 검사에 필요한 최소 사이즈 검사
            if (buf.Length < 3)
            {
                receivedData = null;
                return false;
            }

            var functionCode = buf[1];

            // 데이터 에러 검사 및 데이터 사이즈 계산
            int dataFrameLength;
            if ((functionCode & 0x80) == 0)
            {
                // 에러가 없을 때
                // 데이터 프레임 사이즈 계산
                if (functionCode == 0x05 || functionCode == 0x06)
                {
                    // Write function 의 경우, 응답 데이터 프레임에 data length가 없이 총 8바이트 이다.
                    dataFrameLength = 8;
                }
                else
                {
                    // Read 명령의 경우, 응답 데이터 프레임에 data length가 포함되어 있다.
                    // data size 외의 데이터는 총 5바이트가 있으므로 더해서 사용한다.
                    dataFrameLength = buf[2] + 5;
                }
            }
            else
            {
                // 에러가 있을 때
                // 데이터 프레임 사이즈 계산
                // Error의 경우 데이터는 총 5바이트 이다.
                dataFrameLength = 5;
            }

            // 데이터 프레임 사이즈 검사
            if (buf.Length < dataFrameLength)
            {
                receivedData = null;
                return false;
            }

            // 데이터 프레임 생성 for checking CRC16
            var dataFrameBuffer = new List<byte>();
            for (int i = 0; i < dataFrameLength; i++)
            {
                dataFrameBuffer.Add(buf[i]);
            }

            // CRC 검사
            if (!CRC16.CheckCRC(dataFrameBuffer))
            {
                // crc 에러
                // 재송수신 필요
                receivedData = null;
                return false;
            }

            receivedData = dataFrameBuffer.ToArray();
            return true;
        }

    }
}
