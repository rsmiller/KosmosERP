import { FaHome, FaCogs, FaMoneyBill, FaChartBar, FaCubes, FaFileAlt, FaTruckMoving,
          FaUserFriends, FaCity, FaBoxOpen, FaDollarSign, FaListAlt, FaReceipt, FaTh } from "react-icons/fa";
import "../globals.css";
import "../styles/sidebar.component.css";
import "react-datepicker/dist/react-datepicker.css";
import { Provider } from "@/components/ui/provider"
import { Box, Flex, VStack, Link, Text } from '@chakra-ui/react';


export default function ErpLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <Flex minH="100vh">
      <Box
        w="240px"
        color="black"
        p={5}
        boxShadow="lg"
        position="sticky"
        top={0}
        h="100vh"
      >
        <VStack align="stretch" gap={2} className="sidebar-menu">
          <Text fontWeight="bold" fontSize="xl" mb={6}>Kosmos ERP</Text>
          <Link href="/erp"><FaHome style={{ marginRight: 6 }} />Home</Link>
          <Link href="/erp/ar"><FaMoneyBill style={{ marginRight: 6 }} />Accounts Receivable</Link>
          <Link href="/erp/ap"><FaDollarSign style={{ marginRight: 6 }} />Accounts Payable</Link>
          <Link href="/erp/customers"><FaUserFriends style={{ marginRight: 6 }} />Customers</Link>
          <Link href="/erp/salesorders"><FaReceipt style={{ marginRight: 6 }} />Sales Orders</Link>
          <Link href="/erp/crm"><FaListAlt style={{ marginRight: 6 }} />CRM</Link>
          <Link href="/erp/purchaseorders"><FaBoxOpen style={{ marginRight: 6 }} />Purchase Orders</Link>
          <Link href="/erp/poreceive"><FaCubes style={{ marginRight: 6 }} />PO Receive</Link>
          <Link href="/erp/documents"><FaFileAlt style={{ marginRight: 6 }} />Documents</Link>
          <Link href="/erp/shipments"><FaTruckMoving style={{ marginRight: 6 }} />Shipments</Link>
          <Link href="/erp/vendors"><FaCity style={{ marginRight: 6 }} />Vendors</Link>
          <Link href="/erp/products"><FaTh style={{ marginRight: 6 }} />Product Catalog</Link>
          <Link href="/erp/admin"><FaChartBar style={{ marginRight: 6 }} />Reports</Link>
          <Link href="/erp/admin"><FaCogs style={{ marginRight: 6 }} />Administration</Link>
        </VStack>
      </Box>

      {/* Main Content */}
      <Box flex={1} p={8} bg="gray.50">
        <Provider>{children}</Provider>
      </Box>
    </Flex>
  );
}
