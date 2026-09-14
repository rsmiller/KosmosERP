"use client"

import '../../../../styles/date-picker.css';
import '../../../../styles/page.component.css';

import { useForm } from 'react-hook-form'
import {
  Grid,
  Stack,
  Input,
  GridItem,
  Field,
  Checkbox,
  Textarea,
  NativeSelect
} from '@chakra-ui/react';

import { useEffect, useState, useRef } from "react";
import { useParams, useRouter } from 'next/navigation';
import PageActionsComponent from '@/components/page-actions';
import { ChartOfAccountEditCommand, ChartOfAccountDeleteCommand, ChartOfAccountDto, AccountType, NormalBalance } from '@/models/chart-of-account-models';
import { chartOfAccountService } from '@/services/chart-of-account-service';
import { useKeycloak } from '@react-keycloak/web';
import { permissionsService, ERPModules, ERPModulePermission } from '@/services/permissions-service';

function EditChartOfAccountPage() {
    const { keycloak } = useKeycloak();
    const params = useParams();
    const router = useRouter();

    const [hasAccess, setHasAccess] = useState(true);
    const [hasEditPermission, setHasEditPermission] = useState(false);
    const [hasDeletePermission, setHasDeletePermission] = useState(false);
    const [account, setAccount] = useState<ChartOfAccountDto | null>(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);
    const [saveable, canSave] = useState(false);
    const [successSaved, setSuccessSaved] = useState(false);
    const [failedSaved, setFailedSaved] = useState(false);
    
    const hasInitialized = useRef(false);

    const {
        register,
        handleSubmit,
        formState: { errors, isValid },
        setValue,
        watch,
        control,
    } = useForm<ChartOfAccountEditCommand>();

    const loadAccount = async () => {
        try {
            setLoading(true);
            const accountId = String(params.id);
            
            if (accountId == "") {
                setError('Invalid account ID');
                return;
            }

            const response = await chartOfAccountService.getByGuid(accountId, keycloak.token || "");
            if (response.success && response.data) {
                setAccount(response.data);
                setValue('id', response.data.id);
                setValue('account_number', response.data.account_number);
                setValue('account_name', response.data.account_name);
                setValue('account_type', response.data.account_type);
                setValue('normal_balance', response.data.normal_balance);
                setValue('is_active', response.data.is_active);
                setValue('description', response.data.description);
                setValue('parent_account_id', response.data.parent_account_id);
            } else {
                setError('Failed to load account');
            }
        } catch (err) {
            console.error('Error loading account:', err);
            setError('Error loading account');
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        if (keycloak.authenticated == false) return;

        if (hasInitialized.current) return;
        hasInitialized.current = true;

        const realmRoles = keycloak?.tokenParsed?.realm_access?.roles || [];
        const hasPermission = permissionsService.HasPermission(
          ERPModules.ChartOfAccountModule,
          ERPModulePermission.Edit,
          realmRoles
        );

        if (!hasPermission) {
          setHasAccess(false);
          router.push('/erp');
          return;
        }
        setHasEditPermission(true);
        setHasDeletePermission(permissionsService.HasPermission(
          ERPModules.ChartOfAccountModule,
          ERPModulePermission.Delete,
          realmRoles
        ));

        loadAccount();
    }, [params.id, keycloak.authenticated]);

    const handleDeleteClick = async () => {
        if (!account) return;

        let deleteCommand = new ChartOfAccountDeleteCommand();
        deleteCommand.id = account.id;

        try {
            await chartOfAccountService.delete(deleteCommand, keycloak.token || "").then((response) => {
                if(response.success) {
                    router.push("/erp/chartofaccounts/");
                } else {
                    setFailedSaved(true);
                }
            });
        } catch(e) {
            setFailedSaved(true);
        }
    };

    const handleSaveClick = async () => {
        setSuccessSaved(false);

        let command = new ChartOfAccountEditCommand();
        command.id = account?.id;
        command.account_number = watch('account_number');
        command.account_name = watch('account_name');
        command.account_type = Number(watch('account_type'));
        command.normal_balance = Number(watch('normal_balance'));
        command.is_active = Boolean(watch('is_active'));
        command.description = watch('description');
        command.parent_account_id = watch('parent_account_id') ? Number(watch('parent_account_id')) : null;

        try {
            await chartOfAccountService.update(command, keycloak.token || "").then((response) => {
                if(response.success) {
                    setSuccessSaved(true);
                    setFailedSaved(false);
                } else {
                    setSuccessSaved(false);
                    setFailedSaved(true);
                }
            });
        } catch(e) {
            setSuccessSaved(false);
            setFailedSaved(true);
        }
    };

    const CheckFormValidity = () => {
        const hasRequiredFields = Boolean(watch('account_number') && watch('account_name'));
        canSave(!hasRequiredFields);
    };

    if (loading) {
        return <div>Loading account...</div>;
    }

    if (error) {
        return <div>Error: {error}</div>;
    }

    if (!account) {
        return <div>Account not found</div>;
    }

    if (!hasAccess) {
        return <div>Redirecting...</div>;
    }

    return (
        <form onSubmit={handleSubmit(handleSaveClick)}>
            <PageActionsComponent 
                showDelete={hasDeletePermission}
                showSave={hasEditPermission} 
                canSave={saveable}
                showSaveSuccess={successSaved}
                showSaveFailed={failedSaved}
                onDelete={handleDeleteClick} 
                onSave={handleSaveClick} 
            />
            <Grid
                templateColumns="repeat(5, 2fr)"
                gap={6}
                display="grid"
                width="100%"
                p="auto"
                m="auto"
            >
                <GridItem colSpan={6}>
                    <h1>Edit Account</h1>
                </GridItem>
                
                <Stack gap="4" align="flex-start" maxW="md">
                    <Field.Root required invalid={!!errors.account_number}>
                        <Field.Label>Account Number</Field.Label>
                        <Input 
                            {...register('account_number', { required: 'Account number is required' })}
                            onChange={(e) => { setValue('account_number', e.target.value); CheckFormValidity(); }}
                        />
                        <Field.ErrorText>{errors.account_number?.message}</Field.ErrorText>
                    </Field.Root>
                </Stack>
                <Stack gap="4" align="flex-start" maxW="md">
                    <Field.Root required invalid={!!errors.account_name}>
                        <Field.Label>Account Name</Field.Label>
                        <Input 
                            {...register('account_name', { required: 'Account name is required' })}
                            onChange={(e) => { setValue('account_name', e.target.value); CheckFormValidity(); }}
                        />
                        <Field.ErrorText>{errors.account_name?.message}</Field.ErrorText>
                    </Field.Root>
                </Stack>
                <GridItem colSpan={4}></GridItem>
                
                <Stack gap="4" align="flex-start" maxW="md">
                    <Field.Root>
                        <Field.Label>Account Type</Field.Label>
                        <NativeSelect.Root>
                            <NativeSelect.Field 
                                {...register('account_type')}
                                onChange={(e) => { 
                                    const val = Number(e.target.value);
                                    setValue('account_type', val);
                                    if (val === AccountType.Asset || val === AccountType.Expense) {
                                        setValue('normal_balance', NormalBalance.Debit);
                                    } else {
                                        setValue('normal_balance', NormalBalance.Credit);
                                    }
                                    CheckFormValidity();
                                }}
                            >
                                <option value={AccountType.Asset}>Asset</option>
                                <option value={AccountType.Liability}>Liability</option>
                                <option value={AccountType.Equity}>Equity</option>
                                <option value={AccountType.Revenue}>Revenue</option>
                                <option value={AccountType.Expense}>Expense</option>
                            </NativeSelect.Field>
                        </NativeSelect.Root>
                    </Field.Root>
                </Stack>
                <Stack gap="4" align="flex-start" maxW="md">
                    <Field.Root>
                        <Field.Label>Normal Balance</Field.Label>
                        <NativeSelect.Root>
                            <NativeSelect.Field 
                                {...register('normal_balance')}
                                onChange={(e) => { setValue('normal_balance', Number(e.target.value)); CheckFormValidity(); }}
                            >
                                <option value={NormalBalance.Debit}>Debit</option>
                                <option value={NormalBalance.Credit}>Credit</option>
                            </NativeSelect.Field>
                        </NativeSelect.Root>
                    </Field.Root>
                </Stack>
                <GridItem colSpan={4}></GridItem>

                <Stack gap="4" align="flex-start" maxW="md">
                    <Field.Root>
                        <Checkbox.Root
                            checked={watch('is_active')}
                            onCheckedChange={(e) => { setValue('is_active', !!e.checked); CheckFormValidity(); }}
                        >
                            <Checkbox.HiddenInput />
                            <Checkbox.Control />
                            <Checkbox.Label>Active</Checkbox.Label>
                        </Checkbox.Root>
                    </Field.Root>
                </Stack>
                <GridItem colSpan={5}></GridItem>

                <GridItem colSpan={3}>
                    <Field.Root>
                        <Field.Label>Description</Field.Label>
                        <Textarea 
                            {...register('description')}
                            onChange={(e) => { setValue('description', e.target.value); CheckFormValidity(); }}
                        />
                    </Field.Root>
                </GridItem>
                <GridItem colSpan={3}></GridItem>
            </Grid>
        </form>
    );
}

export default EditChartOfAccountPage;
