#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <winSock2.h>

#pragma comment(lib , "Ws2_32.lib")

#define PORT 50003
char *IP_ADDR;

extern char *msg_parser(char *command, char *result);

void CheckIP(void) //����CheckIP�������������ڻ�ȡ����IP��ַ 
{
	WSADATA wsaData;
	char name[255];//�������ڴ�Ż�õ��������ı��� 
	char *ip;//����IP��ַ���� 
	PHOSTENT hostinfo; 

	//����MAKEWORD�������Winsock�汾����ȷֵ�����ڼ���Winsock�� 
	if ( WSAStartup( MAKEWORD(2,0), &wsaData ) == 0 ) { 
		//�����Ǽ���Winsock�⣬���WSAStartup������������ֵΪ0��˵�����سɹ���������Լ��� 
		if( gethostname ( name, sizeof(name)) == 0) { 
			//����ɹ��ؽ������������������name����ָ���Ļ������� 
			if((hostinfo = gethostbyname(name)) != NULL) { 
				//���ǻ�ȡ���������������������ɹ��Ļ���������һ��ָ�룬ָ��hostinfo��hostinfo 
				//ΪPHOSTENT�͵ı��������漴���õ�����ṹ�� 
				ip = inet_ntoa (*(struct in_addr *)*hostinfo->h_addr_list); 
				//����inet_ntoa������������hostinfo�ṹ�����е�h_addr_listת��Ϊ��׼�ĵ�ֱ�ʾ��IP 
				//��ַ����192.168.0.1�� 
				IP_ADDR = ip;
				//printf("%s\n",ip);//���IP��ַ
			} 
		} 
		WSACleanup( );//ж��Winsock�⣬���ͷ�������Դ 
	}
} 

/*
��ʼ����һ��socket
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
			puts("������Ϣʧ��");
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
			puts("������Ϣʧ��");
		}
		else {
			puts("������Ϣ�ɹ�");
		}
	}
	closesocket(connectingSock);
}

/*
����socket
*/
DWORD WINAPI init_socket(LPVOID pM)
{
	SOCKET serverSock;
	struct sockaddr_in serverAddr, connectingAddr;
	int addrlen;
	WSADATA wsaData;
	CheckIP();	//��ʼ��ip
	if (WSAStartup(0x101, &wsaData) != 0)
	{
		puts("socket��������ʧ��");
		exit(1);
	}
	serverSock = socket(AF_INET, SOCK_STREAM, IPPROTO_TCP);
	if (serverSock == INVALID_SOCKET)
	{
		puts("socket����ʧ��");
		exit(1);
	}

	addrlen = sizeof(struct sockaddr);
	memset(&serverAddr, 0, addrlen);
	serverAddr.sin_family = AF_INET;
	serverAddr.sin_port = htons(PORT);
	serverAddr.sin_addr.s_addr = inet_addr(IP_ADDR);

	if (bind(serverSock, (struct sockaddr*)&serverAddr, sizeof(struct sockaddr_in)) == SOCKET_ERROR)
	{
		puts("��IP ��ַ��socketʧ��");
		exit(1);
	}
	if (listen(serverSock, 10) == SOCKET_ERROR)
	{
		puts("����socketΪ����״̬ʧ��");
		exit(1);
	}

	while (1) {
		SOCKET connectingSock;
		struct sockaddr_in connectingAddr;
		puts("�ȴ��ͻ�������...");
		connectingSock = accept(serverSock, (struct sockaddr*)&connectingAddr, &addrlen);
		if (connectingSock == INVALID_SOCKET)
		{
			puts("��������ʱ��������");
		}
		CreateThread(NULL, 0, start_listen, (LPVOID)connectingSock, 0, NULL);
		puts("������һ���µ�����");
	}
	closesocket(serverSock);
	WSACleanup();
}