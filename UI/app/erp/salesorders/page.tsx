"use client"

import '../../styles/page.component.css'


import { AllCommunityModule, ColDef, ModuleRegistry, CsvExportModule } from "ag-grid-community";
import { Button } from '@chakra-ui/react'
import { useRouter } from 'next/navigation';
import SalesOrdersListComponent from '@/components/lists/sales-orders-list-component';
import { useAuth } from '@/lib/auth/auth-context';
import { permissionsService, ERPModules, ERPModulePermission } from '@/services/permissions-service';
import { useEffect, useState } from 'react';

ModuleRegistry.registerModules([AllCommunityModule]);


function SalesOrdersPage() {
  const auth = useAuth();
  const router = useRouter();
  const [hasAccess, setHasAccess] = useState(true);

  useEffect(() => {
    if (auth.authenticated == false) return;

    // Check permission
    const realmRoles = auth.roles || [];
    const hasPermission = permissionsService.HasPermission(
      ERPModules.OrderModule,
      ERPModulePermission.Read,
      realmRoles
    );

    if (!hasPermission) {
      setHasAccess(false);
      router.push('/erp');
      return;
    }
  }, [auth.authenticated]);

  const handleNewClick = () => {
    router.push("/erp/salesorders/new");
  };

  if (!hasAccess) {
    return <div>Redirecting...</div>;
  }

  return (
    <div style={{ width: "100%"}}>
        <div style={{paddingBottom: "25px"}}>
          <div style={{ width: "49%", display: "inline-block" }}>
            <h1>Sales Orders</h1>
          </div>
          <div style={{ width: "49%", display: "inline-block", textAlign: "right" }}>
            <Button type="submit" colorPalette="blue" onClick={handleNewClick}>New Order</Button>
          </div>
        </div>
        <SalesOrdersListComponent customer_id={null} onChange={() => {}} />
    </div>
  );
}


export default SalesOrdersPage;