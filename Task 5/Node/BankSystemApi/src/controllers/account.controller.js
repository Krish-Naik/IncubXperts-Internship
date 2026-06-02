class AccountController {
    constructor(accountService) {
        this.accountService = accountService;
    }
    
    getAll = async (req, res, next) => {
        try {
            const data = await this.accountService.getAll();
            res.status(200).json({
                success: true,
                message: "Accounts fetched successfully",
                data,
            });
        } catch (err) {
            next(err);
        }
    };

    getById = async (req, res, next) => {
        try {
            const data = await this.accountService.getById(req.params.id);
            res.status(200).json({
                success: true,
                message: "Account fetched successfully",
                data,
            });
        } catch (err) {
            next(err);
        }
    };

    create = async (req, res, next) => {
        try {
            const data = await this.accountService.create(req.body);
            res.status(201).json({
                success: true,
                message: "Account created successfully",
                data,
            });
        } catch (err) {
            next(err);
        }
    };

    update = async (req, res, next) => {
        try {
            const data = await this.accountService.update(req.params.id, req.body);
            res.status(200).json({
                success: true,
                message: "Account updated successfully",
                data,
            });
        } catch (err) {
            next(err);
        }
    };

    delete = async (req, res, next) => {
        try {
            await this.accountService.delete(req.params.id);
            res.status(200).json({
                success: true,
                message: "Account deleted successfully",
                data: null,
            });
        } catch (err) {
            next(err);
        }
    };

    deposit = async (req, res, next) => {
        try {
            const data = await this.accountService.deposit(
                req.params.id,
                req.body.amount,
            );
            res.status(200).json({
                success: true,
                message: "Money deposited successfully",
                data,
            });
        } catch (err) {
            next(err);
        }
    };

    withdraw = async (req, res, next) => {
        try {
            const data = await this.accountService.withdraw(
                req.params.id,
                req.body.amount,
            );
            res.status(200).json({
                success: true,
                message: "Money withdrawn successfully",
                data,
            });
        } catch (err) {
            next(err);
        }
    };

    transfer = async (req, res, next) => {
        try {
            const data = await this.accountService.transfer(
                req.params.id,
                req.body.receiverId,
                req.body.amount,
            );
            res.status(200).json({
                success: true,
                message: "Transfer successful",
                data,
            });
        } catch (err) {
            next(err);
        }
    };
}

module.exports = AccountController;
