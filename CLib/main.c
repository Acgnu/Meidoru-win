#include <Windows.h>
#include "socketmgmt.h"
#include <stdio.h>
/*
����socket�����߳�
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