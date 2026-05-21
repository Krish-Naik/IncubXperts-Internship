#pragma once
#include <iostream>
using namespace std;

class Transaction{
private:
    int amount;
    string type;
public:
    Transaction(int amount, string type){
        this->amount = amount;
        this->type = type;
    }
    void displayTransaction(){
        cout<<"Amount: "<<amount<<endl;
        cout<<"Type: "<<type<<endl;
    }
    int getAmount(){
        return amount;
    }
    string getType(){
        return type;
    }
};