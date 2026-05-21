#include <iostream>
#include "BankSystem.h"

using namespace std;

int main(){
    BankSystem bank;
    int choice;
    do{
        cout<<endl;
        cout<<"---------------------------------"<<endl;
        cout<<"BANK MANAGEMENT SYSTEM"<<endl<<endl;
        cout<<"1. Create Savings Account"<<endl;
        cout<<"2. Create Current Account"<<endl;
        cout<<"3. Deposit Money"<<endl;
        cout<<"4. Withdraw Money"<<endl;
        cout<<"5. Transfer Money"<<endl;
        cout<<"6. Display Account Details"<<endl;;
        cout<<"7. Display All Accounts"<<endl;;
        cout<<"8. Show Interest"<<endl;;
        cout<<"9. Show Transaction History"<<endl;
        cout<<"10. Exit"<<endl;
        cout<<"Enter your choice: ";
        cin >>choice;
        try{
            switch(choice){
                case 1:{
                    string accountNumber;
                    string accountHolderName;
                    double balance;
                    double interestRate;

                    cout<<"Enter Account Number: ";
                    cin>>accountNumber;
                    cin.ignore();
                    cout << "Enter Holder Name: ";
                    getline(cin, accountHolderName);

                    cout<<"Enter Initial Balance: ";
                    cin>>balance;
                    cout<<"Enter Interest Rate: ";
                    cin>>interestRate;
                    bank.createSavingsAccount(
                        accountNumber,
                        accountHolderName,
                        balance,
                        interestRate
                    );
                    break;
                }
                case 2: {
                    string accountNumber;
                    string accountHolderName;
                    double balance;
                    double overdraftLimit;

                    cout<<"Enter Account Number: ";
                    cin>>accountNumber;
                    cin.ignore();
                    cout<<"Enter Holder Name: ";
                    getline(cin, accountHolderName);

                    cout<<"Enter Initial Balance: ";
                    cin>>balance;

                    cout<<"Enter Overdraft Limit: ";
                    cin>>overdraftLimit;

                    bank.createCurrentAccount(
                        accountNumber,
                        accountHolderName,  
                        balance,
                        overdraftLimit
                    );
                    break;
                }
                case 3: {
                    string accountNumber;
                    int amount;

                    cout<<"Enter Account Number: ";
                    cin>>accountNumber;

                    cout<<"Enter Deposit Amount: ";
                    cin>>amount;

                    bank.deposit(accountNumber, amount);

                    break;
                }

                case 4: {
                    string accountNumber;
                    int amount;
                    cout<<"Enter Account Number: ";
                    cin>>accountNumber;
                    cout<<"Enter Withdrawal Amount: ";
                    cin>>amount;
                    bank.withdraw(accountNumber, amount);
                    break;
                }
                case 5: {
                    string senderAcc;
                    string receiverAcc;
                    int amount;
                    cout<<"Enter Sender Account Number: ";
                    cin>>senderAcc;
                    cout<<"Enter Receiver Account Number: ";
                    cin>>receiverAcc;
                    cout<<"Enter Amount To Transfer: ";
                    cin>>amount;
                    bank.transferMoney(senderAcc, receiverAcc, amount);
                    break;
                }

                case 6: {
                    string accountNumber;
                    cout<<"Enter Account Number: ";
                    cin>>accountNumber;
                    bank.displaySingleAccount(accountNumber);
                    break;
                }

                case 7:{
                    bank.displayAllAccounts();
                    break;
                }

                case 8: {
                    string accountNumber;
                    cout<<"Enter Account Number: ";
                    cin>>accountNumber;
                    bank.calculateInterestForAccount(accountNumber);
                    break;
                }

                case 9: {
                    string accountNumber;

                    cout<<"Enter Account Number: ";
                    cin>>accountNumber;

                    bank.showAccountTransactions(accountNumber);

                    break;
                }

                case 10: {
                    cout<<"Thank You For Using Banking System."<<endl;
                    break;
                }

                default:
                    cout<<"Invalid Choice."<<endl;
            }
        }
        catch(exception &e){
            cout<<"Error: "<<e.what()<<endl;
        }
    }while(choice != 10);
    return 0;
}