"use client"

import '../../styles/page.component.css'


import { Button } from '@chakra-ui/react'
import { useRouter } from 'next/navigation';
import PurchaseOrdersListComponentPage from '@/components/lists/purchase-orders-list-component';
import { useKeycloak } from '@react-keycloak/web';
import { permissionsService, ERPModules, ERPModulePermission } from '@/services/permissions-service';
import { useEffect, useState } from 'react';


function PurchaseOrdersPage() {
  const { keycloak } = useKeycloak();
  const router = useRouter();
  const [hasAccess, setHasAccess] = useState(true);

  useEffect(() => {
    if (keycloak.authenticated == false) return;

    // Check permission
    const realmRoles = keycloak?.tokenParsed?.realm_access?.roles || [];
    const hasPermission = permissionsService.HasPermission(
      ERPModules.PurchaseOrderModule,
      ERPModulePermission.Read,
      realmRoles
    );

    if (!hasPermission) {
      setHasAccess(false);
      router.push('/erp');
      return;
    }
  }, [keycloak.authenticated]);

  const handleNewClick = () => {
    router.push("/erp/purchaseorders/new");
  };

  if (!hasAccess) {
    return <div>Redirecting...</div>;
  }


  return (
    <div style={{ width: "100%", height: "500px" }}>
        <div style={{paddingBottom: "25px"}}>
          <div style={{ width: "49%", display: "inline-block" }}>
            <h1>Purchase Orders</h1>
          </div>
          <div style={{ width: "49%", display: "inline-block", textAlign: "right" }}>
            <Button type="submit" colorPalette="blue" onClick={handleNewClick}>New Purchase Order</Button>
          </div>
        </div>
        <PurchaseOrdersListComponentPage vendor_id={null} onChange={() => {}} />
    </div>
  );
}


export default PurchaseOrdersPage;