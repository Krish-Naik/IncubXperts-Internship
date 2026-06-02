const express = require("express");

function createAccountRoutes(accountController) {
    const router = express.Router();

    /**
     * @swagger
     * /api/accounts:
     *   get:
     *     summary: Get all bank accounts
     *     tags: [Accounts]
     *     responses:
     *       200:
     *         description: Accounts fetched successfully
     */
    router.get("/", accountController.getAll);

    /**
     * @swagger
     * /api/accounts/{id}:
     *   get:
     *     summary: Get a bank account by ID
     *     tags: [Accounts]
     *     parameters:
     *       - in: path
     *         name: id
     *         required: true
     *         schema:
     *           type: string
     *           format: uuid
     *     responses:
     *       200:
     *         description: Account fetched successfully
     *       404:
     *         description: Account not found
     */
    router.get("/:id", accountController.getById);

    /**
     * @swagger
     * /api/accounts:
     *   post:
     *     summary: Create a new bank account
     *     tags: [Accounts]
     *     requestBody:
     *       required: true
     *       content:
     *         application/json:
     *           schema:
     *             type: object
     *             required:
     *               - holderName
     *               - email
     *               - type
     *               - openingBalance
     *             properties:
     *               holderName:
     *                 type: string
     *                 example: Krish Naik
     *               email:
     *                 type: string
     *                 example: krish@example.com
     *               type:
     *                 type: string
     *                 example: SAVINGS
     *               openingBalance:
     *                 type: number
     *                 example: 5000
     *     responses:
     *       201:
     *         description: Account created successfully
     *       400:
     *         description: Validation failed
     *       409:
     *         description: Email already exists
     */
    router.post("/", accountController.create);

    /**
     * @swagger
     * /api/accounts/{id}:
     *   put:
     *     summary: Update a bank account
     *     tags: [Accounts]
     *     parameters:
     *       - in: path
     *         name: id
     *         required: true
     *         schema:
     *           type: string
     *           format: uuid
     *     requestBody:
     *       required: true
     *       content:
     *         application/json:
     *           schema:
     *             type: object
     *             properties:
     *               holderName:
     *                 type: string
     *                 example: Krish Naik
     *               email:
     *                 type: string
     *                 example: krish@example.com
     *               type:
     *                 type: string
     *                 example: SAVINGS
     *               isActive:
     *                 type: boolean
     *                 example: true
     *     responses:
     *       200:
     *         description: Account updated successfully
     */
    router.put("/:id", accountController.update);

    /**
     * @swagger
     * /api/accounts/{id}:
     *   delete:
     *     summary: Delete a bank account
     *     tags: [Accounts]
     *     parameters:
     *       - in: path
     *         name: id
     *         required: true
     *         schema:
     *           type: string
     *           format: uuid
     *     responses:
     *       200:
     *         description: Account deleted successfully
     */
    router.delete("/:id", accountController.delete);

    /**
     * @swagger
     * /api/accounts/{id}/deposit:
     *   put:
     *     summary: Deposit money into an account
     *     tags: [Accounts]
     *     parameters:
     *       - in: path
     *         name: id
     *         required: true
     *         schema:
     *           type: string
     *           format: uuid
     *     requestBody:
     *       required: true
     *       content:
     *         application/json:
     *           schema:
     *             type: object
     *             required:
     *               - amount
     *             properties:
     *               amount:
     *                 type: number
     *                 example: 1000
     *     responses:
     *       200:
     *         description: Money deposited successfully
     */
    router.put("/:id/deposit", accountController.deposit);

    /**
     * @swagger
     * /api/accounts/{id}/withdraw:
     *   put:
     *     summary: Withdraw money from an account
     *     tags: [Accounts]
     *     parameters:
     *       - in: path
     *         name: id
     *         required: true
     *         schema:
     *           type: string
     *           format: uuid
     *     requestBody:
     *       required: true
     *       content:
     *         application/json:
     *           schema:
     *             type: object
     *             required:
     *               - amount
     *             properties:
     *               amount:
     *                 type: number
     *                 example: 500
     *     responses:
     *       200:
     *         description: Money withdrawn successfully
     */
    router.put("/:id/withdraw", accountController.withdraw);

    /**
     * @swagger
     * /api/accounts/{id}/transfer:
     *   put:
     *     summary: Transfer money from one account to another
     *     tags: [Accounts]
     *     parameters:
     *       - in: path
     *         name: id
     *         required: true
     *         schema:
     *           type: string
     *           format: uuid
     *     requestBody:
     *       required: true
     *       content:
     *         application/json:
     *           schema:
     *             type: object
     *             required:
     *               - receiverId
     *               - amount
     *             properties:
     *               receiverId:
     *                 type: string
     *                 format: uuid
     *               amount:
     *                 type: number
     *                 example: 250
     *     responses:
     *       200:
     *         description: Transfer successful
     */
    router.put("/:id/transfer", accountController.transfer);

    return router;
}

module.exports = createAccountRoutes;
