import { FaHome, FaCogs, FaMoneyBill, FaChartBar, FaCubes, FaFileAlt, FaTruckMoving,
          FaUserFriends, FaCity, FaBoxOpen, FaDollarSign, FaListAlt, FaReceipt, FaTh,
          FaClipboardList, FaUsers, FaUserPlus, FaRegIdCard, FaCashRegister, 
          FaBoxes, FaAddressBook} from "react-icons/fa";
import { CiMemoPad } from "react-icons/ci";
import { BsListColumnsReverse } from "react-icons/bs";
import { Link as ChakraLink, Menu, Portal, VStack } from "@chakra-ui/react";
import { ERPModulePermission, ERPModules, permissionsService } from '@/services/permissions-service';
import { useEffect, useState } from "react";
import { useAuth } from "@/lib/auth/auth-context";
import { useRouter } from "next/navigation";

function SidebarComponent({ onNavigate } : any)
{
    const router = useRouter();
    const auth = useAuth();

    const PermissionsService = permissionsService;
    const [userPermissions, setUserPermissions] = useState<string[]>([]);
    const [isAdmin, setIsAdmin] = useState<boolean>(false);


    useEffect(() => {
        setUserPermissions(auth.roles || []);
    }, [auth.roles]);

    const doRoute = (url: string) => () => {
        // Token refresh is now owned by the active auth provider in the
        // background, so navigation is a plain route change.
        router.push(url);
    };

    const hasAccountingAcccess = () => 
    {
        return PermissionsService.HasPermission(ERPModules.ARModule, ERPModulePermission.Read, userPermissions) ||
               PermissionsService.HasPermission(ERPModules.APModule, ERPModulePermission.Read, userPermissions) ||
               PermissionsService.HasPermission(ERPModules.CreditMemoModule, ERPModulePermission.Read, userPermissions) ||
               PermissionsService.HasPermission(ERPModules.GeneralLedgerModule, ERPModulePermission.Read, userPermissions) ||
               PermissionsService.HasPermission(ERPModules.SubscriptionModule, ERPModulePermission.Read, userPermissions) ||
               PermissionsService.HasPermission(ERPModules.ChartOfAccountModule, ERPModulePermission.Read, userPermissions) ||
               PermissionsService.HasPermission(ERPModules.JournalEntryModule, ERPModulePermission.Read, userPermissions) ||
               PermissionsService.HasPermission(ERPModules.FinancialTransactionModule, ERPModulePermission.Read, userPermissions);
    }

    const hasGLAccess = () => 
    {
        return PermissionsService.HasPermission(ERPModules.ChartOfAccountModule, ERPModulePermission.Read, userPermissions) ||
               PermissionsService.HasPermission(ERPModules.JournalEntryModule, ERPModulePermission.Read, userPermissions) ||
               PermissionsService.HasPermission(ERPModules.FinancialTransactionModule, ERPModulePermission.Read, userPermissions);
    }

    const hasCRMAccess = () => 
    {
        return PermissionsService.HasPermission(ERPModules.LeadModule, ERPModulePermission.Read, userPermissions) ||
               PermissionsService.HasPermission(ERPModules.OpportunityModule, ERPModulePermission.Read, userPermissions) ||
               PermissionsService.HasPermission(ERPModules.ContactModule, ERPModulePermission.Read, userPermissions) ||
               PermissionsService.HasPermission(ERPModules.ActivityModule, ERPModulePermission.Read, userPermissions);
    }

    return (
        <VStack align="stretch" gap={2} className="sidebar-menu">
              
            <ChakraLink onClick={ doRoute('/erp')}><FaHome style={{ marginRight: 6 }} />Home</ChakraLink>
            <Menu.Root positioning={{ placement: "right-start" }} size="md">
                <Menu.Trigger asChild hidden={!hasAccountingAcccess()}>
                    <ChakraLink><FaCashRegister style={{ marginRight: 6 }} />Accounting</ChakraLink>
                </Menu.Trigger>
                <Portal>
                    <Menu.Positioner>
                    <Menu.Content >
                        <Menu.Item value="ar" py={4} hidden={!PermissionsService.HasPermission(ERPModules.ARModule, ERPModulePermission.Read, userPermissions)}>
                            <ChakraLink fontSize="lg" onClick={ doRoute('/erp/ar')}><FaMoneyBill style={{ marginRight: 6 }} />Accounts Receivable</ChakraLink>
                        </Menu.Item>
                        <Menu.Item value="ap" py={4} hidden={!PermissionsService.HasPermission(ERPModules.APModule, ERPModulePermission.Read, userPermissions)}>
                            <ChakraLink fontSize="lg" onClick={ doRoute('/erp/ap')}><FaDollarSign style={{ marginRight: 6 }} />Accounts Payable</ChakraLink>
                        </Menu.Item>
                        <Menu.Item value="creditmemo" py={4} hidden={!PermissionsService.HasPermission(ERPModules.CreditMemoModule, ERPModulePermission.Read, userPermissions)}>
                            <ChakraLink fontSize="lg" onClick={ doRoute('/erp/creditmemos')}><CiMemoPad style={{ marginRight: 6 }} />Credit Memos</ChakraLink>
                        </Menu.Item>
                        <Menu.Item value="subscription" py={4} hidden={!PermissionsService.HasPermission(ERPModules.SubscriptionModule, ERPModulePermission.Read, userPermissions)}>
                            <ChakraLink fontSize="lg" onClick={ doRoute('/erp/subscriptions')}><FaAddressBook style={{ marginRight: 6 }} />Subscriptions</ChakraLink>
                        </Menu.Item>
                        <Menu.Item value="chartofaccounts" py={4} hidden={!PermissionsService.HasPermission(ERPModules.ChartOfAccountModule, ERPModulePermission.Read, userPermissions)}>
                            <ChakraLink fontSize="lg" onClick={ doRoute('/erp/chartofaccounts')}><FaListAlt style={{ marginRight: 6 }} />Chart of Accounts</ChakraLink>
                        </Menu.Item>
                        <Menu.Item value="journalentries" py={4} hidden={!PermissionsService.HasPermission(ERPModules.JournalEntryModule, ERPModulePermission.Read, userPermissions)}>
                            <ChakraLink fontSize="lg" onClick={ doRoute('/erp/journalentries')}><FaFileAlt style={{ marginRight: 6 }} />Journal Entries</ChakraLink>
                        </Menu.Item>
                        <Menu.Item value="financialtransactions" py={4} hidden={!PermissionsService.HasPermission(ERPModules.FinancialTransactionModule, ERPModulePermission.Read, userPermissions)}>
                            <ChakraLink fontSize="lg" onClick={ doRoute('/erp/financialtransactions')}><FaChartBar style={{ marginRight: 6 }} />Financial Transactions</ChakraLink>
                        </Menu.Item>
                    </Menu.Content>
                    </Menu.Positioner>
                </Portal>
            </Menu.Root>

            <ChakraLink hidden={!PermissionsService.HasPermission(ERPModules.CustomerModule, ERPModulePermission.Read, userPermissions)} onClick={ doRoute('/erp/customers')}><FaUserFriends style={{ marginRight: 6 }} />Customers</ChakraLink>
            <ChakraLink hidden={!PermissionsService.HasPermission(ERPModules.OrderModule, ERPModulePermission.Read, userPermissions)} onClick={ doRoute('/erp/salesorders')}><FaReceipt style={{ marginRight: 6 }} />Sales Orders</ChakraLink>
            <ChakraLink hidden={!PermissionsService.HasPermission(ERPModules.ProductionOrderModule, ERPModulePermission.Read, userPermissions)} onClick={ doRoute('/erp/productionorders')}><FaClipboardList style={{ marginRight: 6 }} />Production Orders</ChakraLink>
            <Menu.Root positioning={{ placement: "right-start" }} size="md">
                <Menu.Trigger asChild hidden={!hasCRMAccess()}>
                    <ChakraLink><FaListAlt style={{ marginRight: 6 }} />CRM</ChakraLink>
                </Menu.Trigger>
                <Portal>
                    <Menu.Positioner>
                    <Menu.Content >
                        <Menu.Item value="opportunities" py={4} hidden={!PermissionsService.HasPermission(ERPModules.OpportunityModule, ERPModulePermission.Read, userPermissions)}>
                        <ChakraLink fontSize="lg" onClick={ doRoute('/erp/opportunities')}><FaRegIdCard style={{ marginRight: 6 }} />Opportunities</ChakraLink>
                        </Menu.Item>
                        <Menu.Item value="contacts" py={4} hidden={!PermissionsService.HasPermission(ERPModules.ContactModule, ERPModulePermission.Read, userPermissions)}>
                        <ChakraLink fontSize="lg" onClick={ doRoute('/erp/contacts')}><FaUsers style={{ marginRight: 6 }} />Contacts</ChakraLink>
                        </Menu.Item>
                        <Menu.Item value="leads" py={4} hidden={!PermissionsService.HasPermission(ERPModules.LeadModule, ERPModulePermission.Read, userPermissions)}>
                        <ChakraLink fontSize="lg" onClick={ doRoute('/erp/leads')}><FaUserPlus style={{ marginRight: 6 }} />Leads</ChakraLink>
                        </Menu.Item>
                        <Menu.Item value="activities" py={4} hidden={!PermissionsService.HasPermission(ERPModules.ActivityModule, ERPModulePermission.Read, userPermissions)}>
                        <ChakraLink fontSize="lg" onClick={ doRoute('/erp/activities')}><FaUserPlus style={{ marginRight: 6 }} />Activities</ChakraLink>
                        </Menu.Item>
                    </Menu.Content>
                    </Menu.Positioner>
                </Portal>
            </Menu.Root>
            <ChakraLink hidden={!PermissionsService.HasPermission(ERPModules.PurchaseOrderModule, ERPModulePermission.Read, userPermissions)} onClick={ doRoute('/erp/purchaseorders')}><FaBoxOpen style={{ marginRight: 6 }} />Purchase Orders</ChakraLink>
            <ChakraLink hidden={!PermissionsService.HasPermission(ERPModules.PurchaseOrderReceiveModule, ERPModulePermission.Read, userPermissions)} onClick={ doRoute('/erp/poreceive')}><FaCubes style={{ marginRight: 6 }} />PO Receive</ChakraLink>
            <ChakraLink hidden={!PermissionsService.HasPermission(ERPModules.DocumentModule, ERPModulePermission.Read, userPermissions)} onClick={ doRoute('/erp/documents')}><FaFileAlt style={{ marginRight: 6 }} />Documents</ChakraLink>
            <ChakraLink hidden={!PermissionsService.HasPermission(ERPModules.ShippingModule, ERPModulePermission.Read, userPermissions)} onClick={ doRoute('/erp/shipments')}><FaTruckMoving style={{ marginRight: 6 }} />Shipments</ChakraLink>
            <ChakraLink hidden={!PermissionsService.HasPermission(ERPModules.InventoryModule, ERPModulePermission.Read, userPermissions)} onClick={ doRoute('/erp/inventory')}><FaBoxes style={{ marginRight: 6 }} />Inventory</ChakraLink>
            <ChakraLink hidden={!PermissionsService.HasPermission(ERPModules.VendorModule, ERPModulePermission.Read, userPermissions)} onClick={ doRoute('/erp/vendors')}><FaCity style={{ marginRight: 6 }} />Vendors</ChakraLink>
            <ChakraLink hidden={!PermissionsService.HasPermission(ERPModules.ProductModule, ERPModulePermission.Read, userPermissions)} onClick={ doRoute('/erp/products')}><FaTh style={{ marginRight: 6 }} />Product Catalog</ChakraLink>
            <ChakraLink href={process.env.NEXT_PUBLIC_REPORTS_URL} target="blank"><FaChartBar style={{ marginRight: 6 }} />Reports</ChakraLink>
            <ChakraLink hidden={!PermissionsService.HasPermission(ERPModules.Admin, ERPModulePermission.Read, userPermissions)} onClick={ doRoute('/erp/admin')}><FaCogs style={{ marginRight: 6 }} />Administration</ChakraLink>
            
        </VStack>
    )
}

export default SidebarComponent;