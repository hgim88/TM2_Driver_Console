
namespace TM2Driver
{

    public enum ErrorCodes
    {
        IllegalFunction = 0x01,     // 지원하지 않는 명령 에러
        IllegalDataAddress = 0x02,  // 요청 데이터 시작 번지 에러
        IllegalDataValue = 0x03,    // 요청 데이터 개수 에러
        SlaveDeviceFailure = 0x04   // 요청 명령 처리 불능 에러
    }
}
