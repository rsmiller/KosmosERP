"use client"

import '../../../styles/date-picker.css';
import '../../../styles/page.component.css';
import 'ag-grid-community/styles/ag-theme-quartz.css';

import { useForm } from 'react-hook-form'
import {
  Grid,
  Stack,
  Input,
  GridItem,
  Field,
  Button,
  Dialog,
  Portal,
  CloseButton,
} from '@chakra-ui/react';

import { useEffect, useState, useRef, useCallback } from "react";
import { useRouter } from 'next/navigation';
import DatePicker from 'react-datepicker';

import { OpportunityCreateCommand, OpportunityDto, OpportunityLineCreateCommand } from '@/models/opportunity-models';
import CustomerCombobox, { CustomerComboboxRef } from '@/components/customer-combobox';
import ContactCombobox, { ContactComboboxRef } from '@/components/contact-combobox';
import PageActionsComponent from '@/components/page-actions';

import OpportunityWinCombobox, { OpportunityWinComboboxRef } from '@/components/opportunity-win-combobox';
import { opportunityService } from '@/services/opportunity-service';
import OpportunityStageCombobox, { OpportunityStageComboboxRef } from '@/components/opportunity-stage-combobox';
import SessionStorage from '@/components/session-storage';

import { format, parse } from 'date-fns';
import { AllCommunityModule, ColDef, ModuleRegistry } from 'ag-grid-community';
import { CurrencyFormatter } from '@/components/ag-grid/currency-formatter';
import AddOpportunityLineDialog, { AddOpportunityLineDialogRef } from '@/components/dialogs/add-opportunity-line';
import { AgGridReact } from 'ag-grid-react';
import { useKeycloak } from '@react-keycloak/web';
import { permissionsService, ERPModules, ERPModulePermission } from '@/services/permissions-service';

ModuleRegistry.registerModules([AllCommunityModule]);

function NewOpportunityPage() {
    const { keycloak } = useKeycloak();
    const userId = SessionStorage.getUserId();
    const router = useRouter();
    const [hasAccess, setHasAccess] = useState(true);
    const [hasWritePermission, setHasWritePermission] = useState(false);

    const [loading, setLoading] = useState(false);
    const [error, setError] = useState<string | null>(null);
    const [saveable, canSave] = useState(false);
    const [successSaved, setSuccessSaved] = useState(false);
    const [failedSaved, setFailedSaved] = useState(false);

    const stageComboboxRef = useRef<OpportunityStageComboboxRef>(null);
    const customerComboboxRef = useRef<CustomerComboboxRef>(null);
    const contactComboboxRef = useRef<ContactComboboxRef>(null);
    const winChanceComboboxRef = useRef<OpportunityWinComboboxRef>(null);
    const addOpoortunityLineDialogRef = useRef<AddOpportunityLineDialogRef>(null);

    const [selectedLineId, setSelectedLineId] = useState<string>("");

    const [openLineDialog, setOpenLineDialog] = useState(false);
    const [isDeleteDialogOpen, setIsDeleteDialogOpen] = useState(false);

    const {
        register,
        handleSubmit,
        formState: { errors, isValid },
        setValue,
        watch,
        control,
    } = useForm<OpportunityCreateCommand>();

    useEffect(() => {
        if(keycloak.authenticated == false) return;

        const realmRoles = keycloak?.tokenParsed?.realm_access?.roles || [];
        const hasPermission = permissionsService.HasPermission(
          ERPModules.OpportunityModule,
          ERPModulePermission.Write,
          realmRoles
        );
        if (!hasPermission) {
          setHasAccess(false);
          router.push('/erp');
          return;
        }
        setHasWritePermission(true);
    }, [keycloak.authenticated, router]);

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

            let command = new OpportunityCreateCommand();
            command.stage = watch('stage');
            command.customer_id = Number(watch('customer_id'));
            command.contact_id = Number(watch('contact_id'));
            command.opportunity_name = watch('opportunity_name');
            command.expected_close = watch('expected_close') == undefined ? format(new Date(), 'yyyy-MM-dd') : watch('expected_close');
            command.win_chance = Number(watch('win_chance'));
            command.amount = 0;
            command.owner_id = Number(userId);

            command.opportunity_lines = new Array<OpportunityLineCreateCommand>();

            for(let i=0; i < rowData.length; i++)
            {
                let line_command = rowData[i];

                command.opportunity_lines.push(line_command);
            }



            //console.log(command);
            //return;
            try
            {
                await opportunityService.create(command, keycloak.token || "").then((response) =>
                {
                    //console.log(response)
                    if(response.success)
                    {
                        setSuccessSaved(true);
                        setFailedSaved(false);
                        // Route to opportunities page on successful save
                        router.push("/erp/opportunities");
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
                console.error(e)
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
        const value = watch(formName);
        if(value == undefined || value == null || value === '')
        {
            return true;
        }

        return false;
    }

    const handleDeleteLineClick = (value: any) =>
    {
        //console.log(value);
        setSelectedLineId(value as string);
        setIsDeleteDialogOpen(true);
    }
    
    const doLineDelete = () => {

        setRowData(prev => prev.filter(line => line.guid !== selectedLineId));
        setIsDeleteDialogOpen(false);
    }
    

    const handleAddSelect = async (command: OpportunityLineCreateCommand) =>
    {
        //console.log(command);

        if(command != null)
        {
            command.guid = Math.floor(Math.random() * 1000).toString();

            setRowData(prev => [...prev, command]);
        }  

        setOpenLineDialog(false);
    }

    const getRowId = useCallback((params: any) => String(params.data.guid), []);

    const [rowData, setRowData] = useState<OpportunityLineCreateCommand[]>([]);
        
    const [colDefs, setColDefs] = useState<ColDef<OpportunityLineCreateCommand>[]>([
        { field: "product_name", headerName: "Product Name"},
        { field: "description", headerName: "Description", editable: true},
        { field: "quantity", headerName: "Quantity",
            editable: true,
            cellEditor: 'agNumberCellEditor',
            cellEditorParams: {
                precision: 2,
                step: 1
            },
            valueParser: (params: any) => {
                const value = params.newValue;
                return value === '' || value === null || value === undefined ? 0 : Number(value);
            },
            },
        { field: "unit_price", headerName: "Unit Price", 
            editable: true,
            cellEditor: 'agNumberCellEditor',
            cellEditorParams: {
                precision: 2,
                step: 0.01
            },
            valueParser: (params: any) => {
                const value = params.newValue;
                return value === '' || value === null || value === undefined ? 0 : Number(value);
            },
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
            field: "guid",
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
                        <h1>New Opportunity</h1>
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
                    <GridItem colSpan={5}></GridItem>
                    <GridItem colSpan={5}>
                        <div style={{ width: "100%", height: "500px" }}>
                            <Button onClick={() => setOpenLineDialog(!openLineDialog)} colorPalette="blue">Add New Line</Button>
                            <AgGridReact
                                rowData={rowData}
                                columnDefs={colDefs}
                                defaultColDef={defaultColDef}
                                stopEditingWhenCellsLoseFocus={true}
                                getRowId={getRowId}
                                />
                        </div>
                    </GridItem>
                    <GridItem colSpan={5}></GridItem>
                    <GridItem colSpan={5}>
                        <PageActionsComponent canSave={saveable && hasWritePermission} 
                                                onSave={handleSaveClick} 
                                                onDelete={undefined} 
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
                        <Button colorPalette="red" onClick={() => doLineDelete()} >Delete</Button>
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

export default NewOpportunityPage;