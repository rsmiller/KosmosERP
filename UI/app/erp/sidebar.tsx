import { FaHome, FaCogs, FaMoneyBill, FaChartBar, FaCubes, FaFileAlt, FaTruckMoving,
          FaUserFriends, FaCity, FaBoxOpen, FaDollarSign, FaListAlt, FaReceipt, FaTh,
          FaClipboardList, FaUsers, FaUserPlus, FaRegIdCard, FaCashRegister,
          FaBoxes, FaAddressBook, FaChevronDown, FaChevronRight, FaCog } from "react-icons/fa";
import { CiMemoPad } from "react-icons/ci";
import { Link as ChakraLink, Menu, Portal, VStack } from "@chakra-ui/react";
import NextLink from "next/link";
import { ERPModulePermission, ERPModules, permissionsService } from '@/services/permissions-service';
import { ReactNode, useEffect, useState } from "react";
import { useAuth } from "@/lib/auth/auth-context";

// Real links (with href) so navigation is accessible: role=link, middle-click, open in new tab.
function NavLink({ href, hidden, children }: { href: string, hidden?: boolean, children: ReactNode })
{
    return (
        <ChakraLink asChild hidden={hidden}>
            <NextLink href={href}>{children}</NextLink>
        </ChakraLink>
    );
}

function NavMenuItem({ value, href, hidden, children }: { value: string, href: string, hidden?: boolean, children: ReactNode })
{
    return (
        <Menu.Item value={value} py={4} fontSize="lg" hidden={hidden} asChild>
            <NextLink href={href}>{children}</NextLink>
        </Menu.Item>
    );
}

function SidebarComponent({ onNavigate } : any)
{
    const auth = useAuth();

    const PermissionsService = permissionsService;
    const [userPermissions, setUserPermissions] = useState<string[]>([]);
    const [isAdmin, setIsAdmin] = useState<boolean>(false);


    useEffect(() => {
        setUserPermissions(auth.roles || []);
    }, [auth.roles]);

    const canRead = (module: string) => PermissionsService.HasPermission(module, ERPModulePermission.Read, userPermissions);

    const hasAccountingAcccess = () =>
    {
        return canRead(ERPModules.ARModule) ||
               canRead(ERPModules.APModule) ||
               canRead(ERPModules.CreditMemoModule) ||
               canRead(ERPModules.GeneralLedgerModule) ||
               canRead(ERPModules.SubscriptionModule) ||
               canRead(ERPModules.ChartOfAccountModule) ||
               canRead(ERPModules.JournalEntryModule) ||
               canRead(ERPModules.FinancialTransactionModule);
    }

    const hasCRMAccess = () =>
    {
        return canRead(ERPModules.LeadModule) ||
               canRead(ERPModules.OpportunityModule) ||
               canRead(ERPModules.ContactModule) ||
               canRead(ERPModules.ActivityModule);
    }

    return (
        <VStack as="nav" aria-label="Main" align="stretch" gap={2} className="sidebar-menu">

            <NavLink href="/erp"><FaHome style={{ marginRight: 6 }} />Home</NavLink>
            <Menu.Root positioning={{ placement: "right-start" }} size="md">
                <Menu.Trigger asChild hidden={!hasAccountingAcccess()} >
                    <ChakraLink as="button"><FaCashRegister style={{ marginRight: 6}} className="chakra-link" />Accounting <FaChevronRight style={{ marginLeft: 55 }} /></ChakraLink>
                </Menu.Trigger>
                <Portal>
                    <Menu.Positioner>
                    <Menu.Content >
                        <NavMenuItem value="ar" href="/erp/ar" hidden={!canRead(ERPModules.ARModule)}><FaMoneyBill style={{ marginRight: 6 }} />Accounts Receivable</NavMenuItem>
                        <NavMenuItem value="ap" href="/erp/ap" hidden={!canRead(ERPModules.APModule)}><FaDollarSign style={{ marginRight: 6 }} />Accounts Payable</NavMenuItem>
                        <NavMenuItem value="creditmemo" href="/erp/creditmemos" hidden={!canRead(ERPModules.CreditMemoModule)}><CiMemoPad style={{ marginRight: 6 }} />Credit Memos</NavMenuItem>
                        <NavMenuItem value="subscription" href="/erp/subscriptions" hidden={!canRead(ERPModules.SubscriptionModule)}><FaAddressBook style={{ marginRight: 6 }} />Subscriptions</NavMenuItem>
                        <NavMenuItem value="chartofaccounts" href="/erp/chartofaccounts" hidden={!canRead(ERPModules.ChartOfAccountModule)}><FaListAlt style={{ marginRight: 6 }} />Chart of Accounts</NavMenuItem>
                        <NavMenuItem value="journalentries" href="/erp/journalentries" hidden={!canRead(ERPModules.JournalEntryModule)}><FaFileAlt style={{ marginRight: 6 }} />Journal Entries</NavMenuItem>
                        <NavMenuItem value="financialtransactions" href="/erp/financialtransactions" hidden={!canRead(ERPModules.FinancialTransactionModule)}><FaChartBar style={{ marginRight: 6 }} />Financial Transactions</NavMenuItem>
                    </Menu.Content>
                    </Menu.Positioner>
                </Portal>
            </Menu.Root>

            <NavLink href="/erp/customers" hidden={!canRead(ERPModules.CustomerModule)}><FaUserFriends style={{ marginRight: 6 }} />Customers</NavLink>
            <NavLink href="/erp/salesorders" hidden={!canRead(ERPModules.OrderModule)}><FaReceipt style={{ marginRight: 6 }} />Sales Orders</NavLink>
            <NavLink href="/erp/productionorders" hidden={!canRead(ERPModules.ProductionOrderModule)}><FaClipboardList style={{ marginRight: 6 }} />Production Orders</NavLink>
            <Menu.Root positioning={{ placement: "right-start" }} size="md">
                <Menu.Trigger asChild hidden={!hasCRMAccess()}>
                    <ChakraLink as="button"><FaListAlt style={{ marginRight: 6 }} />CRM <FaChevronRight style={{ marginLeft: 100 }} /></ChakraLink>
                </Menu.Trigger>
                <Portal>
                    <Menu.Positioner>
                    <Menu.Content >
                        <NavMenuItem value="opportunities" href="/erp/opportunities" hidden={!canRead(ERPModules.OpportunityModule)}><FaRegIdCard style={{ marginRight: 6 }} />Opportunities</NavMenuItem>
                        <NavMenuItem value="contacts" href="/erp/contacts" hidden={!canRead(ERPModules.ContactModule)}><FaUsers style={{ marginRight: 6 }} />Contacts</NavMenuItem>
                        <NavMenuItem value="leads" href="/erp/leads" hidden={!canRead(ERPModules.LeadModule)}><FaUserPlus style={{ marginRight: 6 }} />Leads</NavMenuItem>
                        <NavMenuItem value="activities" href="/erp/activities" hidden={!canRead(ERPModules.ActivityModule)}><FaCog style={{ marginRight: 6 }} />Activities</NavMenuItem>
                    </Menu.Content>
                    </Menu.Positioner>
                </Portal>
            </Menu.Root>
            <NavLink href="/erp/purchaseorders" hidden={!canRead(ERPModules.PurchaseOrderModule)}><FaBoxOpen style={{ marginRight: 6 }} />Purchase Orders</NavLink>
            <NavLink href="/erp/poreceive" hidden={!canRead(ERPModules.PurchaseOrderReceiveModule)}><FaCubes style={{ marginRight: 6 }} />PO Receive</NavLink>
            <NavLink href="/erp/documents" hidden={!canRead(ERPModules.DocumentModule)}><FaFileAlt style={{ marginRight: 6 }} />Documents</NavLink>
            <NavLink href="/erp/shipments" hidden={!canRead(ERPModules.ShippingModule)}><FaTruckMoving style={{ marginRight: 6 }} />Shipments</NavLink>
            <NavLink href="/erp/inventory" hidden={!canRead(ERPModules.InventoryModule)}><FaBoxes style={{ marginRight: 6 }} />Inventory</NavLink>
            <NavLink href="/erp/vendors" hidden={!canRead(ERPModules.VendorModule)}><FaCity style={{ marginRight: 6 }} />Vendors</NavLink>
            <NavLink href="/erp/products" hidden={!canRead(ERPModules.ProductModule)}><FaTh style={{ marginRight: 6 }} />Product Catalog</NavLink>
            <NavLink href="/erp/reports"><FaChartBar style={{ marginRight: 6 }} />Reports</NavLink>
            <NavLink href="/erp/admin" hidden={!canRead(ERPModules.Admin)}><FaCogs style={{ marginRight: 6 }} />Administration</NavLink>

        </VStack>
    )
}

export default SidebarComponent;
