#define _CRT_SECURE_NO_WARNINGS
#include<Windows.h>
#include<CommCtrl.h>
#include<cstdio>
#include"resource.h"

BOOL CALLBACK DlgProc(HWND hwnd, UINT uMsg, WPARAM wParam, LPARAM lparam);

INT WINAPI WinMain(HINSTANCE hInstance, HINSTANCE hPrevInst, LPSTR lpCmdLine, INT nCmdShow)
{
	DialogBoxParam(hInstance, MAKEINTRESOURCE(IDD_DIALOG_MAIN), NULL, (DLGPROC)DlgProc, 0);
	return 0;
}

BOOL CALLBACK DlgProc(HWND hwnd, UINT uMsg, WPARAM wParam, LPARAM lparam)
{
	switch (uMsg)
	{
		case WM_INITDIALOG:
		{
			HWND hPrefix = GetDlgItem(hwnd, IDC_SPIN_PREFIX);
			SendMessage(hPrefix, UDM_SETRANGE, 0, MAKELPARAM(30, 1));

			SetFocus(GetDlgItem(hwnd, IDC_IPADDRESS));
		}
			break;

		case WM_COMMAND:
		{
			HWND hIPaddress = GetDlgItem(hwnd, IDC_IPADDRESS);
			HWND hIPmask = GetDlgItem(hwnd, IDC_IPMASK);
			HWND hEditPrefix = GetDlgItem(hwnd, IDC_EDIT_PREFIX);
			DWORD dwIPaddress = 0;
			DWORD dwIPmask = 0;

			switch (LOWORD(wParam))
			{
				case IDC_IPADDRESS:
				{
					SendMessage(hIPaddress, IPM_GETADDRESS, 0, (LPARAM)&dwIPaddress);
					if (FIRST_IPADDRESS(dwIPaddress) < 128) SendMessage(hIPmask, IPM_SETADDRESS, 0, 0xFF000000);
					if (FIRST_IPADDRESS(dwIPaddress) < 192) SendMessage(hIPmask, IPM_SETADDRESS, 0, 0xFFFF0000);
					if (FIRST_IPADDRESS(dwIPaddress) < 224) SendMessage(hIPmask, IPM_SETADDRESS, 0, 0xFFFFFF00);
				}
					break;


				case IDC_IPMASK:
				{
					SendMessage(hIPmask, IPM_GETADDRESS, 0, (LPARAM)&dwIPmask);
					DWORD dwIPprefix = 0;
					for (DWORD iMask = dwIPmask; iMask & 0x80000000; dwIPprefix++) iMask <<= 1;
					CHAR sz_prefix[3];
					sprintf(sz_prefix, "%i", dwIPprefix);
					SendMessage(hEditPrefix, WM_SETTEXT, 0, (LPARAM)sz_prefix);
				}
					break;


				case IDOK:
					break;

				case IDCANCEL:
					EndDialog(hwnd, 0);
			}
		}
		break;

		case WM_CLOSE:
			EndDialog(hwnd, 0);
	}

	return FALSE;
}