import { useEffect, useRef, useState } from "react";
import { CustomCellEditorProps } from "ag-grid-react";
import {
    Combobox,
    Portal,
    useFilter,
    useListCollection,
} from "@chakra-ui/react";
import { useAuth } from '@/lib/auth/auth-context';
import { chartOfAccountService } from "@/services/chart-of-account-service";
import { ChartOfAccountFindCommand, ChartOfAccountListDto } from "@/models/chart-of-account-models";

/** "1010 - Operating Cash" */
export function accountLabel(account: { account_number?: string | null; account_name?: string | null }): string {
    if (!account.account_number) return "";
    return account.account_name ? `${account.account_number} - ${account.account_name}` : account.account_number;
}

/**
 * Picks a chart of accounts entry for a grid row (journal entry lines). The cell
 * value is the account id; on a pick, the row's account_number and account_name
 * are updated too, so the grid can show "1010 - Operating Cash" (BUG-021).
 * Typing filters on the label, so accounts are found by number or name.
 * Use with `cellEditorPopup: true` on the column.
 */
function ChartOfAccountCellEditor(props: CustomCellEditorProps) {
    const auth = useAuth();
    const { contains } = useFilter({ sensitivity: "base" });
    const [accounts, setAccounts] = useState<ChartOfAccountListDto[]>([]);
    const [loaded, setLoaded] = useState(false);
    const inputRef = useRef<HTMLInputElement>(null);

    const { collection, filter, set } = useListCollection<ChartOfAccountListDto>({
        initialItems: [],
        filter: contains,
        itemToString: (item) => accountLabel(item),
        itemToValue: (item) => String(item.id),
    });

    useEffect(() => {
        if (!auth.token) return;

        const command = new ChartOfAccountFindCommand();
        command.is_active = true;
        chartOfAccountService.find(command, auth.token, 1, 1000, "account_number-asc")
            .then((response) => {
                const items = response.success && response.data ? response.data : [];
                setAccounts(items);
                set(items);
            })
            .catch((error) => console.error("Error loading accounts:", error))
            .finally(() => setLoaded(true));
    }, [auth.token, set]);

    // Focus once the combobox is mounted, so typing opens the list straight away.
    // autoFocus fires before the combobox starts listening and leaves it closed.
    useEffect(() => {
        if (!loaded) return;
        const frame = requestAnimationFrame(() => inputRef.current?.focus());
        return () => cancelAnimationFrame(frame);
    }, [loaded]);

    const onPick = (accountId: string | undefined) => {
        const account = accounts.find((a) => String(a.id) === accountId);
        if (!account) return;

        if (props.data) {
            props.data.account_number = account.account_number;
            props.data.account_name = account.account_name;
        }
        props.onValueChange(account.id);
        props.stopEditing();
    };

    if (!loaded) {
        return <div>Loading accounts...</div>;
    }

    return (
        <Combobox.Root
            collection={collection}
            defaultValue={props.value ? [String(props.value)] : []}
            onValueChange={(e) => onPick(e.value[0])}
            onInputValueChange={(e) => filter(e.inputValue)}
            openOnClick
            width="320px"
        >
            <Combobox.Control>
                <Combobox.Input ref={inputRef} placeholder="Account number or name" aria-label="Account" />
                <Combobox.IndicatorGroup>
                    <Combobox.Trigger />
                </Combobox.IndicatorGroup>
            </Combobox.Control>
            <Portal>
                <Combobox.Positioner>
                    <Combobox.Content>
                        <Combobox.Empty>No accounts found</Combobox.Empty>
                        {collection.items.map((item) => (
                            <Combobox.Item item={item} key={item.id}>
                                {accountLabel(item)}
                                <Combobox.ItemIndicator />
                            </Combobox.Item>
                        ))}
                    </Combobox.Content>
                </Combobox.Positioner>
            </Portal>
        </Combobox.Root>
    );
}

export default ChartOfAccountCellEditor;
