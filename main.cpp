#include "gui.h"
#include "client.h"
using namespace std;

extern Client myClient;

int main(void)
{
    bool isLogin = displayHome();

    if (!isLogin)
    {
        return 0;
    }

    displayMenu();

    return 0;
}