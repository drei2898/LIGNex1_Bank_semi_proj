#include <iostream>
#include <string>
#include <vector>

class Account {
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

class Client {
private:
    int clientId;
    std::string password;
    std::vector<Account> accounts;  

public:
    Client(int id, std::string pwd) : clientId(id), password(pwd) {}

    bool login(const std::string& inputPw);
    Account& createAccount();
    std::vector<Account>& getAccounts();
    int getClientId();
};
