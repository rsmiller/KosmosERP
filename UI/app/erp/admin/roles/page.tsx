"use client"

import SessionStorage from "@/components/session-storage";
import { userService } from "@/services/user-service";
import { useKeycloak } from '@react-keycloak/web';
import { useEffect, useState, useRef } from "react";
import { AllCommunityModule, ColDef, ModuleRegistry } from "ag-grid-community";
import { RoleCreateCommand, RoleDto } from "@/models/user-models";
import { AgGridReact } from "ag-grid-react";
import { RoleDetailRenderer } from "@/components/ag-grid/role-detailcell-renderer";
import { keyValueService } from "@/services/keyvalue-service";
import { Button, CloseButton, Dialog, Field, Input, Portal, Spinner } from "@chakra-ui/react";
import { useForm } from "react-hook-form";

ModuleRegistry.registerModules([AllCommunityModule]);


function AdminRolesPage() {
    const { keycloak } = useKeycloak();
    const userId = SessionStorage.getUserId();
    const sessionId = SessionStorage.getSession();

    const [isDialogOpen, setIsDialogOpen] = useState(false);
    const [isWorking, setIsWorking] = useState(false);
    const [loading, setLoading] = useState(true);
    const [saveable, canSave] = useState(false);

    const [modules, setModules] = useState<Module[]>();
    const [roles, setRoles] = useState<RoleDto[]>([]);
    const hasInitialized = useRef(false);
    

    const {
        register,
        formState: { errors, isValid },
        setValue,
        watch,
    } = useForm<RoleCreateCommand>({
        mode: 'onChange',
    });

    interface Module
    {
        module_id: string;
        name: string;
    }

    
    useEffect(() => {
        if(keycloak.authenticated == false) return;

        if (hasInitialized.current) return;
        hasInitialized.current = true;

        setLoading(true);
        userService.getRoles(keycloak?.token || "").then( (role_response) => 
        {
            //console.log("getRoles:", role_response);

            if(role_response.success && role_response.data)
            {
                setRoles(role_response.data)
            }
            

            keyValueService.getModuleInfo(keycloak.token || "").then( (module_response) =>
            {
                //console.log("getModuleInfo:", module_response);
                
                if(module_response.success && module_response.data)
                {
                    let bufferList = new Array<Module>();
                    for(let i=0; i<module_response.data?.length; i++)
                    {
                        let module_id = module_response.data[i].module_id;

                        if(module_response.data && module_response.data[i].module_id)
                        {
                            var found = bufferList.filter(m => m.module_id == module_id);

                            if(found == undefined || found.length == 0)
                            {
                                let mod = {
                                    module_id: module_response.data[i].module_id,
                                    name: module_response.data[i].module_name
                                } as Module;

                                bufferList.push(mod);
                            }
                        }
                    }

                    setLoading(false);
                    setModules(bufferList);
                    
                }
            });
        });
    }, [keycloak.authenticated]);

    
    useEffect(() => {

        setColDefs(
            [
                { field: "name", headerName: "Role Name" },
                { field: "role_permissions", headerName: "Role Permissions", 
                    cellRenderer: RoleDetailRenderer,
                    cellRendererParams: {
                        moduleData: modules,
                        onRoleDelete: handleDeleteClick
                    },
                },
            ]
        );

    }, [modules])

    const doAddRole = () =>
    {
        setIsDialogOpen(true);
    }

    const doSave = () =>
    {
        let command = new RoleCreateCommand();
        command.role_name = watch("role_name");

        userService.createRole(command, keycloak?.token || "").then( (response) => {
            setIsDialogOpen(false);
            
            if(response.success && response.data != undefined)
            {
                setRoles(prev => [...prev, response.data!]);
            }
        });
        
    }

    const FormChange = () => {
        const isValid = Boolean(watch("role_name"));

        canSave(isValid);
    };

    const IsDirty = (formName: any) => {
        if(watch(formName) == undefined || watch(formName) == null || watch(formName) == '')
        {
            return true;
        }

        return false;
    }

    const handleDeleteClick = (id: any) => {
        //console.log("Deleted id: ", id);

        setRoles(prev => prev.filter(line => line.role_id !== id));
    }

    const [colDefs, setColDefs] = useState<ColDef<RoleDto>[]>([]);
    
    const defaultColDef: ColDef = {
        flex: 1,
        filter: false,
        sortable: true
    };
    
    return (
        <div style={{ width: "100%", height: "500px" }}>
            <div style={{ width: "100%" }}>
                <Button colorPalette="blue" onClick={() => doAddRole()} >Add Role</Button>
            </div>
            <AgGridReact
                loading={loading}
                rowData={roles}
                columnDefs={colDefs}
                defaultColDef={defaultColDef}
            />

            <Dialog.Root open={isDialogOpen} onOpenChange={(details) => setIsDialogOpen(details.open)} size="sm">
                <Portal>
                    <Dialog.Backdrop />
                    <Dialog.Positioner>
                    <Dialog.Content>
                        <Dialog.Header>
                        <Dialog.Title>Add Role</Dialog.Title>
                        </Dialog.Header>
                        <Dialog.Body>
                            <form onChange={FormChange}>
                                 <Field.Root invalid={IsDirty('role_name')} required={true}>
                                    <Field.Label><Field.RequiredIndicator /> Role Name</Field.Label>
                                    <Input 
                                        {...register('role_name', { required: 'Role name is required' })}
                                    />
                                    <Field.ErrorText>This field is required</Field.ErrorText>
                                </Field.Root>
                            </form>
                        </Dialog.Body>
                        <Dialog.Footer>
                        <Dialog.ActionTrigger asChild>
                            <Button variant="outline">Cancel</Button>
                        </Dialog.ActionTrigger>
                        <Button colorPalette="blue" onClick={() => doSave()} disabled={!saveable}><Spinner hidden={!isWorking} /> Save</Button>
                        </Dialog.Footer>
                        <Dialog.CloseTrigger asChild>
                            <CloseButton size="sm" />
                        </Dialog.CloseTrigger>
                    </Dialog.Content>
                    </Dialog.Positioner>
                </Portal>
            </Dialog.Root>

            
        </div>
    )
}

export default AdminRolesPage;