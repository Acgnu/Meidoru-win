#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <winSock2.h>

#pragma comment(lib , "Ws2_32.lib")

#define PORT 50003
char *IP_ADDR;

extern char *msg_parser(char *command, char *result);

void CheckIP(void) //定义CheckIP（）函数，用于获取本机IP地址 
{
	WSADATA wsaData;
	char name[255];//定义用于存放获得的主机名的变量 
	char *ip;//定义IP地址变量 
	PHOSTENT hostinfo; 

	//调用MAKEWORD（）获得Winsock版本的正确值，用于加载Winsock库 
	if ( WSAStartup( MAKEWORD(2,0), &wsaData ) == 0 ) { 
		//现在是加载Winsock库，如果WSAStartup（）函数返回值为0，说明加载成功，程序可以继续 
		if( gethostname ( name, sizeof(name)) == 0) { 
			//如果成功地将本地主机名存放入由name参数指定的缓冲区中 
			if((hostinfo = gethostbyname(name)) != NULL) { 
				//这是获取主机名，如果获得主机名成功的话，将返回一个指针，指向hostinfo，hostinfo 
				//为PHOSTENT型的变量，下面即将用到这个结构体 
				ip = inet_ntoa (*(struct in_addr *)*hostinfo->h_addr_list); 
				//调用inet_ntoa（）函数，将hostinfo结构变量中的h_addr_list转化为标准的点分表示的IP 
				//地址（如192.168.0.1） 
				IP_ADDR = ip;
				//printf("%s\n",ip);//输出IP地址
			} 
		} 
		WSACleanup( );//卸载Winsock库，并释放所有资源 
	}
} 

/*
开始监听一个socket
*/
DWORD WINAPI start_listen(LPVOID pM) {
	char msg[1000];
	char tosend[1000] = "down\n";
	SOCKET connectingSock = (SOCKET)pM;
	memset(msg, 0, 1000);
	//memset(tosend, 0, 1000);
	while (1) 
	{
		if (recv(connectingSock, msg, 999, 0) <= 0)
		{
			puts("接收消息失败");
			break;
		}
		else {
			msg_parser(msg, tosend);
		}
		//sprintf(tosend);
		sprintf(msg, tosend);
		//free(tosend);
		if (send(connectingSock, msg, strlen(msg), 0) <= 0)
		{
			puts("发送消息失败");
		}
		else {
			puts("发送消息成功");
		}
	}
	closesocket(connectingSock);
}

/*
启动socket
*/
DWORD WINAPI init_socket(LPVOID pM)
{
	SOCKET serverSock;
	struct sockaddr_in serverAddr, connectingAddr;
	int addrlen;
	WSADATA wsaData;
	CheckIP();	//初始化ip
	if (WSAStartup(0x101, &wsaData) != 0)
	{
		puts("socket服务启动失败");
		exit(1);
	}
	serverSock = socket(AF_INET, SOCK_STREAM, IPPROTO_TCP);
	if (serverSock == INVALID_SOCKET)
	{
		puts("socket建立失败");
		exit(1);
	}

	addrlen = sizeof(struct sockaddr);
	memset(&serverAddr, 0, addrlen);
	serverAddr.sin_family = AF_INET;
	serverAddr.sin_port = htons(PORT);
	serverAddr.sin_addr.s_addr = inet_addr(IP_ADDR);

	if (bind(serverSock, (struct sockaddr*)&serverAddr, sizeof(struct sockaddr_in)) == SOCKET_ERROR)
	{
		puts("绑定IP 地址到socket失败");
		exit(1);
	}
	if (listen(serverSock, 10) == SOCKET_ERROR)
	{
		puts("设置socket为监听状态失败");
		exit(1);
	}

	while (1) {
		SOCKET connectingSock;
		struct sockaddr_in connectingAddr;
		puts("等待客户端连接...");
		connectingSock = accept(serverSock, (struct sockaddr*)&connectingAddr, &addrlen);
		if (connectingSock == INVALID_SOCKET)
		{
			puts("处理连接时发生错误");
		}
		CreateThread(NULL, 0, start_listen, (LPVOID)connectingSock, 0, NULL);
		puts("创建了一个新的连接");
	}
	closesocket(serverSock);
	WSACleanup();
}