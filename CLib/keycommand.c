#include <stdio.h>
#include <windows.h> 

/*
��������
len ���µļ�������
type 0 ����/һ�������� 1 ��ϼ�
keys �����int����ָ��
*/
void vkey_command(int len, int type, int *keys) {
	int i,key;
	INPUT *input = (INPUT *)malloc(sizeof(INPUT)*(len << 1));		//����������
	INPUT *addr = input;	//��¼input�׵�ַ
	//va_list np;	//���
	//va_start(np, type);
	for(i = 0; i < len; i++){
		//��������/һ��������
		//key = va_arg(np, int);
		key = *keys++;
		input->type = INPUT_KEYBOARD;
		input->ki.wVk = key;
		input->ki.dwFlags = KEYEVENTF_UNICODE;
		if(type == 0){
			input++;
			input->type = INPUT_KEYBOARD;
			input->ki.wVk = key;
			input->ki.dwFlags = KEYEVENTF_KEYUP | KEYEVENTF_UNICODE;
		}
		input++;
	}
	//��ϼ�, ȫ�����º���Ҫȫ��̧��
	if(type == 1){ 
		//va_start(np, type);
		for(i = 0; i < len; i++){
			//key = va_arg(np, int);
			key = *--keys;
			input->type = INPUT_KEYBOARD;
			input->ki.wVk = key;
			input->ki.dwFlags = KEYEVENTF_KEYUP | KEYEVENTF_UNICODE;
			input++;
		}
	}
	//��ָ������ָ����λ
	input=addr;
	//va_end(np);
	SendInput(len << 1, input, sizeof(INPUT));
	free(input);
	//obsolete
	//keybd_event(VK_VOLUME_UP, MapVirtualKey(VK_VOLUME_UP, 0), KEYEVENTF_EXTENDEDKEY, 0);
	//keybd_event(VK_VOLUME_UP, MapVirtualKey(VK_VOLUME_UP, 0), KEYEVENTF_KEYUP, 0);
}
