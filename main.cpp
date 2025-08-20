#include "gui.h"
#include "client.h"
using namespace std;

extern Client myClient;

int main(void)
{
    bool isLogin = displayHome();

    if (isLogin)
    {
        displayMenu();
    }

    displayMenu();

    return 0;
}