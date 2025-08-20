#include "client.h"

Client myClient;

void Account::deposit(long long amount)
{
    balance += amount;
}

bool Account::withdraw(long long amount)
{
    if (balance - amount < 0)
        return false;
    balance -= amount;
    return true;
}

int Account::getAccountId() const
{
    return accountId;
}

long long Account::getBalance() const
{
    return balance;
}

bool Client::login(const std::string &inputId, const std::string &inputPw)
{
    if (clientId == inputId && password == inputPw)
        return true;
    return false;
}

std::string Client::getClientName()
{
    return clientName;
}

Account &Client::createAccount()
{
    static int newId = 1;
    Account newAccount(newId++);
    accounts.push_back(newAccount);
    return accounts.back();
}

std::vector<Account> &Client::getAccounts()
{
    return accounts;
}

void Client::setClient(std::string name, std::string id, std::string pw)
{
    clientName = name;
    clientId = id;
    password = pw;
}

Account &Client::getAccountById(int id)
{
    Account nullAccount = {0, 0};

    for (auto it : accounts)
    {
        if (id == it.getAccountId())
        {
            return it;
        }
    }

    return nullAccount;
}