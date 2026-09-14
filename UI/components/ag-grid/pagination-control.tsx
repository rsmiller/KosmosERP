import { ButtonGroup, createListCollection, IconButton, Pagination, Portal, Select } from "@chakra-ui/react";
import { useState } from "react";
import { HiChevronLeft, HiChevronRight } from "react-icons/hi";

function AgGridCustomPagination({totalCount, onPage, onSizeChange}: any) {
    const pagination_options = createListCollection({items: [{ label: "20", value: "20" }, { label: "50", value: "50" }, { label: "100", value: "100" },  { label: "200", value: "200" }]});

    const [pageSize, setPageSize] = useState<number>(50);
    
    const onSelect = (details: any) => {
        if(details && details.value && details.value.length > 0)
        {
            setPageSize(details.value[0]);
            onSizeChange(details.value[0]);
        }
    }
    
    return (
        <div className={"pagination-block"}>
            <div className={"pagination-size-block"}>
                <div className={"sit-me-side-by-side padding-md-right"}>Total:&nbsp; {totalCount}&nbsp;</div>
                <div className={"sit-me-side-by-side"}>Page Size:&nbsp;</div>
                <div className={"sit-me-side-by-side"}>
                    <Select.Root collection={pagination_options} size="sm" width="75px" defaultValue={[pageSize.toString()]} onValueChange={onSelect}>
                    <Select.HiddenSelect />
                    <Select.Control>
                        <Select.Trigger>
                        <Select.ValueText placeholder="Select framework" />
                        </Select.Trigger>
                        <Select.IndicatorGroup>
                        <Select.Indicator />
                        </Select.IndicatorGroup>
                    </Select.Control>
                    <Portal>
                        <Select.Positioner>
                        <Select.Content>
                            {pagination_options.items.map((po: any) => (
                            <Select.Item item={po} key={po.value}>
                                {po.label}
                                <Select.ItemIndicator />
                            </Select.Item>
                            ))}
                        </Select.Content>
                        </Select.Positioner>
                    </Portal>
                    </Select.Root>
                </div>
            </div>
            <div className={"pagination-pagination-block"}>
                <Pagination.Root count={totalCount} pageSize={pageSize} defaultPage={1} onPageChange={(e) => onPage(e.page)}>
                <ButtonGroup gap="4" variant="ghost">
                    <Pagination.PrevTrigger asChild>
                    <IconButton>
                        <HiChevronLeft />
                    </IconButton>
                    </Pagination.PrevTrigger>
                    <Pagination.PageText />
                    <Pagination.NextTrigger asChild>
                    <IconButton>
                        <HiChevronRight />
                    </IconButton>
                    </Pagination.NextTrigger>
                </ButtonGroup>
                </Pagination.Root>
            </div>
        </div>
    );
}

export default AgGridCustomPagination;