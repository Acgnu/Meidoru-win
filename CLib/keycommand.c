#include <stdio.h>
#include <windows.h> 

/*
按键按下
len 按下的键的数量
type 0 单个/一连串按件 1 组合键
keys 键码的int数组指针
*/
void vkey_command(int len, int type, int *keys) {
	int i,key;
	INPUT *input = (INPUT *)malloc(sizeof(INPUT)*(len << 1));		//按件输入流
	INPUT *addr = input;	//记录input首地址
	//va_list np;	//变参
	//va_start(np, type);
	for(i = 0; i < len; i++){
		//单个按键/一连串按件
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
	//组合键, 全部按下后，需要全部抬起
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
	//将指针重新指向首位
	input=addr;
	//va_end(np);
	SendInput(len << 1, input, sizeof(INPUT));
	free(input);
	//obsolete
	//keybd_event(VK_VOLUME_UP, MapVirtualKey(VK_VOLUME_UP, 0), KEYEVENTF_EXTENDEDKEY, 0);
	//keybd_event(VK_VOLUME_UP, MapVirtualKey(VK_VOLUME_UP, 0), KEYEVENTF_KEYUP, 0);
}
