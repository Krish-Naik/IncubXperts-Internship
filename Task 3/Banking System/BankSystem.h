#pragma once
#include "BankAccount.h"
#include "SavingsAccount.h"
#include "CurrentAccount.h"
#include <vector>
using namespace std;

class BankSystem{
private:
    vector<BankAccount*> accounts;
public:
    ~BankSystem(){
        for(int i = 0;i<accounts.size();i++){
            delete accounts[i];
        }
    }
    void createSavingsAccount(string accountNumber, string accountHolderName, double balance, double interestRate){
        SavingsAccount* account = new SavingsAccount(accountNumber, accountHolderName, balance, interestRate);
        accounts.push_back(account);
    }
    void createCurrentAccount(string accountNumber, string accountHolderName, double balance, double overdraftLimit){
        CurrentAccount* account = new CurrentAccount(accountNumber, accountHolderName, balance, overdraftLimit);
        accounts.push_back(account);
    }
    void displayAllAccounts(){
        for(int i = 0; i < accounts.size(); i++){
            accounts[i]->displayAccount();
        }
    }
    void displaySingleAccount(string accountNumber) {
        BankAccount* account = findAccount(accountNumber);
        if(account != nullptr){
            account->displayAccount();
        }
        else{
            cout << "Account not found."<<endl;
        }
    }
    BankAccount* findAccount(string accountNumber){
        for(int i = 0; i < accounts.size(); i++){
            if(accounts[i]->getAccountNumber() == accountNumber){
                return accounts[i];
            }
        }
        return nullptr;
    }
    void deposit(string accountNumber, int amount){
        BankAccount* account = findAccount(accountNumber);
        if(account){
            account->deposit(amount);
        }
        else{
            cout<<"Account not found"<<endl;
        }
    }
    void withdraw(string accountNumber, int amount){
        BankAccount* account = findAccount(accountNumber);
        if(account){
            account->withdraw(amount);
        }
        else{
            cout<<"Account not found"<<endl;
        }
    }
    void transferMoney(string fromAccountNumber, string toAccountNumber, double amount){
        BankAccount* fromAccount = findAccount(fromAccountNumber);
        BankAccount* toAccount = findAccount(toAccountNumber);
        
        if(fromAccount == nullptr){
            cout<<"Sender account not found."<<endl;
            return;
        }
        if(toAccount == nullptr){
            cout<<"Receiver account not found."<<endl;
            return;
        }
        if(fromAccount->getBalance() < amount){
            cout<<"Insufficient balance for transfer."<<endl;
            return;
        }
        fromAccount->withdraw(amount);
        toAccount->deposit(amount);
        cout<<"Transfer of Rs. "<<amount<< " successful."<<endl;
    }
    void calculateInterestForAccount(string accountNumber){
        BankAccount* account = findAccount(accountNumber);
        if(account != nullptr){
            account->calculateInterest();
        }
        else{
            cout<<"Account not found."<<endl;
        }
    }
    void showAccountTransactions(string accountNumber){
        BankAccount* account = findAccount(accountNumber);
        if(account != nullptr){
            account->showTransactions();
        }
        else{
            cout<<"Account not found."<<endl;
        }
    }
};