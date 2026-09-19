"use client"

import { Avatar, Button, Card, Stack } from "@chakra-ui/react";
import { FaRegUser, FaThList } from "react-icons/fa";
import { IoDocumentText, IoSettings } from "react-icons/io5";
import { useRouter } from 'next/navigation';
import { FaUsersViewfinder } from "react-icons/fa6";
import { useAuth } from '@/lib/auth/auth-context';
import { HiAdjustments } from "react-icons/hi";
import { permissionsService, ERPModules, ERPModulePermission } from '@/services/permissions-service';
import { useEffect, useState } from "react";

function AdminDashboardPage() {
    const router = useRouter();
    const auth = useAuth();
    const [hasAccess, setHasAccess] = useState(true);

    useEffect(() => {
        if (auth.authenticated == false) return;

        // Check permission
        const realmRoles = auth.roles || [];
        const hasPermission = permissionsService.HasPermission(
            ERPModules.Admin,
            ERPModulePermission.Read,
            realmRoles
        );

        if (!hasPermission) {
            setHasAccess(false);
            router.push('/erp');
            return;
        }
    }, [auth.authenticated]);

    if (!hasAccess) {
        return <div>Redirecting...</div>;
    }

    return (
        <Stack gap="4" direction="row" wrap="wrap">
            <Card.Root width="320px" variant="elevated">
                <Card.Body gap="2" alignContent="center" alignItems="center">
                    <Avatar.Root variant="subtle" size="xl">
                        <FaRegUser/>
                    </Avatar.Root>
                    <Card.Title mb="2">Users</Card.Title>
                    <Card.Description>
                        Add, modify, or disable users.
                    </Card.Description>
                </Card.Body>
                <Card.Footer justifyContent="center">
                    <Button variant="outline" onClick={() =>  { router.push("/erp/admin/users"); }}>Edit</Button>
                </Card.Footer>
            </Card.Root>

            <Card.Root width="320px" variant="elevated">
                <Card.Body gap="2" alignContent="center" alignItems="center">
                    <Avatar.Root variant="subtle" size="xl">
                        <FaUsersViewfinder/>
                    </Avatar.Root>
                    <Card.Title mb="2">Roles</Card.Title>
                    <Card.Description>
                        Add, modify, or disable system roles.
                    </Card.Description>
                </Card.Body>
                <Card.Footer justifyContent="center">
                    <Button variant="outline" onClick={() =>  { router.push("/erp/admin/roles"); }}>Edit</Button>
                </Card.Footer>
            </Card.Root>

            <Card.Root width="320px" variant="elevated">
                <Card.Body gap="2" alignContent="center" alignItems="center">
                    <Avatar.Root variant="subtle" size="xl">
                        <FaThList/>
                    </Avatar.Root>
                    <Card.Title mb="2">Object Lists</Card.Title>
                    <Card.Description>
                        These are common values of objects found in drop down lists. Edit or create new ones here.
                    </Card.Description>
                </Card.Body>
                <Card.Footer justifyContent="center">
                    <Button variant="outline" onClick={() =>  { router.push("/erp/admin/lists"); }}>Edit</Button>
                </Card.Footer>
            </Card.Root>

            <Card.Root width="320px" variant="elevated">
                <Card.Body gap="2" alignContent="center" alignItems="center">
                    <Avatar.Root variant="subtle" size="xl">
                        <IoDocumentText/>
                    </Avatar.Root>
                    <Card.Title mb="2">Document Types</Card.Title>
                    <Card.Description>
                        Add, modify, or disable document types and their associated required fields.
                    </Card.Description>
                </Card.Body>
                <Card.Footer justifyContent="center">
                    <Button variant="outline" onClick={() =>  { router.push("/erp/admin/document-types"); }}>Edit</Button>
                </Card.Footer>
            </Card.Root>

            <Card.Root width="320px" variant="elevated">
                <Card.Body gap="2" alignContent="center" alignItems="center">
                    <Avatar.Root variant="subtle" size="xl">
                        <IoSettings />
                    </Avatar.Root>
                    <Card.Title mb="2">Settings</Card.Title>
                    <Card.Description>
                        These are basic settings and configurations of the ERP.
                    </Card.Description>
                </Card.Body>
                <Card.Footer justifyContent="center">
                    <Button variant="outline" onClick={() =>  { router.push("/erp/admin/settings"); }}>Edit</Button>
                </Card.Footer>
            </Card.Root>

            <Card.Root width="320px" variant="elevated">
                <Card.Body gap="2" alignContent="center" alignItems="center">
                    <Avatar.Root variant="subtle" size="xl">
                        <HiAdjustments />
                    </Avatar.Root>
                    <Card.Title mb="2">Adjustments</Card.Title>
                    <Card.Description>
                        Perform manual transcational adjustments to inventory.
                    </Card.Description>
                </Card.Body>
                <Card.Footer justifyContent="center">
                    <Button variant="outline" onClick={() =>  { router.push("/erp/admin/adjustments"); }}>Edit</Button>
                </Card.Footer>
            </Card.Root>
        </Stack>
    )
}

export default AdminDashboardPage;