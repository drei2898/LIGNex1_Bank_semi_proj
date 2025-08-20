#include "gui.h"
using namespace std;

void displayTitle(void)
{
    cout << "\n";
    cout << "   +-------------------------------+\n";
    cout << "   |                               |\n";
    cout << "   |      ä������ ���ͳݹ�ŷ      |\n";
    cout << "   |                               |\n";
    cout << "   +-------------------------------+\n";
    cout << "\n";
}

bool displayHome(void)
{
    int inputMenu;
    vector<string> menuName = { "�α���", "ȸ������", "����" };

    while (1)
    {
        system("cls");
        displayTitle();

        for (int i = 0; i < menuName.size(); i++)
        {
            cout << "   " << i + 1 << ". " << menuName[i] << "\n";
        }

        cout << "\n�޴� �Է�: ";
        cin >> inputMenu;

        switch (inputMenu)
        {
        case 1:
            while (1)
            {
                if (displayLogin())
                {
                    return true;
                }
            }
        case 2:
            displayRegister();
            break;
        case 3:
            return false;
        }
    }
}

bool displayLogin(void)
{
    string id, pw;

    system("cls");
    displayTitle();

    cout << "   - ���̵�: ";
    cin >> id;
    cout << "   - �н�����: ";
    cin >> pw;

    return myClient.login(id, pw);
}

void displayRegister(void)
{
    string id, name, pw;

    system("cls");
    displayTitle();

    cout << "   - �̸�: ";
    cin >> name;
    cout << "   - ���̵�: ";
    cin >> id;
    cout << "   - �н�����: ";
    cin >> pw;

    myClient.setClient(name, id, pw);
    //(���� ������ �Լ�)
}

void displayMenu(void)
{
    int inputMenu;
    vector<string> menuName = { "���� ����", "���� ��ȸ", "�Ա�/���", "����" };

    while (1)
    {
        system("cls");
        displayTitle();

        for (int i = 0; i < menuName.size(); i++)
        {
            cout << "   " << i + 1 << ". " << menuName[i] << "\n";
        }

        cout << "\n�޴� �Է�: ";
        cin >> inputMenu;

        switch (inputMenu)
        {
        case 1:
            displayCreate();
            break;
        case 2:
            displayCheck();
            break;
        case 3:
            displayDeposit();
            break;
        case 4:
            return;
        }
    }
}

void displayCreate(void)
{
    int accountId;
    string name;

    system("cls");
    displayTitle();

    myClient.createAccount();
    //(������ ���� ����)

    cout << "   - �̸�: " << myClient.getClientName() << "\n";
    cout << "   - ���� ���̵�: " << myClient.getAccounts().back().getAccountId() << "\n";

    _getch();
}

void displayCheck(void)
{
    int accountId, balance;
    string name;

    system("cls");
    displayTitle();

    cout << "   - �̸�: " << name << "\n";

    for (auto it : myClient.getAccounts())
    {
        cout << "   - ���� ���̵�: " << it.getAccountId() << "\n";
        cout << "   - �Ա� �ݾ�: " << it.getBalance() << "\n";
    }

    _getch();
}

void displayDeposit(void)
{
    int accountId, balance, req;
    string name;

    system("cls");
    displayTitle();

    cout << "\n* ��û: 1. �Ա� 2. ���\n";

    cout << "   - �̸�: " << myClient.getClientName() << "\n";
    cout << "   - ���� ���̵�: ";
    cin >> accountId;
    cout << "   - �Ա�/���: ";
    cin >> req;
    cout << "   - �ݾ�: ";
    cin >> balance;

    switch (req)
    {
    case 1:
        for(auto &it : myClient.getAccounts())
        {
            if (it.getAccountId() == accountId)
            {
				it.deposit(balance);
            }
		}
        break;
    case 2:
        for (auto& it : myClient.getAccounts())
        {
            if (it.getAccountId() == accountId)
            {
                it.withdraw(balance);
            }
        }
        break;
    }

    _getch();
}