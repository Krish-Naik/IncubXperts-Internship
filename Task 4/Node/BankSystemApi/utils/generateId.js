const generateId = (accounts) => {
    if (accounts.length === 0) {
      return 1;
    }
  
    return accounts[accounts.length - 1].id + 1;
  };
  
  module.exports = generateId;