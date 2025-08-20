#include "client.h"

void Account::deposit(long long amount) {
    balance += amount;
}

bool Account::withdraw(long long amount){
    if(balance - amount<0) return false;
    balance -= amount;
    return true;
}

int Account::getAccountId() const{
    return accountId;
}

long long Account::getBalance() const {
    return balance;
}

bool Client::login(const std::string& inputPw) {
    if(password==inputPw) return true;
    return false;
}

int Client::getClientId(){
    return clientId;
}

Account& Client::createAccount(){
    int newId = clientId * 1000 + accounts.size() + 1;
    Account newAccount(newId);
    accounts.push_back(newAccount);
    return newAccount;
}

std::vector<Account>& Client::getAccounts(){
    return accounts;
}