const express = require('express');

const {
  getAccounts,
  getAccountById,
  createAccount,
  updateAccount,
  deleteAccount,
  depositMoney,
  withdrawMoney,
  transferMoney
} = require('../controllers/accountController');

const router = express.Router();

router.get('/', getAccounts);

router.get('/:id', getAccountById);

router.post('/', createAccount);

router.put('/:id', updateAccount);

router.delete('/:id', deleteAccount);

router.put('/:id/deposit', depositMoney);

router.put('/:id/withdraw', withdrawMoney);

router.put('/:id/transfer', transferMoney);

module.exports = router;