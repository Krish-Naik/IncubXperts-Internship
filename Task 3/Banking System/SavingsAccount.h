#pragma once
#include "BankAccount.h"
using namespace std;

class SavingsAccount : public BankAccount{
private:
    double interestRate;
public:
    SavingsAccount(string accountNumber, string accountHolderName, double balance, double interestRate) : BankAccount(accountNumber, accountHolderName, balance){
        this->interestRate = interestRate;
    }
    void calculateInterest() override{
        double interest = balance * interestRate/100;
        cout<<"Interest: Rs. "<<interest<<endl;

    }
    void displayAccount() override{
        BankAccount::displayAccount();
        cout<<"Account Type: Savings"<<endl;
        cout<<"Interest Rate: "<<interestRate<<endl;
    }
};