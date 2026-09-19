"use client"

import '../../../../styles/date-picker.css';
import '../../../../styles/page.component.css';
import 'ag-grid-community/styles/ag-theme-quartz.css';

import { useForm } from 'react-hook-form'
import {
  Grid,
  Stack,
  Input,
  GridItem,
  Field,
  Tabs,
  Button,
  Dialog,
  Spinner,
  Portal,
  CloseButton,
} from '@chakra-ui/react';

import { useEffect, useState, useRef } from "react";
import { useParams, useRouter } from 'next/navigation';
import DatePicker from 'react-datepicker';

import { OpportunityEditCommand, OpportunityDto, OpportunityDeleteCommand, OpportunityLineDto, OpportunityLineCreateCommand, OpportunityLineDeleteCommand, OpportunityLineEditCommand } from '@/models/opportunity-models';
import CustomerCombobox, { CustomerComboboxRef } from '@/components/customer-combobox';
import ContactCombobox, { ContactComboboxRef } from '@/components/contact-combobox';
import PageActionsComponent from '@/components/page-actions';

import OpportunityWinCombobox, { OpportunityWinComboboxRef } from '@/components/opportunity-win-combobox';
import { opportunityService } from '@/services/opportunity-service';
import OpportunityStageCombobox, { OpportunityStageComboboxRef } from '@/components/opportunity-stage-combobox';
import SessionStorage from '@/components/session-storage';

import { format, parse } from 'date-fns';
import ActivitiesListComponent from '@/components/lists/activities-list-component';
import { AgGridReact } from 'ag-grid-react';
import { AllCommunityModule, ColDef, ModuleRegistry } from 'ag-grid-community';
import { CurrencyFormatter } from '@/components/ag-grid/currency-formatter';
import AddOpportunityLineDialog, { AddOpportunityLineDialogRef } from '@/components/dialogs/add-opportunity-line';
import { useAuth } from '@/lib/auth/auth-context';
import { permissionsService, ERPModules, ERPModulePermission } from '@/services/permissions-service';

ModuleRegistry.registerModules([AllCommunityModule]);

function EditOpportunityPage() {
    const auth = useAuth();
    const params = useParams();
    const router = useRouter();
    const [hasAccess, setHasAccess] = useState(true);
    const [hasEditPermission, setHasEditPermission] = useState(false);
    const [hasDeletePermission, setHasDeletePermission] = useState(false);

    const userId = SessionStorage.getUserId();
    const sessionId = SessionStorage.getSession();

    const [opportunity, setOpportunity] = useState<OpportunityDto | null>(null);
    const [loading, setLoading] = useState(true);
    const [isWorking, setIsWorking] = useState(false);
    
    const [error, setError] = useState<string | null>(null);
    const [saveable, canSave] = useState(false);
    const [successSaved, setSuccessSaved] = useState(false);
    const [failedSaved, setFailedSaved] = useState(false);

    const stageComboboxRef = useRef<OpportunityStageComboboxRef>(null);
    const customerComboboxRef = useRef<CustomerComboboxRef>(null);
    const contactComboboxRef = useRef<ContactComboboxRef>(null);
    const winChanceComboboxRef = useRef<OpportunityWinComboboxRef>(null);
    const addOpoortunityLineDialogRef = useRef<AddOpportunityLineDialogRef>(null);
    const hasInitialized = useRef(false);

    const [selectedLineId, setSelectedLineId] = useState<number>(0);

    const [openLineDialog, setOpenLineDialog] = useState(false);
    const [isDeleteDialogOpen, setIsDeleteDialogOpen] = useState(false);
    

    const {
        register,
        handleSubmit,
        formState: { errors, isValid },
        setValue,
        watch,
        control,
    } = useForm<OpportunityEditCommand>();

    const loadOpportunity = async () => {
        try {
            setLoading(true);
            const opportunityId = String(params.id);
            
            if (opportunityId == "") {
                setError('Invalid opportunity ID');
                return;
            }

            const response = await opportunityService.getByGuid(opportunityId, auth.token || "");
            //console.log(response)
            if (response.success && response.data) {
                setOpportunity(response.data);
                
                // Set form values with loaded data
                setValue('id', response.data.id);
                setValue('opportunity_name', response.data.opportunity_name || '');
                setValue('customer_id', response.data.customer_id);
                setValue('contact_id', response.data.contact_id);
                setValue('amount', response.data.amount);
                setValue('stage', response.data.stage || '');
                setValue('win_chance', response.data.win_chance);
                setValue('expected_close', response.data.expected_close?.toString());
                setValue('owner_id', response.data.owner_id);


                if(response.data.opportunity_lines)
                {
                    setRowData(response.data.opportunity_lines);
                }

            } else {
                setError('Failed to load opportunity');
            }
        } catch (err) {
            console.error('Error loading opportunity:', err);
            setError('Error loading opportunity');
        } finally {
            setLoading(false);
        }
    };
    
    useEffect(() => {
        if(auth.authenticated == false) return;

        const realmRoles = auth.roles || [];
        const hasPermission = permissionsService.HasPermission(
          ERPModules.OpportunityModule,
          ERPModulePermission.Read,
          realmRoles
        );
        if (!hasPermission) {
          setHasAccess(false);
          router.push('/erp');
          return;
        }

        // Check Edit permission
        const canEdit = permissionsService.HasPermission(
          ERPModules.OpportunityModule,
          ERPModulePermission.Edit,
          realmRoles
        );
        setHasEditPermission(canEdit);

        // Check Delete permission
        const canDelete = permissionsService.HasPermission(
          ERPModules.OpportunityModule,
          ERPModulePermission.Delete,
          realmRoles
        );
        setHasDeletePermission(canDelete);

        if (hasInitialized.current) return;

        hasInitialized.current = true;


        loadOpportunity();
    }, [params.id, setValue, auth.authenticated]);


    const handleCustomerSelect = (value: any) => {
        setValue('customer_id', value?.id);
        CheckFormValidity();
    }

    const handleContactSelect = (value: any) => {
        setValue('contact_id', value?.id);
        CheckFormValidity();
    }

    const handleStageSelect = (value: any) => {
        setValue('stage', value?.key);
        CheckFormValidity();
    }

    const handleWinChanceSelect = (value: any) => {
        setValue('win_chance', value?.value);
        CheckFormValidity();
    }

    const handleExpectedCloseChange = (date: Date | null) => {
        if( date != null)
        {
            setValue('expected_close', format(date, 'yyyy-MM-dd'));
        }
    }

    const handleDeleteClick = async () => {
        //console.log('Delete clicked:');
        let command = new OpportunityDeleteCommand();
        command.id = opportunity?.id;

        try
        {
            await opportunityService.delete(command, auth.token || "").then((response) => {
                //console.log(response)
                if(response.success)
                {
                    router.push("/erp/opportunities/");
                }
                else
                {
                    setSuccessSaved(false);
                    setFailedSaved(true);
                }
            });
        }
        catch(e)
        {
            setSuccessSaved(false);
            setFailedSaved(true);
        }
        
    };

    const handleSaveClick = async () => {
        //console.log('Save clicked:');
        
        // Check if all comboboxes are valid
        if (stageComboboxRef.current && customerComboboxRef.current && contactComboboxRef.current && winChanceComboboxRef.current) {
            const isStageValid = stageComboboxRef.current.isValid();
            const isCustomerValid = customerComboboxRef.current.isValid();
            const isContactValid = contactComboboxRef.current.isValid();
            const isWinChanceValid = winChanceComboboxRef.current.isValid();
            
            
            if (!isStageValid || !isCustomerValid || !isContactValid || !isWinChanceValid) {
                console.log('Form is not valid, cannot save');
                return;
            }

            setSuccessSaved(false);

            let command = new OpportunityEditCommand();
            command.id = opportunity?.id;
            command.stage = watch('stage');
            command.customer_id = watch('customer_id');
            command.contact_id = watch('contact_id');
            command.opportunity_name = watch('opportunity_name');
            command.expected_close = watch('expected_close');
            command.win_chance = watch('win_chance');

            const oppLines: OpportunityLineEditCommand[] = rowData.map((line, index) => ({
                  id: line.id,
                  opportunity_id: opportunity?.id,
                  product_id: line.product_id,
                  line_number: line.line_number || index + 1,
                  description: line.description,
                  quantity: line.quantity,
                  unit_price: line.unit_price,
                  calling_user_id: Number(userId),
                  token: sessionId?.toString(),
                }));
            
                command.opportunity_lines = oppLines;

            console.log(command);
            //return;

            try
            {
                await opportunityService.update(command, auth.token || "").then((response) =>
                {
                    //console.log(response)
                    if(response.success)
                    {
                        setSuccessSaved(true);
                        setFailedSaved(false);
                    }
                    else
                    {
                        setSuccessSaved(false);
                        setFailedSaved(true);
                    }
                });
            }
            catch(e)
            {
                setSuccessSaved(false);
                setFailedSaved(true);
            }

            
        } else {
            console.log('Refs not available, cannot validate');
            return;
        }
    };

    const CheckFormValidity = () =>{
        
        if(stageComboboxRef.current && customerComboboxRef.current && contactComboboxRef.current && winChanceComboboxRef.current)
        {
            const stageValid = stageComboboxRef.current?.isValid();
            const customerValid = customerComboboxRef.current?.isValid();
            const contactValid = contactComboboxRef.current?.isValid();
            const winChanceValid = winChanceComboboxRef.current?.isValid();
            
            const allValid = stageValid && customerValid && contactValid && winChanceValid;

            //console.log('stageValid: ', stageValid);
            //console.log('customerValid: ', customerValid);
            //console.log('contactValid: ', contactValid);
            //console.log('winChanceValid: ', winChanceValid);
            //console.log('allValid: ', allValid);

            canSave(!allValid);
        }
        else
        {
            canSave(false);
        }
    };

    const getExpirationDate = () =>
    {
        if(watch('expected_close') != undefined) {
            let expected_close = watch('expected_close') ? String(watch('expected_close', "")) : "";
            if (expected_close) {
                return parse(expected_close, 'yyyy-MM-dd', new Date());
            }
        }

        return new Date();
    }

    const IsDirty = (formName: any) => {
        if(watch(formName) == undefined || watch(formName) == null || watch(formName) == '')
        {
            return true;
        }

        return false;
    }

    const handleDeleteLineClick = (value: any) =>
    {
        //console.log(value);
        setSelectedLineId(value as number);
        setIsDeleteDialogOpen(true);
    }

    const doLineDelete = () => {

        setIsWorking(true);

        let command = new OpportunityLineDeleteCommand();
        command.id = selectedLineId;

        opportunityService.deleteLine(command, auth.token || "").then( (response) => {
            //console.log(response);

            setIsWorking(false);
            setIsDeleteDialogOpen(false);

            if(response.success && response.data)
            {
                let lineId = response.data.id;
                setRowData(prev => prev.filter(line => line.id !== lineId));
            }
            else
            {
                console.error(response);
            }
        });
    }

    const handleAddSelect = async (command: OpportunityLineCreateCommand) =>
    {
        //console.log(command);

        if(command != null)
        {
            //console.log("COMMAND: ", command)
            command.line_number = rowData.length + 1;
            command.opportunity_id = opportunity?.id;
            
            await opportunityService.createLine(command, auth.token || "").then((response) =>
            {
                if(response.success && response.data)
                {
                    var data = response.data;

                    setRowData(prev => [...prev, data]);
                }
                else
                {
                    console.error(response);
                }
            });
        }


        setOpenLineDialog(false);
    }


    const [rowData, setRowData] = useState<OpportunityLineDto[]>([]);
    
    const [colDefs, setColDefs] = useState<ColDef<OpportunityLineDto>[]>([
    { field: "product_name", headerName: "Product Name"},
    { field: "description", headerName: "Description", editable: true},
    { field: "quantity", headerName: "Quantity",
        editable: true,
        cellEditor: 'agNumberCellEditor',
        },
    { field: "unit_price", headerName: "Unit Price", 
        editable: true,
        cellEditor: 'agNumberCellEditor',
        cellRenderer: CurrencyFormatter 
    },
    { headerName: "Total", 
        cellRenderer: (props: any) => 
        {
        const total = props.data.quantity * props.data.unit_price;
        return "$" + total.toFixed(2);
        }
    },
    {
        field: "id",
        headerName: "Actions",
        cellRenderer: (props: any) => {
        return ( 
            <div>
                <Button type="button" colorPalette="red" onClick={() => handleDeleteLineClick(props.value)} >Delete</Button>
            </div>
        );
        }
    }
    ]);

    const defaultColDef: ColDef = {
        flex: 1,
        filter: true,
        sortable: true,
    };
    
    if (!hasAccess) {
        return <div>Redirecting...</div>;
    }

    if (loading) {
        return <div>Loading opportunity...</div>;
    }

    if (error) {
        return <div>Error: {error}</div>;
    }

    if (!opportunity) {
        return <div>Opportunity not found</div>;
    }

    return (
        <div>
            <form onChange={CheckFormValidity}>
                <Grid
                    templateColumns="repeat(5, 2fr)"
                    gap={6}
                    display="grid"
                    width="100%"
                    p="auto"
                    m="auto"
                >
                    <GridItem colSpan={6}>
                        <h1>Edit Opportunity</h1>
                    </GridItem>
                    <GridItem colSpan={1}>
                        <Field.Root invalid={IsDirty("stage")} required={true}>
                            <Field.Label><Field.RequiredIndicator /> Opportunity Stage</Field.Label>
                            <OpportunityStageCombobox 
                                ref={stageComboboxRef}
                                dbKey={watch('stage') ? [watch('stage') as string] : []}
                                onChange={handleStageSelect}
                                control={control}
                                name="stage"
                                error={errors.stage}
                                onValidationChange={CheckFormValidity}
                            />
                        </Field.Root>
                    </GridItem>
                    <GridItem colSpan={5}></GridItem>
                    <GridItem colSpan={2}>
                        <Stack gap="4" align="flex-start" maxW="md">
                            <Field.Root invalid={IsDirty('opportunity_name')} required={true}>
                                <Field.Label><Field.RequiredIndicator /> Opportunity Name</Field.Label>
                                <Input 
                                    {...register('opportunity_name')}
                                />
                                <Field.ErrorText>This field is required</Field.ErrorText>
                            </Field.Root>
                        </Stack>
                    </GridItem>
                    <GridItem colSpan={4}></GridItem>
                    <Stack gap="4" align="flex-start" maxW="md">
                        <Field.Root invalid={IsDirty("customer_id")} required={true}>
                            <Field.Label><Field.RequiredIndicator /> Customer</Field.Label>
                            <CustomerCombobox 
                                ref={customerComboboxRef}
                                dbKey={watch('customer_id')}
                                onChange={handleCustomerSelect}
                                control={control}
                                name="customer_id"
                                error={errors.customer_id}
                                onValidationChange={CheckFormValidity} disabled={false}
                                />
                            <Field.ErrorText>This field is required</Field.ErrorText>
                        </Field.Root>
                    </Stack>
                    
                    <Stack gap="4" align="flex-start" maxW="md">
                        <Field.Root invalid={IsDirty("contact_id")} required={true}>
                            <Field.Label><Field.RequiredIndicator /> Contact</Field.Label>
                            <ContactCombobox 
                                ref={contactComboboxRef}
                                dbKey={watch('contact_id')}
                                customerId={watch('customer_id')}
                                onChange={handleContactSelect}
                                control={control}
                                name="contact_id"
                                error={errors.contact_id}
                                onValidationChange={CheckFormValidity}
                            />
                            <Field.ErrorText>This field is required</Field.ErrorText>
                        </Field.Root>
                    </Stack>
                    <GridItem colSpan={4}></GridItem>

                    <Stack gap="4" align="flex-start" maxW="md">
                        <Field.Root invalid={IsDirty('expected_close')} required={true}>
                            <Field.Label><Field.RequiredIndicator /> Expected Close</Field.Label>
                            <DatePicker 
                                selected={getExpirationDate()}
                                onChange={handleExpectedCloseChange}
                                dateFormat="MM/dd/yyyy"
                                placeholderText="Select date"
                            />
                            <Field.ErrorText>This field is required</Field.ErrorText>
                        </Field.Root>
                    </Stack>
                    <Stack gap="4" align="flex-start" maxW="md">
                        <Field.Root invalid={IsDirty("win_chance")} required={true}>
                            <Field.Label><Field.RequiredIndicator /> Win Percentage</Field.Label>
                            <OpportunityWinCombobox 
                                ref={winChanceComboboxRef}
                                dbKey={watch('win_chance')}
                                onChange={handleWinChanceSelect}
                                control={control}
                                name="win_chance"
                                error={errors.win_chance}
                                onValidationChange={CheckFormValidity}
                            />
                            <Field.ErrorText>This field is required</Field.ErrorText>
                        </Field.Root>
                    </Stack>
                    <GridItem colSpan={5}>
                        <Tabs.Root lazyMount unmountOnExit defaultValue="lines">
                            <Tabs.List>
                                <Tabs.Trigger value="lines">Opportunity Lines</Tabs.Trigger>
                                <Tabs.Trigger value="activities">Activities</Tabs.Trigger>
                            </Tabs.List>
                            <Tabs.Content value="lines">
                                <div style={{ width: "100%", height: "500px" }}>
                                    <Button onClick={() => setOpenLineDialog(!openLineDialog)} colorPalette="blue">Add New Line</Button>
                                    <AgGridReact
                                        rowData={rowData}
                                        columnDefs={colDefs}
                                        defaultColDef={defaultColDef}
                                        />
                                </div>
                            </Tabs.Content>
                            <Tabs.Content value="activities">
                                <ActivitiesListComponent entity_id={opportunity.id} entity_type="opportunity" onChange={() => {}} />
                            </Tabs.Content>
                        </Tabs.Root>
                    </GridItem>
                    <GridItem colSpan={5}>
                        <PageActionsComponent canSave={saveable && hasEditPermission} 
                                                canDelete={hasDeletePermission}
                                                onSave={handleSaveClick} 
                                                onDelete={handleDeleteClick} 
                                                successSaved={successSaved}
                                                failedSaved={failedSaved}/>
                    </GridItem>
                </Grid>
            </form>

            <AddOpportunityLineDialog 
                openDialog={openLineDialog}
                ref={addOpoortunityLineDialogRef}
                control={control}
                name="add_dialog"
                onChange={handleAddSelect}
            />

            <Dialog.Root open={isDeleteDialogOpen} onOpenChange={(details) => setIsDeleteDialogOpen(details.open)} role="alertdialog">
                <Portal>
                    <Dialog.Backdrop />
                    <Dialog.Positioner>
                    <Dialog.Content>
                        <Dialog.Header>
                        <Dialog.Title>Are you sure?</Dialog.Title>
                        </Dialog.Header>
                        <Dialog.Body>
                        <p>
                            Are you sure you want to delete this Line Item?
                        </p>
                        </Dialog.Body>
                        <Dialog.Footer>
                        <Dialog.ActionTrigger asChild>
                            <Button variant="outline">Cancel</Button>
                        </Dialog.ActionTrigger>
                        <Button colorPalette="red" onClick={() => doLineDelete()} ><Spinner hidden={!isWorking} /> Delete</Button>
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

export default EditOpportunityPage;