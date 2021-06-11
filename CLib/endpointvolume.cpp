//#include <windows.h> 
#include <mmdeviceapi.h> 
#include <endpointvolume.h>
//#include <audioclient.h>
#include <iostream>

//����:
//    -2 ��ȡ������Ϣ
//    -1 ����
//    0~100:��������
//		return 1: �����ɹ�/�Ǿ���, 0 ����, -1 �쳣
int VolumeLevelControl(int level)
{
	int result = -1;
    HRESULT hr;
    IMMDeviceEnumerator* pDeviceEnumerator=0;
    IMMDevice* pDevice=0;
    IAudioEndpointVolume* pAudioEndpointVolume=0;
    //IAudioClient* pAudioClient=0;

    try{
        hr = CoCreateInstance(__uuidof(MMDeviceEnumerator),NULL,CLSCTX_ALL,__uuidof(IMMDeviceEnumerator),(void**)&pDeviceEnumerator);
        if(FAILED(hr)) throw "CoCreateInstance";
        hr = pDeviceEnumerator->GetDefaultAudioEndpoint(eRender,eMultimedia,&pDevice);
        if(FAILED(hr)) throw "GetDefaultAudioEndpoint";
        hr = pDevice->Activate(__uuidof(IAudioEndpointVolume),CLSCTX_ALL,NULL,(void**)&pAudioEndpointVolume);
        if(FAILED(hr)) throw "pDevice->Active";
        //hr = pDevice->Activate(__uuidof(IAudioClient),CLSCTX_ALL,NULL,(void**)&pAudioClient);
        //if(FAILED(hr)) throw "pDevice->Active";
		
		
        if(level==-2){
			float fpLevel;
			float fVolume;
            hr = pAudioEndpointVolume->GetMasterVolumeLevelScalar(&fpLevel);
            if(FAILED(hr)) throw "GetMasterVolumeLevelScalar";
            fVolume = fpLevel * 100.0f;
			result = fVolume;
        }else if(level==-1){
			BOOL isMute = 0;
			pAudioEndpointVolume->GetMute(&isMute);
            hr = pAudioEndpointVolume->SetMute(!isMute,NULL);
            if(FAILED(hr)) throw "SetMute";
        }else{		
            if(level<0 || level>100){
                hr = E_INVALIDARG;
                throw "Invalid Arg";
            }

            float fVolume;
            fVolume = level/100.0f;
            hr = pAudioEndpointVolume->SetMasterVolumeLevelScalar(fVolume,&GUID_NULL);
            if(FAILED(hr)) throw "SetMasterVolumeLevelScalar";
            //pAudioClient->Release();
            pAudioEndpointVolume->Release();
            pDevice->Release();
            pDeviceEnumerator->Release();
			result = 1;
            return result;
        }
    }
    catch(...){
        //if(pAudioClient) pAudioClient->Release();
        if(pAudioEndpointVolume) pAudioEndpointVolume->Release();
        if(pDevice) pDevice->Release();
        if(pDeviceEnumerator) pDeviceEnumerator->Release();
        throw;
    }
    return result;
}

//create interface for C
#ifdef __cplusplus
extern "C"{
#endif
int setSysVol(int level){
	int b;
	CoInitialize(0);
	try{
		b = VolumeLevelControl(level);
	}
	catch(...){
	}
	CoUninitialize();
	return b;
}
#ifdef __cplusplus
}
#endif