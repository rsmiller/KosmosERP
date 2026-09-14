import { RolePermissionsDto } from "@/models/user-models"

export const permissionsService = {
    HasPermission(module_start: string, erp_module_permission: string, permissions: string[] | undefined){
        console.log(permissions);
        if(permissions == undefined)
        {
            return false;
        }
        if(module_start == "admin")
        {
            let role_build = module_start;

            for(let i=0; i<permissions.length; i++){
                let perm = permissions[i];
                if(perm == role_build)
                {
                    return true;
                }
            }
        }
        else
        {
            let role_build = module_start + "_" + erp_module_permission;

            for(let i=0; i<permissions.length; i++){
                let perm = permissions[i];
                if(perm == role_build)
                {
                    return true;
                }
            }
        }
        return false;
    }
}

export const ERPModules = {
    ActivityModule: "activity",
    ARModule: "ar_invoice",
    APModule: "ap_invoice",
    BOMModule: "bom",
    ContactModule: "contact",
    CreditMemoModule: "credit_memo",
    CustomerModule: "customers",
    CommentModule: "comment",
    DocumentModule: "document",
    InventoryModule: "inventory",
    KeyValueModule: "key_value",
    LeadModule: "crm",
    OpportunityModule: "crm",
    OrderModule: "sales_order",
    ProductionOrderModule: "production_order",
    ProductModule: "product",
    PurchaseOrderModule: "purchase_order",
    PurchaseOrderReceiveModule: "purchase_order_receive",
    SubscriptionModule: "subscription",
    ShippingModule: "shipping",
    TransactionModule: "transaction",
    UserModule: "admin",
    VendorModule: "vendor",
    GeneralLedgerModule: "general_ledger",
    ChartOfAccountModule: "chart_of_account",
    JournalEntryModule: "journal_entry",
    FinancialTransactionModule: "financial_transaction",
    Admin: "admin"
}

export const ERPModulesId = {
    ActivityModule: "1e8f4af3-a0bc-4958-8996-b39dbf6d0084",
    ARModule: "fab75a7f-af8c-4416-9e40-9c774aba1811",
    APModule: "ecf469ca-0a18-459b-954d-47de0bf23cf6",
    BOMModule: "737d367d-3a2d-4b07-87ca-33baf7bb55f3",
    ContactModule: "e89c86b7-44e8-4cac-aee3-3e7bcea845ef",
    CreditMemoModule: "30ccc6b9-d81c-457b-a6df-065adf577316",
    CustomerModule: "09b8ce60-c202-4601-87ab-07edb01a06ed",
    CommentModule: "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
    DocumentModule: "4b0ce064-9c4b-4e39-8812-79cc3f69e945",
    InventoryModule: "0ded003d-e411-48d9-a5e7-7102e42f36a0",
    KeyValueModule: "8063f3ae-6b8f-4f23-90f0-1daccbd18e00",
    LeadModule: "6a4897a8-571d-4f5b-99c4-9eb15f56f619",
    OpportunityModule: "0c3959c3-15dc-44ab-8e2c-9b9e2773e65f",
    OrderModule: "68c1862f-6043-4b09-8674-dd844a3a6fed",
    ProductionOrderModule: "97dd4b13-ff15-47ff-955d-5e957644cffd",
    ProductModule: "bc4b4287-e927-4b9a-b0cf-1174f0a13f39",
    PurchaseOrderModule: "78c4861d-1252-4cac-9461-0e1e0399cd83",
    PurchaseOrderReceiveModule: "bdaa14c4-64d8-44f3-b1ad-d272c408832f",
    ShippingModule: "9d624ee2-6433-49f0-bc6c-3e6978e2ac9c",
    TransactionModule: "416786e0-47b3-440a-90da-b7036d72b1f7",
    UserModule: "b8b0d255-3901-4007-b9c7-b0678f89c955",
    VendorModule: "dae2593c-678b-4f6d-9c84-f4f74e066428",
}

export const ERPModulePermission = {
    Read: "read",
    Edit: "edit",
    Write: "write",
    Delete: "delete"
}