
#include <Windows.h>
#include <stdio.h>
#include "cJSON.h"

extern void vkey_command(int len, int type, int *keys);
int setSysVol(int level);		//defined in cpplib/endpointvolumn.cpp


int setPCVol(cJSON *data){
	cJSON *vol = cJSON_GetObjectItem(data, "vol");
	return setSysVol(vol->valueint);
}

void excuteDOS(cJSON *data){
	cJSON *cmd = cJSON_GetObjectItem(data, "cmd");
	system(cmd->valuestring);
}

void execKey(cJSON *data){
	int i;
	int size = 0;
	int *keys;

	cJSON *type = cJSON_GetObjectItem(data, "type");
	cJSON *dataArr = cJSON_GetObjectItem(data, "keys");
	size = cJSON_GetArraySize(dataArr);
	keys = (int *)malloc(sizeof(int) * size);
	for(i = 0; i < size; i++){
		*keys = cJSON_GetArrayItem(dataArr, i)->valueint;
		keys++;
	}
	keys -= size;
	vkey_command(size, type->valueint, keys);
	free(keys);
}
char *load_sys_info(){
	char *v;
	cJSON *info = cJSON_CreateObject();
	cJSON *code = cJSON_CreateNumber(1004);
	cJSON *sysvol = cJSON_CreateNumber(setSysVol(-2));
	cJSON_AddItemToObject(info, "code", code);
	cJSON_AddItemToObject(info, "sysvol", sysvol);
	v = cJSON_PrintUnformatted(info);
	cJSON_Delete(info);
	return v;
}
/*
执行指令代码
*/
char *do_command(int code, char *msg, cJSON *data, char *result)
{
	char *tmp;
	printf("指令代码：%d\n",code);
	switch (code)
	{
	case 1001:
		execKey(data);
		break;
	case 1002:
		setPCVol(data);
		break;
	case 1003:
		excuteDOS(data);
		break;
	case 1004:
		tmp = load_sys_info();
		strcpy_s(result, 1000, tmp);
		strcat_s(result, 1000, "\n");
		free(tmp);
	default:
		break;
	}
	return result;
}

/*
解析收到的数据
*/
char *msg_parser(char *command, char *result){
	cJSON *command_JSON = cJSON_Parse(command);

	puts(command);
	if (NULL == command_JSON)
	{
		cJSON_Delete(command_JSON);
	}
	else 
	{
		cJSON *code = cJSON_GetObjectItem(command_JSON, "code");
		cJSON *msg = cJSON_GetObjectItem(command_JSON, "msg");
		cJSON *data = cJSON_GetObjectItem(command_JSON, "data");

		result = do_command(code->valueint, msg->valuestring, data, result);
		cJSON_Delete(command_JSON);
	}
	return result;
}
