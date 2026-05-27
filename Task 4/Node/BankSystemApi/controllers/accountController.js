const accounts = require('../data/accounts');
const generateId = require('../utils/generateId');


// GET ALL ACCOUNTS
const getAccounts = (req, res) => {
  res.status(200).json({
    success: true,
    data: accounts
  });
};

// GET ACCOUNT BY ID
const getAccountById = (req, res) => {
  const id = parseInt(req.params.id);

  const account = accounts.find(acc => acc.id === id);

  if (!account) {
    return res.status(404).json({
      success: false,
      message: 'Account not found'
    });
  }

  res.status(200).json({
    success: true,
    data: account
  });
};


// CREATE ACCOUNT
const createAccount = (req, res) => {
  const { name, balance, type } = req.body;

  if (!name || balance === undefined || !type) {
    return res.status(400).json({
      success: false,
      message: 'Please provide all fields'
    });
  }

  const newAccount = {
    id: generateId(accounts),
    name,
    balance,
    type
  };

  accounts.push(newAccount);

  res.status(201).json({
    success: true,
    message: 'Account created successfully',
    data: newAccount
  });
};


// UPDATE ACCOUNT
const updateAccount = (req, res) => {
  const id = parseInt(req.params.id);

  const account = accounts.find(acc => acc.id === id);

  if (!account) {
    return res.status(404).json({
      success: false,
      message: 'Account not found'
    });
  }

  const { name, balance, type } = req.body;

  if (name !== undefined) {
    account.name = name;
  }

  if (balance !== undefined) {
    account.balance = balance;
  }

  if (type !== undefined) {
    account.type = type;
  }

  res.status(200).json({
    success: true,
    message: 'Account updated successfully',
    data: account
  });
};


// DELETE ACCOUNT
const deleteAccount = (req, res) => {
  const id = parseInt(req.params.id);

  const index = accounts.findIndex(acc => acc.id === id);

  if (index === -1) {
    return res.status(404).json({
      success: false,
      message: 'Account not found'
    });
  }

  const deletedAccount = accounts.splice(index, 1);

  res.status(200).json({
    success: true,
    message: 'Account deleted successfully',
    data: deletedAccount[0]
  });
};


// DEPOSIT MONEY
const depositMoney = (req, res) => {
  const id = parseInt(req.params.id);

  const account = accounts.find(acc => acc.id === id);

  if (!account) {
    return res.status(404).json({
      success: false,
      message: 'Account not found'
    });
  }

  const { amount } = req.body;

  if (!amount || amount <= 0) {
    return res.status(400).json({
      success: false,
      message: 'Invalid amount'
    });
  }

  account.balance += amount;

  res.status(200).json({
    success: true,
    message: 'Money deposited successfully',
    data: account
  });
};


// WITHDRAW MONEY
const withdrawMoney = (req, res) => {
  const id = parseInt(req.params.id);

  const account = accounts.find(acc => acc.id === id);

  if (!account) {
    return res.status(404).json({
      success: false,
      message: 'Account not found'
    });
  }

  const { amount } = req.body;

  if (!amount || amount <= 0) {
    return res.status(400).json({
      success: false,
      message: 'Invalid amount'
    });
  }

  if (amount > account.balance) {
    return res.status(400).json({
      success: false,
      message: 'Insufficient balance'
    });
  }

  account.balance -= amount;

  res.status(200).json({
    success: true,
    message: 'Money withdrawn successfully',
    data: account
  });
};


// TRANSFER MONEY
const transferMoney = (req, res) => {
  const senderId = parseInt(req.params.id);

  const { receiverId, amount } = req.body;

  const sender = accounts.find(acc => acc.id === senderId);
  const receiver = accounts.find(acc => acc.id === receiverId);

  if (!sender || !receiver) {
    return res.status(404).json({
      success: false,
      message: 'Sender or receiver not found'
    });
  }

  if (amount <= 0) {
    return res.status(400).json({
      success: false,
      message: 'Invalid amount'
    });
  }

  if (sender.balance < amount) {
    return res.status(400).json({
      success: false,
      message: 'Insufficient balance'
    });
  }

  sender.balance -= amount;
  receiver.balance += amount;

  res.status(200).json({
    success: true,
    message: 'Transfer successful',
    sender,
    receiver
  });
};


module.exports = {
  getAccounts,
  getAccountById,
  createAccount,
  updateAccount,
  deleteAccount,
  depositMoney,
  withdrawMoney,
  transferMoney
};