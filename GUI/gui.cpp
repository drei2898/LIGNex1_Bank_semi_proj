#include "gui.h"
using namespace std;

void displayTitle(void)
{
    cout << "\n";
    cout << "   +-------------------------------+\n";
    cout << "   |                               |\n";
    cout << "   |      채우은행 인터넷뱅킹      |\n";
    cout << "   |                               |\n";
    cout << "   +-------------------------------+\n";
    cout << "\n";
}

bool displayHome(void)
{
    int inputMenu;
    vector<string> menuName = {"로그인", "회원가입", "종료"};

    while (1)
    {
        system("cls");
        displayTitle();

        for (int i = 0; i < menuName.size(); i++)
        {
            cout << "   " << i + 1 << ". " << menuName[i] << "\n";
        }

        cout << "\n메뉴 입력: ";
        cin >> inputMenu;

        switch (inputMenu)
        {
        case 1:
            while (1)
            {
                if (displayLogin)
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

    cout << "   - 아이디: ";
    cin >> id;
    cout << "   - 패스워드: ";
    cin >> pw;

    return 함수(id, pw);
}

void displayRegister(void)
{
    string id, name, pw;

    system("cls");
    displayTitle();

    cout << "   - 이름: ";
    cin >> name;
    cout << "   - 아이디: ";
    cin >> id;
    cout << "   - 패스워드: ";
    cin >> pw;

    함수(name, id, pw);
}

void displayMenu(void)
{
    int inputMenu;
    vector<string> menuName = {"계좌 생성", "계좌 조회", "입금/출금", "종료"};

    while (1)
    {
        system("cls");
        displayTitle();

        for (int i = 0; i < menuName.size(); i++)
        {
            cout << "   " << i + 1 << ". " << menuName[i] << "\n";
        }

        cout << "\n메뉴 입력: ";
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
    int accountId, balance;
    string name;

    system("cls");
    displayTitle();

    cout << "   - 이름: " << name << "\n";
    cout << "   - 계좌 아이디: " << accountId << "\n";
    cout << "   - 입금 금액: ";
    cin >> balance;

    함수(balance);
}

void displayCheck(void)
{
    int accountId, balance;
    string name;

    system("cls");
    displayTitle();

    cout << "   - 이름: " << name << "\n";

    for (auto it : 클라.계좌)
    {
        cout << "   - 계좌 아이디: " << accountId << "\n";
        cout << "   - 입금 금액: " << balance << "\n";
    }
}

void displayDeposit(void)
{
    int accountId, balance, req;
    string name;

    system("cls");
    displayTitle();

    cout << "\n* 요청: 1. 입금 2. 출금\n";

    cout << "   - 이름: " << name << "\n";
    cout << "   - 계좌 아이디: ";
    cin >> accountId;
    cout << "   - 입금/출금: ";
    cin >> req;
    cout << "   - 금액: ";
    cin >> balance;

    return 함수(accountId, req, balance);
}