"use client"

import { PasswordInput } from '@/components/ui/password-input';
import '../../../styles/page.component.css'
import '../../../styles/tree-view.css'

import SessionStorage from "@/components/session-storage";
import { userService } from "@/services/user-service";
import { useKeycloak } from '@react-keycloak/web';
import { Alert, Button, Checkbox, CloseButton, Combobox, createTreeCollection, Dialog, Field, Grid, GridItem, Input, Portal, Spinner, Tabs, TreeView, useFilter, useListCollection } from "@chakra-ui/react";
import { useEffect, useState, useRef } from "react";
import { FaUser } from "react-icons/fa";
import { AssignUserRoleCommand, RoleDto, UserAdminListDto, UserCreateCommand, UserEditCommand, UserRoleDto } from '@/models/user-models';
import { useForm } from 'react-hook-form';
import { MdAddCircle } from 'react-icons/md';
import { AllCommunityModule, ColDef, ModuleRegistry } from 'ag-grid-community';
import { AgGridReact } from 'ag-grid-react';
import { UserRoleRenderer } from '@/components/ag-grid/user-role-renderer';


ModuleRegistry.registerModules([AllCommunityModule]);

function AdminUserPage() {
    const { keycloak } = useKeycloak();
    const { contains } = useFilter({ sensitivity: "base" })

    interface Node {
        id: string;
        name: string;
        dto?: UserAdminListDto;
        children?: Node[];
    }

    const userId = SessionStorage.getUserId();
    const sessionId = SessionStorage.getSession();

    const [currentTab, setCurrentTab] = useState("tab-0");
    const [userData, setUserData] = useState<Node>();

    const [loading, setLoading] = useState(true);
    const [userRoles, setUserRoles] = useState<UserRoleDto[] | undefined>([]);
    const [userSelected, setUserSelected] = useState<UserAdminListDto>();

    const [roles, setRoles] = useState<RoleDto[]>([]);
    const [selectedRole, setSelectedRole] = useState<any[]>([]);
    const [selectedRoleValue, setSelectedRoleValue] = useState<string>();
    const hasInitialized = useRef(false);

    const [saveable, canSave] = useState(false);
    const [saveableAssociation, canSaveAssociation] = useState(false);
    
    const [successSaved, setSuccessSaved] = useState(false);
    const [failedSaved, setFailedSaved] = useState(false);
    const [isWorking, setIsWorking] = useState(false);
    const [isNewUserValid, setIsNewUserValid] = useState(false);

    const [isDialogNewUserOpen, setIsDialogNewUserOpen] = useState(false);
    const [isDialogAssociationOpen, setIsDialogAssociationOpen] = useState(false);

    const { collection: roleCollection, filter, set } = useListCollection({
        initialItems: roles,
        filter: contains,
        itemToString: (item) => item.role_id ? item.role_id.toString() : "-- ERROR --",
        itemToValue: (item) => item.name ? item.name : "-- ERROR --",
        isItemDisabled: (item) => {
            
            let user_role = userRoles?.filter(m => m.role_id == item.role_id);

            if(user_role == undefined || user_role == null || user_role.length == 0)
            {
                return false;
            }

            return true;
        }
    })

    const {
        register,
        setValue,
        watch,
    } = useForm<UserEditCommand>({
        mode: 'onChange',
    });

    const {
        register: registerNewUser,
        setValue: setValueNewUser,
        watch: watchValueNewUser,
    } = useForm<UserCreateCommand>({
        mode: 'onChange',
    });
    
    const collection = createTreeCollection<Node>({
        nodeToValue: (node) => node.id,
        nodeToString: (node) => node.name,
    
        rootNode: userData? userData : {
          id: "ROOT",
          name: "",
          children: []
        }
    })

    const fetchData = (id?: number) => 
    {
        userService.getUsers(keycloak?.token || "").then( (response) => {
            //console.log(response);

            if(response.success && response.data)
            {
                let root_node = {
                    id: "ROOT",
                    name: "",
                    children: new Array<Node>()
                };
    
                for(let i=0; i<response.data.length; i++)
                {
                    let inner_node = {
                        id: response.data[i].id ? response.data[i].id : "",
                        name: response.data[i].first_name ? response.data[i].first_name + " " + response.data[i].last_name : "",
                        dto: response.data[i]
                    } as Node;
    
                    root_node.children.push(inner_node);

                    if(id && response.data[i] && response.data[i].id && id == response.data[i].id)
                    {
                        setUserRoles(response.data[i].user_roles);
                    }
                }
    
                setUserData(root_node);
            }
        });
    }
    
    useEffect(() => {
        if (hasInitialized.current) return;
        hasInitialized.current = true;

        fetchData();

        userService.getRoles(keycloak?.token || "").then( (response) => {
            //console.log(response)
            if(response.success && response.data)
            {
                setRoles(response.data);
                set(response.data);
            }
        });

    }, []);

    const treeItemClick = (treeItem: any) => {
        let result = userData?.children?.filter(m => m.id == treeItem.focusedValue);
        //console.log(result)

        if(result && result.length > 0 && result[0].dto)
        {
            setUserSelected(result[0].dto);
            setUserRoles(result[0].dto.user_roles);

            setValue("first_name", result[0].dto.first_name);
            setValue("last_name", result[0].dto.last_name);
            setValue("username", result[0].dto.username);
            setValue("email", result[0].dto.email);
            setValue("department", result[0].dto.department);
            setValue("employee_number", result[0].dto.employee_number);
            setValue("is_admin", result[0].dto.is_admin);
            setValue("is_management", result[0].dto.is_management);
            setValue("is_external_user", result[0].dto.is_external_user);
            setValue("is_guest", result[0].dto.is_guest);
            setValue("is_deleted", result[0].dto.is_deleted);

            DoValidityCheck();
        }
    }

    const handleSaveClick = async () => {

        let command = new UserEditCommand();
        command.id = Number(userSelected?.id);
        command.first_name = watch("first_name");
        command.last_name = watch("last_name");
        command.email = watch("email");
        command.department = watch("department");
        command.is_admin = Boolean(watch('is_admin'));
        command.is_management = Boolean(watch('is_management'));
        command.is_external_user = Boolean(watch('is_external_user'));
        command.is_guest = Boolean(watch('is_guest'));
        command.is_deleted = Boolean(watch('is_deleted'));


        if(watch("password") && watch("password") != "")
        {
            if(watch("password") == watch("confirm_password"))
            {
                command.password = watch("password");
            }
        }

        //console.log(command);
        //return;
        setIsWorking(true);

        await userService.update(command, keycloak?.token || "").then( (response) => 
        {
            setIsWorking(false);

            if(response.success && response.data)
            {
                setSuccessSaved(true);

                let inner_node = {
                    id: response.data.id ? response.data.id : "",
                    name: response.data.first_name ? response.data.first_name + " " + response.data.last_name : "",
                    dto: response.data
                } as Node;
                
                if(userData != null)
                {
                    let root_node = userData;

                    if(root_node.children)
                    {
                        for(let i=0; i<root_node.children?.length; i++)
                        {
                            // Todo: type difference
                            if(root_node.children[i].id == response.data.id.toString())
                            {
                                root_node.children[i] = inner_node;
                            }
                        }

                        setUserData(root_node);
                    }
                }
            }
            else
            {
                setFailedSaved(true);
            }

        });
    }


    const NewUserFormChange = () =>
    {
        const isValid = Boolean(watchValueNewUser('first_name') 
                                && watchValueNewUser('last_name') 
                                && watchValueNewUser('username')
                                && watchValueNewUser('password')
                                && watchValueNewUser('confirm_password'));

        const password_match = Boolean(watchValueNewUser('password') == watchValueNewUser('confirm_password'));

        //console.log("isValid:", isValid);
        //console.log("password_match:", password_match);

        setIsNewUserValid(isValid && password_match);
    }

    const openNewUserDialog = () => {
        setIsDialogNewUserOpen(true);
    }

    const doNewUserSave = async () => 
    {
        let command = new UserCreateCommand();
        command.first_name = watchValueNewUser("first_name");
        command.last_name = watchValueNewUser("last_name");
        command.username = watchValueNewUser("username");
        command.password = watchValueNewUser("password");

        setIsWorking(true);

        await userService.create(command, keycloak?.token || "").then( (response) => {
            //console.log(response);

            if(response.success && response.data != undefined)
            {
                setIsWorking(false);
                setIsDialogNewUserOpen(false);

                let inner_node = {
                    id: response.data.id ? response.data.id : "",
                    name: response.data.first_name ? response.data.first_name + " " + response.data.last_name : "",
                    dto: response.data
                } as Node;
                
                if(userData != null)
                {
                    let root_node = userData;

                    root_node.children?.push(inner_node);

                    setUserData(root_node);
                }
                
            }
        });

        setIsDialogNewUserOpen(false);
    }

    const DoValidityCheck = () =>
    {
        const isSelected = Boolean(userSelected);
        const hasRequiredFields = Boolean(watch("first_name") && watch("last_name") && watch("username"));

        const valid = isSelected && hasRequiredFields;

        canSave(valid);
    }
    
    const IsDirty = (formName: any) => {
        if(watchValueNewUser(formName) == undefined || watchValueNewUser(formName) == null || watchValueNewUser(formName) == '')
        {
            return true;
        }

        return false;
    }

    const handleAssociateRoleClick = () => {
        setIsDialogAssociationOpen(true);
    }

    const handleAssoicationClick = () => {

        if(selectedRole && selectedRole.length > 0)
        {
            let command = new AssignUserRoleCommand();
            command.user_id = Number(userSelected?.id);
            command.role_id = selectedRole[0].role_id;
            
            userService.assignUserRole(command, keycloak?.token || "").then( (response) => {
                //console.log(response)
                if(response && response.success)
                {
                    setIsDialogAssociationOpen(false);
                    fetchData(Number(userSelected?.id));
                }
                else
                {
                    console.error(response);
                }
            });
        }
        
    }

    const roleInputChange = (inputValue: any) => {
        //console.log(inputValue);

        if(inputValue.value != undefined && inputValue.value.length > 0)
        {
            const selectedItem = roles.find(item => item.name === inputValue.value[0]);

            if (selectedItem) {
                setSelectedRole([selectedItem]);
                setSelectedRoleValue(inputValue.value[0]);
                canSaveAssociation(true);
            } else {
                setSelectedRole([]);
                setSelectedRoleValue('');
                canSaveAssociation(false);
            }
        }
        else
        {
            setSelectedRole([]);
            setSelectedRoleValue('');
            canSaveAssociation(false);
        }
    }

    const inputValChange = (inputValue: any) =>
    {
        filter(inputValue);
    }


    const [colDefs, setColDefs] = useState<ColDef<UserRoleDto>[]>([]);        
    

    useEffect(()=> {
        setColDefs([
            { field: "role_name", headerName: "Role Name" },
            { field: "permissions", headerName: "Permissions", cellRenderer: UserRoleRenderer, cellRendererParams: {user_id: userSelected ? userSelected?.id : null} }
        ]);
    }, [userSelected]);

    const defaultColDef: ColDef = {
        flex: 1,
        filter: false,
        sortable: true
    };

    return (
        <div>
            <form onChange={DoValidityCheck}>
                <Grid
                    templateColumns="repeat(5, 2fr)"
                    gap={6}
                    display="grid"
                    width="100%"
                    p="auto"
                    m="auto"
                    >
                    <GridItem colSpan={1}>
                        <Grid
                            templateColumns="repeat(3, 2fr)"
                            gap={6}
                            display="grid"
                            width="100%"
                            p="auto"
                            m="auto"
                        >
                            <GridItem colSpan={2}>
                                <h3>Users</h3>
                            </GridItem>
                            <GridItem colSpan={1} style={{textAlign: "right"}}>
                                <Button colorPalette="blue" onClick={() => openNewUserDialog()}><MdAddCircle /></Button>
                            </GridItem>
                        </Grid>
                        <TreeView.Root collection={collection} maxW="md" size="md" colorPalette="blue" onSelectionChange={treeItemClick}>
                            <TreeView.Tree>
                            <TreeView.Node
                                indentGuide={<TreeView.BranchIndentGuide />}
                                render={({ node, nodeState }) =>
                                nodeState.isBranch ? (
                                    <TreeView.BranchControl>
                                    <FaUser />
                                    <TreeView.BranchText>{node.name}</TreeView.BranchText>
                                    </TreeView.BranchControl>
                                    ) : (
                                        <TreeView.Item>
                                            <TreeView.ItemText>{node.name}</TreeView.ItemText>
                                        </TreeView.Item>
                                    )
                                }
                            />
                            </TreeView.Tree>
                        </TreeView.Root>
                    </GridItem>
                    <GridItem colSpan={3}>
                        <Tabs.Root defaultValue="tab-0" value={currentTab}>
                            <Tabs.List>
                            <Tabs.Trigger value="tab-0" onClick={() => setCurrentTab('tab-0')}>General</Tabs.Trigger>
                            <Tabs.Trigger value="tab-1" onClick={() => setCurrentTab('tab-1')}>Permissions</Tabs.Trigger>
                            </Tabs.List>
                            <Tabs.Content value="tab-0">
                                <Grid
                                    templateColumns="repeat(5, 2fr)"
                                    gap={6}
                                    display="grid"
                                    width="100%"
                                    p="auto"
                                    m="auto"
                                >
                                    <GridItem colSpan={2}>
                                        <Field.Root required={true}>
                                            <Field.Label><Field.RequiredIndicator /> First Name</Field.Label>
                                            <Input placeholder='First Name' {...register('first_name')}/>
                                            <Field.ErrorText>This field is required</Field.ErrorText>
                                        </Field.Root>
                                    </GridItem>
                                    <GridItem colSpan={2}>
                                        <Field.Root required={true}>
                                            <Field.Label><Field.RequiredIndicator /> Last Name</Field.Label>
                                            <Input placeholder='Last Name' {...register('last_name')}/>
                                            <Field.ErrorText>This field is required</Field.ErrorText>
                                        </Field.Root>
                                    </GridItem>
                                    <GridItem colSpan={1}></GridItem>
                                    <GridItem colSpan={2}>
                                        <Field.Root required={true}>
                                            <Field.Label><Field.RequiredIndicator /> Username</Field.Label>
                                            <Input placeholder='Username' {...register('username')} disabled={true}/>
                                            <Field.ErrorText>This field is required</Field.ErrorText>
                                        </Field.Root>
                                    </GridItem>
                                    <GridItem colSpan={2}>
                                        <Field.Root>
                                            <Field.Label>Email</Field.Label>
                                            <Input placeholder='Email' {...register('email')}/>
                                        </Field.Root>
                                    </GridItem>
                                    <GridItem colSpan={1}></GridItem>
                                    <GridItem colSpan={2}>
                                        <Field.Root>
                                            <Field.Label>Department</Field.Label>
                                            <Input placeholder='Department' {...register('department')}/>
                                        </Field.Root>
                                    </GridItem>
                                    <GridItem colSpan={2}>
                                        <Field.Root>
                                            <Field.Label>Employee Number</Field.Label>
                                            <Input placeholder='Employee Number' {...register('employee_number')}/>
                                        </Field.Root>
                                    </GridItem>
                                    <GridItem colSpan={1}></GridItem>
                                    <GridItem colSpan={2}>
                                        <Field.Root>
                                            <Field.Label>New Password</Field.Label>
                                            <PasswordInput placeholder='New Password' {...register('password')}/>
                                        </Field.Root>
                                    </GridItem>
                                    <GridItem colSpan={2}>
                                        <Field.Root >
                                            <Field.Label>Confirm Password</Field.Label>
                                            <PasswordInput placeholder='Confirm Password' {...register('confirm_password')}/>
                                        </Field.Root>
                                    </GridItem>
                                    <GridItem colSpan={1}></GridItem>
                                    <GridItem colSpan={1}>
                                        <Checkbox.Root checked={watch('is_guest')}>
                                            <Checkbox.HiddenInput {...register('is_guest')} />
                                            <Checkbox.Control /> Is Guest
                                        </Checkbox.Root>
                                    </GridItem>
                                    <GridItem colSpan={1}>
                                        <Checkbox.Root checked={watch('is_external_user')}>
                                            <Checkbox.HiddenInput {...register('is_external_user')} />
                                            <Checkbox.Control /> Is External User
                                        </Checkbox.Root>
                                    </GridItem>
                                    <GridItem colSpan={1}>
                                        <Checkbox.Root checked={watch('is_management')}>
                                            <Checkbox.HiddenInput {...register('is_management')} />
                                            <Checkbox.Control /> Is Management
                                        </Checkbox.Root>
                                    </GridItem>
                                    <GridItem colSpan={1}>
                                        <Checkbox.Root checked={watch('is_admin')}>
                                            <Checkbox.HiddenInput {...register('is_admin')} />
                                            <Checkbox.Control /> Is Administrator
                                        </Checkbox.Root>
                                    </GridItem>
                                    <GridItem colSpan={1}>
                                        <Checkbox.Root checked={watch('is_deleted')}>
                                            <Checkbox.HiddenInput {...register('is_deleted')} />
                                            <Checkbox.Control /> Is Disabled
                                        </Checkbox.Root>
                                    </GridItem>
                                </Grid>

                            </Tabs.Content>
                            <Tabs.Content value="tab-1">
                                <div style={{ width: "100%", height: "500px" }}>
                                    <Button type="button" colorPalette="blue" onClick={handleAssociateRoleClick}>Add Role</Button>
                                    <AgGridReact
                                        rowData={userRoles}
                                        columnDefs={colDefs}
                                        defaultColDef={defaultColDef}
                                    />
                                </div>
                            </Tabs.Content>
                        </Tabs.Root>
                    </GridItem>
                    <GridItem colSpan={1}></GridItem>
                    <GridItem colSpan={5} hidden={!successSaved}>
                        <Alert.Root status="success">
                            <Alert.Indicator />
                            <Alert.Content>
                                <Alert.Title>Record saved!</Alert.Title>
                            </Alert.Content>
                        </Alert.Root>
                    </GridItem>
                    <GridItem colSpan={5} hidden={!failedSaved}>
                        <Alert.Root status="error">
                            <Alert.Indicator />
                            <Alert.Content>
                                <Alert.Title>Record could not be saved!</Alert.Title>
                            </Alert.Content>
                        </Alert.Root>
                    </GridItem>
                    <GridItem colSpan={5}>
                        <hr style={{ width: "100%", marginBottom: "25px", marginTop: "35px"}}/>
                        <div style={{width: "100%", textAlign: "center"}}>
                            <Button type="button" colorPalette="blue" disabled={!saveable} onClick={handleSaveClick}>Save User</Button>
                        </div>
                    </GridItem>
                </Grid>
            </form>

            <Dialog.Root open={isDialogNewUserOpen} onOpenChange={(details) => setIsDialogNewUserOpen(details.open)} size="sm">
                <Portal>
                    <Dialog.Backdrop />
                    <Dialog.Positioner>
                    <Dialog.Content>
                        <Dialog.Header>
                        <Dialog.Title>New User</Dialog.Title>
                        </Dialog.Header>
                        <Dialog.Body>
                            <form onChange={NewUserFormChange}>
                                <Grid
                                    templateColumns="repeat(1, 2fr)"
                                    gap={6}
                                    display="grid"
                                    width="100%"
                                    p="auto"
                                    m="auto"
                                >
                                    <Field.Root invalid={IsDirty('first_name')} required={true}>
                                        <Field.Label><Field.RequiredIndicator /> First Name</Field.Label>
                                        <Input 
                                            {...registerNewUser('first_name', { required: 'First name is required' })}
                                        />
                                        <Field.ErrorText>This field is required</Field.ErrorText>
                                    </Field.Root>

                                    <Field.Root invalid={IsDirty('last_name')} required={true}>
                                        <Field.Label><Field.RequiredIndicator /> Last Name</Field.Label>
                                        <Input 
                                            {...registerNewUser('last_name', { required: 'Last name is required' })}
                                        />
                                        <Field.ErrorText>This field is required</Field.ErrorText>
                                    </Field.Root>

                                    <Field.Root invalid={IsDirty('username')} required={true}>
                                        <Field.Label><Field.RequiredIndicator /> Username</Field.Label>
                                        <Input 
                                            {...registerNewUser('username', { required: 'Username is required' })}
                                        />
                                        <Field.ErrorText>This field is required</Field.ErrorText>
                                    </Field.Root>
                                    <hr/>
                                    <Field.Root invalid={IsDirty('password')} required={true}>
                                        <Field.Label><Field.RequiredIndicator /> Password</Field.Label>
                                        <PasswordInput placeholder='New Password' {...registerNewUser('password')}/>
                                    </Field.Root>
                                    <Field.Root invalid={IsDirty('confirm_password')} required={true}>
                                        <Field.Label><Field.RequiredIndicator /> Confirm Password</Field.Label>
                                        <PasswordInput placeholder='Confirm Password' {...registerNewUser('confirm_password')}/>
                                    </Field.Root>
                                </Grid>
                                
                               
                            </form>
                        </Dialog.Body>
                        <Dialog.Footer>
                        <Dialog.ActionTrigger asChild>
                            <Button variant="outline">Cancel</Button>
                        </Dialog.ActionTrigger>
                        <Button colorPalette="blue" onClick={() => doNewUserSave()} disabled={!isNewUserValid}><Spinner hidden={!isWorking} /> Save</Button>
                        </Dialog.Footer>
                        <Dialog.CloseTrigger asChild>
                            <CloseButton size="sm" />
                        </Dialog.CloseTrigger>
                    </Dialog.Content>
                    </Dialog.Positioner>
                </Portal>
            </Dialog.Root>


            <Dialog.Root open={isDialogAssociationOpen} onOpenChange={(details) => setIsDialogAssociationOpen(details.open)} size="sm">
                <Portal>
                    <Dialog.Backdrop />
                    <Dialog.Positioner>
                    <Dialog.Content>
                        <Dialog.Header>
                            <Dialog.Title>Assign Role</Dialog.Title>
                        </Dialog.Header>
                        <Dialog.Body>
                            <Combobox.Root
                                multiple={false}
                                collection={roleCollection}
                                inputValue={selectedRoleValue}
                                onValueChange={(e) => roleInputChange(e)}
                                onInputValueChange={(e) => inputValChange(e.inputValue)}
                                value={selectedRole}
                                width="100%"
                            >
                                <Combobox.Control>
                                <Combobox.Input placeholder="Type to search" />
                                <Combobox.IndicatorGroup>
                                    <Combobox.ClearTrigger />
                                    <Combobox.Trigger />
                                </Combobox.IndicatorGroup>
                                </Combobox.Control>
                                <Portal>
                                    <Combobox.Positioner>
                                        <Combobox.Content>
                                        <Combobox.Empty>No items found</Combobox.Empty>
                                        {roleCollection.items.map((item) => (
                                            <Combobox.Item item={item} key={item.role_id}>
                                            {item.name}
                                            <Combobox.ItemIndicator />
                                            </Combobox.Item>
                                        ))}
                                        </Combobox.Content>
                                    </Combobox.Positioner>
                                </Portal>
                            </Combobox.Root>
                        </Dialog.Body>
                        <Dialog.Footer>
                        <Dialog.ActionTrigger asChild>
                            <Button variant="outline">Close</Button>
                        </Dialog.ActionTrigger>
                        <Button type="button" colorPalette="blue" disabled={!saveableAssociation} onClick={handleAssoicationClick}>Save Role Association</Button>
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

export default AdminUserPage;