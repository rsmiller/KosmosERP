import { Button, CloseButton, Dialog, Field, Grid, GridItem, Input, Portal, Stack} from "@chakra-ui/react";
import { useEffect, useState } from "react";
import { useForm } from "react-hook-form";
import { FaSearch } from "react-icons/fa";
import AgGridCustomPagination from "./ag-grid/pagination-control";
import { AgGridReact } from "ag-grid-react";
import { AllCommunityModule, ColDef, ModuleRegistry } from "ag-grid-community";
import { ARInvoiceHeaderFindCommand, ARInvoiceHeaderListDto } from "@/models/ar-models";
import { DateOnlyRender } from "./ag-grid/date-only-renderer";
import { CurrencyFormatter } from "./ag-grid/currency-formatter";
import { arInvoiceService } from "@/services/ar-invoice-service";
import { useKeycloak } from '@react-keycloak/web';

export class ARInvoiceSelectorComponentForm
{
    ar_header_id: any;
    ar_search: any;
    invoice_number: any;

    constructor(init?: Partial<ARInvoiceSelectorComponentForm>) {
        Object.assign(this, init);
    }
}

export class ARInvoiceSelectorComponentParams
{
    customer_id: any;
    ar_invoice_number?: string;
    disabled?: boolean;
    onChange?: (ARInvoiceSelectorForm: any) => void;
}

ModuleRegistry.registerModules([AllCommunityModule]);

function ARInvoiceSelectorComponent({customer_id, ar_invoice_number, disabled, onChange}: ARInvoiceSelectorComponentParams) {
    const { keycloak } = useKeycloak();

    const [isDialogOpen, setDialogOpen] = useState<boolean>(false);
    const [page, setPage] = useState<number>(1);
    const [pageSize, setPageSize] = useState<number>(50);
    const [totalCount, setTotalCount] = useState<number>(1);
    const [loading, setLoading] = useState(true);
    const [rowData, setRowData] = useState<ARInvoiceHeaderListDto[]>([]);

    const {
        register,
        formState: { errors, isValid },
        setValue,
        watch,
    } = useForm<ARInvoiceSelectorComponentForm>();
    

    const fetchData = async (wildcard: string) => {

        if(customer_id == null || customer_id == undefined || customer_id == '')
        {
            return;
        }

        let pageStart = (page * pageSize) - pageSize + 1;
    
        if(pageStart == 0) {
          pageStart = 1;    
        }
    
        setRowData([]);
        
        setLoading(true); // Show loading
    
        let command = new ARInvoiceHeaderFindCommand();
        command.wildcard = wildcard;
        command.customer_id = customer_id;
    
        setTotalCount(1);
    
        console.log(command)
    
        let response = await arInvoiceService.find(command, keycloak?.token || "", pageStart, pageSize);
        //console.log(response)
        setLoading(false);
    
        if(response.success && response.data) {
          setTotalCount(response.totalResultCount);
          setRowData([]);
          
          for(let i=0; i<response.data?.length; i++) {
            let record = response.data[i];
            setRowData(prev => [...prev, { 
                id: record.id, 
                invoice_number: record.invoice_number, 
                invoice_date: record.invoice_date, 
                invoice_total: record.invoice_total,
                payment_terms_name: record.payment_terms_name,
            }]);
          }
        }
    };

    useEffect(() => {
        //console.log('ar_invoice_number changed:', ar_invoice_number);
        if(ar_invoice_number != null && ar_invoice_number != undefined && ar_invoice_number != '')
        {
            setValue('invoice_number', ar_invoice_number);
            setValue('ar_header_id', ar_invoice_number);
        }

    }, [ar_invoice_number]);

    useEffect(() => {

        if(keycloak.authenticated == false) return;

        fetchData('');
    
    }, [customer_id, page, pageSize, keycloak.authenticated]);


    const handleSearchClick = () => {
        setDialogOpen(true);
    }

    const IsDirty = (formName: any) => {
        if(watch(formName) == undefined || watch(formName) == null || watch(formName) == '')
        {
            return true;
        }

        return false;
    }

    const selectAR = (ar: any) => {
        //console.log(onChange);
        setValue('ar_header_id', ar.id);
        setValue('invoice_number', ar.invoice_number);

        if(onChange)
        {
            onChange({
                ar_invoice_header_id: watch('ar_header_id'),
                ar_search: watch('ar_search'),
                invoice_number: watch('invoice_number'),
            });
        }
        setDialogOpen(false);
    };

    const onPageEvent = async (page: any) => {
        setPage(page);
    };

    const onPageSizeEvent = async (size: any) => {
        setPageSize(size);
    };

    // Column Definitions: Defines & controls grid columns.
    const [colDefs, setColDefs] = useState<ColDef<ARInvoiceHeaderListDto>[]>([
    { field: "invoice_number", headerName: "Invoice #" },
    { field: "invoice_date", headerName: "Invoice Date", cellRenderer: DateOnlyRender },
    { field: "invoice_total", headerName: "Invoice Total", cellRenderer: CurrencyFormatter },
    { field: "payment_terms_name", headerName: "Payment Terms" },
    {
        field: "id",
        headerName: "Actions",
        cellRenderer: (props: any) => {
            return ( 
            <div>
                <Button type="button" colorPalette="blue" onClick={() => selectAR(props.data) }>Select</Button>
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

    const keyDown = (key: any) => {
        if(key.key == "Enter")
        {
            fetchData(watch('ar_search'));
        }
    }
    
    return (
        <div>
            <Grid
                templateColumns="repeat(2, 2fr)"
                gap={6}
                display="grid"
                width="100%"
                p="auto"
                m="auto">
                <Stack gap="0" maxW="md" w="250px">
                    <Field.Root invalid={IsDirty('invoice_number')} required={true} disabled>
                        <Field.Label><Field.RequiredIndicator /> AR Invoice</Field.Label>
                        <Input {...register('invoice_number')} placeholder="Search AR Invoices..."/>
                        <Field.ErrorText>This field is required</Field.ErrorText>
                    </Field.Root>
                </Stack>
                <Stack gap="0" maxW="md">
                    <Field.Root invalid={IsDirty('invoice_number')} required={true} disabled>
                        <Field.Label>&nbsp;</Field.Label>
                        <Button type="button" colorPalette="blue" onClick={() => handleSearchClick()} disabled={disabled}><FaSearch /></Button>
                    </Field.Root>
                </Stack>
                <Dialog.Root open={isDialogOpen} onOpenChange={(details) => setDialogOpen(details.open)} role="alertdialog">
                    <Portal>
                        <Dialog.Backdrop />
                        <Dialog.Positioner>
                            <Dialog.Content maxW="1200px" maxH="100vh" >
                                <Dialog.Header>
                                    <Dialog.Title>Search AR Invoices</Dialog.Title>
                                </Dialog.Header>
                                <Dialog.Body>
                                    <div style={{ width: "100%", height: "500px" }}>
                                        <Grid
                                        templateColumns="repeat(5, 2fr)"
                                        gap={6}
                                        display="grid"
                                        width="100%"
                                        p="auto"
                                        m="auto"
                                        >
                                        <GridItem colSpan={1}>
                                            <Field.Root>
                                                <Input {...register('ar_search')} placeholder="Search AR Invoices..."  onKeyDown={(key: any) => keyDown(key)}/>
                                            </Field.Root>
                                        </GridItem>
                                        <GridItem colSpan={6}></GridItem>
                                        <GridItem colSpan={6}>
                                            <div style={{ width: "100%", height: "400px" }}>
                                            <AgGridReact
                                                loading={loading}
                                                rowData={rowData}
                                                columnDefs={colDefs}
                                                defaultColDef={defaultColDef}
                                            />
                                            </div>
                                        </GridItem>
                                        <GridItem colSpan={3}></GridItem>
                                        <GridItem colSpan={3}>
                                            <AgGridCustomPagination totalCount={totalCount} onPage={onPageEvent} onSizeChange={onPageSizeEvent}></AgGridCustomPagination>
                                        </GridItem>
                                        </Grid>
                                    </div>
                                </Dialog.Body>
                                <Dialog.Footer>
                                    <div style={{height: "20px"}}>

                                    </div>
                                    <Dialog.CloseTrigger asChild>
                                        <CloseButton size="sm" />
                                    </Dialog.CloseTrigger>
                                </Dialog.Footer>
                            </Dialog.Content>
                        </Dialog.Positioner>
                    </Portal>
                </Dialog.Root>
            </Grid>
        </div>
    );
}

export default ARInvoiceSelectorComponent;