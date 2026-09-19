"use client"

import '../../../../styles/date-picker.css';
import '../../../../styles/page.component.css';
import 'ag-grid-community/styles/ag-theme-quartz.css';

import {
  Grid,
  Stack,
  Input,
  GridItem,
  Field,
  Tabs,
} from '@chakra-ui/react';

import { useEffect, useState, useRef } from "react";
import { useParams } from 'next/navigation';
import DatePicker from 'react-datepicker';

import { OpportunityDto, OpportunityLineDto } from '@/models/opportunity-models';
import CustomerCombobox, { CustomerComboboxRef } from '@/components/customer-combobox';
import ContactCombobox, { ContactComboboxRef } from '@/components/contact-combobox';
import OpportunityWinCombobox, { OpportunityWinComboboxRef } from '@/components/opportunity-win-combobox';
import { opportunityService } from '@/services/opportunity-service';
import OpportunityStageCombobox, { OpportunityStageComboboxRef } from '@/components/opportunity-stage-combobox';
import { useForm } from 'react-hook-form';
import ActivitiesListComponent from '@/components/lists/activities-list-component';
import { AllCommunityModule, ColDef, ModuleRegistry } from 'ag-grid-community';
import { CurrencyFormatter } from '@/components/ag-grid/currency-formatter';
import { AgGridReact } from 'ag-grid-react';
import { useAuth } from '@/lib/auth/auth-context';
import { permissionsService, ERPModules, ERPModulePermission } from '@/services/permissions-service';
import { useRouter } from 'next/navigation';

ModuleRegistry.registerModules([AllCommunityModule]);

function ViewOpportunityPage() {
    const auth = useAuth();
    const router = useRouter();
    const params = useParams();
    const [hasAccess, setHasAccess] = useState(true);

    const [opportunity, setOpportunity] = useState<OpportunityDto | null>(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    const stageComboboxRef = useRef<OpportunityStageComboboxRef>(null);
    const customerComboboxRef = useRef<CustomerComboboxRef>(null);
    const contactComboboxRef = useRef<ContactComboboxRef>(null);
    const winChanceComboboxRef = useRef<OpportunityWinComboboxRef>(null);
    const hasInitialized = useRef(false);

    const {
        setValue,
        watch,
        control,
    } = useForm<OpportunityDto>();

    const loadOpportunity = async () => {
        try {
            setLoading(true);
            const opportunityId = String(params.id);
            
            if (opportunityId == "") {
                setError('Invalid opportunity ID');
                return;
            }

            const response = await opportunityService.getByGuid(opportunityId, auth.token || "");
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
                setValue('expected_close', response.data.expected_close);
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

        if (hasInitialized.current) return;

        hasInitialized.current = true;
        

        loadOpportunity();
    }, [params.id, setValue, auth.authenticated]);

    const getExpirationDate = () => {
        const expectedClose = watch('expected_close');
        if(expectedClose != undefined && expectedClose != null) {
            return expectedClose;
        }

        return new Date();
    }

    const [rowData, setRowData] = useState<OpportunityLineDto[]>([]);
        
    const [colDefs, setColDefs] = useState<ColDef<OpportunityLineDto>[]>([
    { field: "product_name", headerName: "Product Name"},
    { field: "description", headerName: "Description" },
    { field: "quantity", headerName: "Quantity"},
    { field: "unit_price", headerName: "Unit Price", 
        cellRenderer: CurrencyFormatter 
    },
    { headerName: "Total", 
        cellRenderer: (props: any) => 
        {
        const total = props.data.quantity * props.data.unit_price;
        return "$" + total.toFixed(2);
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

    const stageValue = watch('stage');
    const opportunityNameValue = watch('opportunity_name');
    const customerIdValue = watch('customer_id');
    const contactIdValue = watch('contact_id');
    const winChanceValue = watch('win_chance');

    return (
        <Grid
            templateColumns="repeat(5, 2fr)"
            gap={6}
            display="grid"
            width="100%"
            p="auto"
            m="auto"
        >
            <GridItem colSpan={6}>
                <h1>View Opportunity</h1>
            </GridItem>
            <GridItem colSpan={1}>
                <Field.Root>
                    <Field.Label>Opportunity Stage</Field.Label>
                    <OpportunityStageCombobox 
                        ref={stageComboboxRef}
                        dbKey={stageValue ? [stageValue as string] : []}
                        onChange={() => {}}
                        control={control}
                        name="stage"
                        onValidationChange={() => {}}
                        disabled={true}
                    />
                </Field.Root>
            </GridItem>
            <GridItem colSpan={5}></GridItem>
            <GridItem colSpan={2}>
                <Stack gap="4" align="flex-start" maxW="md">
                    <Field.Root>
                        <Field.Label>Opportunity Name</Field.Label>
                        <Input 
                            value={opportunityNameValue || ''}
                            readOnly
                        />
                    </Field.Root>
                </Stack>
            </GridItem>
            <GridItem colSpan={4}></GridItem>
            <Stack gap="4" align="flex-start" maxW="md">
                <Field.Root>
                    <Field.Label>Customer</Field.Label>
                    <CustomerCombobox 
                        ref={customerComboboxRef}
                        dbKey={customerIdValue}
                        onChange={() => {}}
                        control={control}
                        name="customer_id"
                        disabled={true}
                        onValidationChange={() => {}}
                    />
                </Field.Root>
            </Stack>
            
            <Stack gap="4" align="flex-start" maxW="md">
                <Field.Root>
                    <Field.Label>Contact</Field.Label>
                    <ContactCombobox 
                        ref={contactComboboxRef}
                        dbKey={contactIdValue}
                        customerId={customerIdValue}
                        onChange={() => {}}
                        control={control}
                        name="contact_id"
                        onValidationChange={() => {}}
                        disabled={true}
                    />
                </Field.Root>
            </Stack>
            <GridItem colSpan={4}></GridItem>

            <Stack gap="4" align="flex-start" maxW="md">
                <Field.Root>
                    <Field.Label>Expected Close</Field.Label>
                    <DatePicker 
                        selected={getExpirationDate()}
                        onChange={() => {}}
                        dateFormat="MM/dd/yyyy"
                        placeholderText="Select date"
                        disabled={true}
                    />
                </Field.Root>
            </Stack>
            <Stack gap="4" align="flex-start" maxW="md">
                <Field.Root>
                    <Field.Label>Win Percentage</Field.Label>
                    <OpportunityWinCombobox 
                        ref={winChanceComboboxRef}
                        dbKey={winChanceValue}
                        onChange={() => {}}
                        control={control}
                        name="win_chance"
                        onValidationChange={() => {}}
                        disabled={true}
                    />
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
        </Grid>
    )
}

export default ViewOpportunityPage;
