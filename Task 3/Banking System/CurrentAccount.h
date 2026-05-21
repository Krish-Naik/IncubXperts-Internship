#pragma once
#include "BankAccount.h"
#include "Transaction.h"
#include <stdexcept>
using namespace std;

class CurrentAccount : public BankAccount{
private:
    double overdraftLimit;
public:
    CurrentAccount(string accountNumber, string accountHolderName, double balance, double overdraftLimit) : BankAccount(accountNumber, accountHolderName, balance){
        this->overdraftLimit = overdraftLimit;
    }
    void withdraw(int amount) override{
        if(amount > (balance + overdraftLimit)){
            throw runtime_error("Overdraft limit exceeded.");
        }
        balance -= amount;
        if(balance < 0){
            cout<<"Overdraft used for: Rs. "<<abs(balance)<<endl;
            balance = 0;
        }
        transactions.push_back(Transaction(amount, "Withdraw"));
        cout<<"Withdrawal Successful"<<endl;
    }
    void calculateInterest() override{
        cout << "No interest for Current Account."<<endl;
    }
    void displayAccount() override{
        BankAccount::displayAccount();
        cout<<"Account Type: Current Account"<<endl;
        cout<<"Overdraft Limit: Rs. "<<overdraftLimit<<endl;
    }
};