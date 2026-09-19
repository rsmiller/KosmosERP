
"use client"

import '../../styles/page.component.css'

import { AllCommunityModule, ModuleRegistry } from "ag-grid-community";
import { Button } from '@chakra-ui/react'
import { useRouter } from 'next/navigation';
import CreditMemosListComponent from '@/components/lists/credit-memos-list-component';
import { useAuth } from '@/lib/auth/auth-context';
import { permissionsService, ERPModules, ERPModulePermission } from '@/services/permissions-service';
import { useEffect, useState } from 'react';

ModuleRegistry.registerModules([AllCommunityModule]);

function CreditMemosPage() {
	const auth = useAuth();
	const router = useRouter();
	const [hasAccess, setHasAccess] = useState(true);

	useEffect(() => {
		if (auth.authenticated == false) return;

		// Check permission
		const realmRoles = auth.roles || [];
		const hasPermission = permissionsService.HasPermission(
			ERPModules.CreditMemoModule,
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
		router.push("/erp/creditmemos/new");
	};

	if (!hasAccess) {
		return <div>Redirecting...</div>;
	}

	return (
		<div style={{ width: "100%"}}>
				<div style={{paddingBottom: "25px"}}>
					<div style={{ width: "49%", display: "inline-block" }}>
						<h1>Credit Memos</h1>
					</div>
					<div style={{ width: "49%", display: "inline-block", textAlign: "right" }}>
						<Button type="submit" colorPalette="blue" onClick={handleNewClick}>New Credit Memo</Button>
					</div>
				</div>
				<CreditMemosListComponent customer_id={null} onChange={() => {}} />
		</div>
	);
}

export default CreditMemosPage;