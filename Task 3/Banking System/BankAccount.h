#pragma once
#include <iostream>
#include "Transaction.h"
using namespace std;

class BankAccount{
private:
    string accountNumber;
    string accountHolderName;
protected:
    double balance;
    vector<Transaction> transactions;
public:
    BankAccount(string accountNumber, string accountHolderName, double balance){
        this->accountNumber = accountNumber;
        this->accountHolderName = accountHolderName;
        this->balance = balance;
    }
    virtual ~BankAccount(){}
    void deposit(int amount){
        balance += amount;
        transactions.push_back(Transaction(amount, "Deposit"));
        cout<<"Deposit successful"<<endl;
    }
    virtual void withdraw(int amount){
        if(balance >= amount && amount > 0){
            balance -= amount;
            transactions.push_back(Transaction(amount, "Withdraw"));
            cout<<"Withdraw successful"<<endl;
        }
        else if(amount <= 0){
            cout<<"Invalid amount"<<endl;
        }
        else{
            cout<<"Insufficient balance"<<endl;
        }
    }
    virtual void displayAccount(){
        cout<<"Account Number: "<<accountNumber<<endl;
        cout<<"Account Holder Name: "<<accountHolderName<<endl;
        cout<<"Balance: "<<balance<<endl;
        cout<<"Transactions: "<<endl;
    }
    void showTransactions(){
        for(int i = 0; i < transactions.size(); i++){
            transactions[i].displayTransaction();
        }
    }
    string getAccountNumber(){
        return accountNumber;
    }
    string getAccountHolderName(){
        return accountHolderName;
    }
    double getBalance(){
        return balance;
    }
    virtual void calculateInterest() = 0;
};