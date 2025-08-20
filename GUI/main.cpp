#include "gui.h"
using namespace std;

int main(void)
{
    bool isLogin = displayHome();

    if (isLogin)
    {
        displayMenu();
    }

    return 0;
}