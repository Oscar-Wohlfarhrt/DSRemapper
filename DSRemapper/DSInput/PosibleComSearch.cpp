//COM port Search with SetupApi:

#pragma comment (lib, "Setupapi.lib")
#include <iostream>
#include <Windows.h>
#include <initguid.h>
#include <ntddstor.h>
#include <Setupapi.h>
#include <cfgmgr32.h>
#include <tchar.h>
#include <winreg.h>

using namespace std;

int main()
{
	GUID ComGuid = GUID_DEVINTERFACE_COMPORT;

	HDEVINFO devInfo = SetupDiGetClassDevsW(&ComGuid, NULL, 0, DIGCF_PRESENT | DIGCF_DEVICEINTERFACE);

	SP_DEVINFO_DATA devInfoData;
	devInfoData.cbSize = sizeof(SP_DEVINFO_DATA);
	DWORD devIndex = 0;

	while (SetupDiEnumDeviceInfo(devInfo, devIndex, &devInfoData)) {
		devIndex++;
		SP_DEVICE_INTERFACE_DATA interfaceData;
		interfaceData.cbSize = sizeof(SP_DEVICE_INTERFACE_DATA);
		DWORD intIndex = 0;

		HKEY hKey = SetupDiOpenDevRegKey(devInfo, &devInfoData, DICS_FLAG_GLOBAL, 0, DIREG_DEV, KEY_READ);

		if (hKey!=INVALID_HANDLE_VALUE) {
			WCHAR valQuery[] = L"PortName";
			WCHAR strKey[100];
			DWORD len = 40;

			if (!RegQueryValueExW(hKey, valQuery, NULL, NULL, (LPBYTE)strKey, &len)) {
				wcout << strKey;
			}

			RegCloseKey(hKey);
		}

		while (SetupDiEnumDeviceInterfaces(devInfo, &devInfoData, &ComGuid, intIndex, &interfaceData)) {
			intIndex++;

			DWORD bufSize = 0;
			SP_DEVICE_INTERFACE_DETAIL_DATA_W detailInterData;
			detailInterData.cbSize = sizeof(SP_DEVICE_INTERFACE_DETAIL_DATA_W);
			SetupDiGetDeviceInterfaceDetailW(devInfo, &interfaceData, NULL, 0, &bufSize, NULL);
			
			DWORD keyBufSize = 100, type = 0, requiredSize = 0;
			WCHAR strKey[1024];

			SetupDiGetDeviceRegistryPropertyW(devInfo, &devInfoData, SPDRP_FRIENDLYNAME, &type, (PBYTE)strKey, keyBufSize, &requiredSize);

			wcout <<L" | Device: " << strKey << endl;
		}
		wcout << endl;
	}
	SetupDiDestroyDeviceInfoList(devInfo);

	return 0;
}