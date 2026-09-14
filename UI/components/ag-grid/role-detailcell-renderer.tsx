
import { Button, CloseButton, Dialog, Portal, Spinner } from "@chakra-ui/react";
import { ColDef } from "ag-grid-community";
import { AgGridReact } from "ag-grid-react";
import { useEffect, useState } from "react";
import { MdEditDocument } from "react-icons/md";

import { ModuleObjectDto } from "@/models/key-value-models";
import { ModulePermissionDeleteCommand, RoleDeleteCommand, RoleModulePermissionCreateCommand, RoleModulePermissionEditCommand, RolePermissionsDto } from "@/models/user-models";
import { userService } from "@/services/user-service";
import { useKeycloak } from '@react-keycloak/web';

export function RoleDetailRenderer(params: any)
{
    const { keycloak } = useKeycloak();

    const [isDialogOpen, setIsDialogOpen] = useState(false);
    const [isWorking, setIsWorking] = useState(false);

    const [rowId, setRowId] = useState<number>(0);
    const [deleteRowId, setDeleteRowId] = useState<number>(0);

    const [rows, setRows] = useState<PermissionRow[]>([]);
    const [colDefs, setColDefs] = useState<ColDef<PermissionRow>[]>([]);
    const [moduleData, setModuleData] = useState<ModuleObjectDto[]>([]);
    
    const [isDeleteRolePermissionDialogOpen, setIsDeleteRolePermissionDialogOpen] = useState(false);
    const [isDeleteRoleDialogOpen, setIsDeleteRoleDialogOpen] = useState(false);

    // Callback
    const onRoleDelete = params.onRoleDelete;


    interface PermissionRow
    {
        id: number;
        module_id: string;
        module_name: string;
        read: boolean;
        write: boolean;
        edit: boolean;
        delete: boolean;
        isDirty: boolean;
    }

    useEffect(() => {

        if(params == undefined || params.length == 0 || params.value == undefined)
        {
            return;
        }

        //console.log("PARAMS_RoleCellRenderer: ", params);

        let data = params.value as RolePermissionsDto[];

        if(data.length > 0 && data[0].role_id)
        {
            setRowId(data[0].role_id);
        }
        
        if(params.data != undefined && params.data.role_id != undefined)
        {
            setRowId(params.data.role_id);
        }
        

        let buffer_rows = new Array<PermissionRow>();

        for(let i=0; i<data.length; i++)
        {
            let row = {
                id: data[i].id,
                module_id: data[i].module_id,
                module_name: data[i].module_name,
                read: data[i].read,
                edit: data[i].edit,
                write: data[i].write,
                delete: data[i].delete,
                isDirty: false
            } as PermissionRow;

            buffer_rows.push(row);
        }

        setRows(buffer_rows);

        if(params.moduleData && params.moduleData.length > 0)
        {
            setModuleData(params.moduleData);
        }
        

    }, [params]);

    useEffect(() => {
        
        if(moduleData == undefined)
        {
            return;
        }

        //console.log("moduleData_RoleCellRenderer: ", moduleData);

        setColDefs([
            { field: "module_name", headerName: "Module Name", 
                editable: true,
                singleClickEdit: true,
                cellEditor: 'agSelectCellEditor',
                cellEditorParams: {
                    values: moduleData.map((i) => {
                        return i.name
                    }),
                    valueListGap: 20
                },
                onCellValueChanged: onCellChanged,
                minWidth: 125
            },
            { field: "read", headerName: "Read", editable: true, onCellValueChanged: onCellChanged},
            { field: "write", headerName: "Write",  editable: true, onCellValueChanged: onCellChanged},
            { field: "edit", headerName: "Edit", editable: true, onCellValueChanged: onCellChanged},
            { field: "delete", headerName: "Delete", editable: true, onCellValueChanged: onCellChanged},
            {
                field: "module_id",
                headerName: "Actions",
                cellRenderer: (props: any) => {
                    return ( 
                        <div>
                            <Button type="button" colorPalette="red" onClick={() => handleDeleteRolePermissionClick(props.data)}>Delete</Button>
                        </div>
                    );
                }
            }
        ])

    }, [rows, moduleData]);

    const onCellChanged = (event: any) => 
    {
        let the_id = Number(event.data.id);
        let record = event.data;
        record.isDirty = true;

        if(event.data.module_name != "")
        {
            let module = moduleData.filter(m => m.name == event.data.module_name);

            if(module != null && module.length > 0)
            {
                record.module_id = module[0].module_id;
            }
        }

        //console.log(record);

        setRows(prev => prev.map(row => 
            row.id === the_id 
                ? { ...row, ...record }
                : row
        ));
    }

    const handleEditClick = () =>
    {
        //console.log(params);

        setIsDialogOpen(true);
    }

    const doSave = () => {
        setIsWorking(true);

        for(let i=0; i<rows.length; i++)
        {
            let record = rows[i];
            if(record.isDirty == true && record.module_id != "")
            {
                //console.log(record);

                if(record.id == 0)
                {
                    let new_command = new RoleModulePermissionCreateCommand();
                    new_command.module_id = record.module_id;
                    new_command.role_id = rowId;
                    new_command.read = record.read;
                    new_command.edit = record.edit;
                    new_command.write = record.write;
                    new_command.delete = record.delete;
                    
                    userService.createNewRoleModulePermission(new_command, keycloak?.token || "").then( (response) => {
                        //console.log(response)
                    });
                }
                else
                {
                    let update_command = new RoleModulePermissionEditCommand();
                    update_command.module_id = record.module_id;
                    update_command.id = record.id;
                    update_command.read = record.read;
                    update_command.edit = record.edit;
                    update_command.write = record.write;
                    update_command.delete = record.delete;
                    
                    userService.updateRoleModulePermission(update_command, keycloak?.token || "").then( (response) => {
                        //console.log(response)
                    });
                }

            }
        }

        setIsWorking(false);
    }

    const doAddPermission = () => {
        setRows(prev => [...prev, {id: 0, module_id: "", module_name: "", read: false, edit: false, write: false, delete: false, isDirty: true}]);
    }

    const handleDeleteRolePermissionClick = (value: PermissionRow) => {
        //console.log(value);
        setDeleteRowId(value.id);
        setIsDeleteRolePermissionDialogOpen(true);
    }

    const handleDeleteRoleClick = () => {
        //console.log(params.data.role_id);
        setDeleteRowId(params.data.role_id);
        setIsDeleteRoleDialogOpen(true);
    }

    const doDeleteRolePermission = () =>
    {
        let command = new ModulePermissionDeleteCommand();
        command.id = deleteRowId;

        setIsWorking(true);

        userService.deleteRoleModulePermission(command, keycloak?.token || "").then( (response) =>
        {
            setRows(prev => prev.filter(line => line.id !== deleteRowId));
            setIsDeleteRolePermissionDialogOpen(false);
            setIsWorking(false);
        });
    }

    const doDeleteRoleClick = () => {
        let command = new RoleDeleteCommand();
        command.id = deleteRowId;

        setIsWorking(true);

        userService.deleteRole(command, keycloak?.token || "").then( (response) =>
        {
            setIsDeleteRoleDialogOpen(false);
            setIsWorking(false);

            if (onRoleDelete && typeof onRoleDelete === 'function') {
                onRoleDelete(deleteRowId);
            }
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
                <Button type="button" colorPalette="green" onClick={() => handleEditClick()}><MdEditDocument />Edit</Button> &nbsp;<Button type="button" colorPalette="red" onClick={() => handleDeleteRoleClick()} >Delete</Button>
            </div>
            <Dialog.Root open={isDialogOpen} onOpenChange={(details) => setIsDialogOpen(details.open)} size="xl">
                <Portal>
                    <Dialog.Backdrop />
                    <Dialog.Positioner>
                    <Dialog.Content>
                        <Dialog.Header>
                        <Dialog.Title>Edit Role Permissions</Dialog.Title>
                        </Dialog.Header>
                        <Dialog.Body>
                            <div style={{ width: "100%", height: "300px", marginBottom: "35px" }}>
                                <Button colorPalette="blue" onClick={() => doAddPermission()} >Add Permission</Button>
                                <AgGridReact
                                    rowData={rows}
                                    columnDefs={colDefs}
                                    defaultColDef={defaultColDef}
                                />
                            </div>
                        </Dialog.Body>
                        <Dialog.Footer>
                        <Dialog.ActionTrigger asChild>
                            <Button variant="outline">Cancel</Button>
                        </Dialog.ActionTrigger>
                        <Button colorPalette="blue" onClick={() => doSave()} ><Spinner hidden={!isWorking} /> Save</Button>
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
                        <Button colorPalette="red" onClick={() => doDeleteRoleClick()} ><Spinner hidden={!isWorking} /> Delete</Button>
                        </Dialog.Footer>
                        <Dialog.CloseTrigger asChild>
                            <CloseButton size="sm" />
                        </Dialog.CloseTrigger>
                    </Dialog.Content>
                    </Dialog.Positioner>
                </Portal>
            </Dialog.Root>

                
            <Dialog.Root open={isDeleteRolePermissionDialogOpen} onOpenChange={(details) => setIsDeleteRolePermissionDialogOpen(details.open)} role="alertdialog">
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
                        <Button colorPalette="red" onClick={() => doDeleteRolePermission()} ><Spinner hidden={!isWorking} /> Delete</Button>
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
