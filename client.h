#pragma once
#include <iostream>
#include <string>
#include <vector>

class Account
{
private:
    long long balance;
    int accountId;

public:
    Account(int id, long long initial = 0) : accountId(id), balance(initial) {}

    void deposit(long long amount);
    bool withdraw(long long amount);
    long long getBalance() const;
    int getAccountId() const;
};

class Client
{
private:
    std::string clientId;
    std::string clientName;
    std::string password;
    std::vector<Account> accounts;

public:
    bool login(const std::string &inputId, const std::string &inputPw);
    Account &createAccount();
    std::vector<Account> &getAccounts();
    Account &getAccountById(int id);
    std::string getClientName();
    void setClient(std::string name, std::string id, std::string pw);
};
