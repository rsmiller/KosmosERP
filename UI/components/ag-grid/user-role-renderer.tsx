import { RoleDeleteCommand, RolePermissionsDto } from "@/models/user-models";
import { userService } from "@/services/user-service";
import { Button, CloseButton, Dialog, Portal, Spinner } from "@chakra-ui/react";
import { useAuth } from '@/lib/auth/auth-context';
import { AllCommunityModule, ColDef, ModuleRegistry } from "ag-grid-community";
import { AgGridReact } from "ag-grid-react";
import { useEffect, useState } from "react";


ModuleRegistry.registerModules([AllCommunityModule]);

export function UserRoleRenderer(params: any)
{
    const auth = useAuth();
    //console.log(params);
    

    const [isDialogViewOpen, setIsDialogViewOpen] = useState(false);
    const [isDeleteRoleDialogOpen, setIsDeleteRoleDialogOpen] = useState(false);
    const [isWorking, setIsWorking] = useState(false);

    const [colDefs, setColDefs] = useState<ColDef<RolePermissionsDto>[]>([]);
    const [rows, setRows] = useState<RolePermissionsDto[]>([]);
    
    useEffect(() => {
        setRows(params.value);

        setColDefs([
            { field: "module_name", headerName: "Module Name" },
            { field: "read", headerName: "Read" },
            { field: "write", headerName: "Write" },
            { field: "edit", headerName: "Edit" },
            { field: "delete", headerName: "Delete" },
        ])
        
    }, []);



    const openDemPermissions = () => {
        setIsDialogViewOpen(true)
    }

    const handleDeleteRoleClick = () => {
        if(params.data.role_name == "Administrators")
        {
            alert("You can not delete permissions from the administrators role")
            return;
        }

        setIsDeleteRoleDialogOpen(true);
    }

    const doDeleteRole = () => {
        let command = new RoleDeleteCommand();
        command.id = params.data?.role_id || params.data?.id || 0;

        setIsWorking(true);

        userService.deleteRole(command, auth.token || "").then((response) => {
            setIsWorking(false);
            setIsDeleteRoleDialogOpen(false);
            // optionally refresh view or notify parent
        });
    }
    
    const defaultColDef: ColDef = {
        flex: 1,
        filter: false,
        sortable: true,
    };

    return (
            <div>
                <div style={{ width: "100%", textAlign: "center" }}>
                    <Button type="button" colorPalette="black" onClick={() => openDemPermissions()}>View Permissions</Button> &nbsp;<Button type="button" colorPalette="red" onClick={() => handleDeleteRoleClick()} >Delete</Button>
                </div>

                <Dialog.Root open={isDialogViewOpen} onOpenChange={(details) => setIsDialogViewOpen(details.open)} size="lg">
                    <Portal>
                        <Dialog.Backdrop />
                        <Dialog.Positioner>
                        <Dialog.Content>
                            <Dialog.Header>
                                <Dialog.Title>Role Permissions</Dialog.Title>
                            </Dialog.Header>
                            <Dialog.Body>
                                <div style={{ width: "100%", height: "300px", marginBottom: "35px" }}>
                                    <AgGridReact
                                        rowData={rows}
                                        columnDefs={colDefs}
                                        defaultColDef={defaultColDef}
                                    />
                                </div>
                            </Dialog.Body>
                            <Dialog.Footer>
                            <Dialog.ActionTrigger asChild>
                                <Button variant="outline">Close</Button>
                            </Dialog.ActionTrigger>
                            
                            </Dialog.Footer>
                            <Dialog.CloseTrigger asChild>
                                <CloseButton size="sm" />
                            </Dialog.CloseTrigger>
                        </Dialog.Content>
                        </Dialog.Positioner>
                    </Portal>
                </Dialog.Root>

                <Dialog.Root open={isDeleteRoleDialogOpen} onOpenChange={(details) => setIsDeleteRoleDialogOpen(details.open)} role="alertdialog">
                    <Portal>
                        <Dialog.Backdrop />
                        <Dialog.Positioner>
                        <Dialog.Content>
                            <Dialog.Header>
                            <Dialog.Title>Are you sure?</Dialog.Title>
                            </Dialog.Header>
                            <Dialog.Body>
                            <p>
                                Are you sure you want to delete this entry?
                            </p>
                            </Dialog.Body>
                            <Dialog.Footer>
                            <Dialog.ActionTrigger asChild>
                                <Button variant="outline">Cancel</Button>
                            </Dialog.ActionTrigger>
                            <Button colorPalette="red" onClick={() => doDeleteRole()} ><Spinner hidden={!isWorking} /> Delete</Button>
                            </Dialog.Footer>
                            <Dialog.CloseTrigger asChild>
                                <CloseButton size="sm" />
                            </Dialog.CloseTrigger>
                        </Dialog.Content>
                        </Dialog.Positioner>
                    </Portal>
                </Dialog.Root>
            </div>
        );
}
