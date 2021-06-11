#include <Windows.h>
#include "socketmgmt.h"
#include <stdio.h>
/*
启动socket连接线程
*/
void init() 
{
	CreateThread(NULL, 0, init_socket, NULL, 0, NULL);
}


void main() 
{
	init();
	while (1);
}