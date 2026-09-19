"use client"

import 'ag-grid-community/styles/ag-theme-quartz.css';
import '../../../../styles/page.component.css';
import '../../../../styles/data-list.css';
import '../../../../styles/date-picker.css';


import { useEffect, useState, useRef } from "react";
import { DataList, Grid, GridItem, useFilter } from '@chakra-ui/react'
import { useParams } from 'next/navigation';
import { SubscriptionDto } from '@/models/subscription-models';
import { subscriptionService } from '@/services/subscription-service';
import { format } from 'date-fns';

import SalesOrdersLinesComponent from '@/components/lists/sales-order-lines-component';
import { CurrencyHelper } from '@/helpers/CurrencyHelper';
import { useAuth } from '@/lib/auth/auth-context';

function ViewSubscriptionsPage() {
    const auth = useAuth();
    const params = useParams();
    
    const [subscription, setSubscription] = useState<SubscriptionDto | null>(null);

    const [error, setError] = useState<string | null>(null);

    const [loading, setLoading] = useState(true);
    const hasInitialized = useRef(false);

    const loadSubscription = async () => {
        try {
            setLoading(true);
            const subscriptionId = String(params.id);
            
            if (subscriptionId == "") {
                setError('Invalid Subscription ID');
                return;
            }
    
            const response = await subscriptionService.getByGuid(subscriptionId, auth.token || "");
            if (response.success && response.data) {
                //console.log(response)
                setSubscription(response.data);
            } else {
                setError('Failed to load subscription');
            }
        } catch (err) {
            console.error('Error loading Subscription:', err);
            setError('Error loading Subscription');
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        if(auth.authenticated == false) return;
            
        if (hasInitialized.current) return;
        hasInitialized.current = true;

        
    
        loadSubscription();
    }, [params.id, auth.authenticated]);


    const DateFormatter = (date: any) =>
    {
        if(date)
        {
            return format(date, 'MM-dd-yyyy');
        }
        else
        {
            return "";
        }
    }

    if (loading) {
        return <div>Loading Subscription...</div>;
    }

    if (error) {
        return <div>Error: {error}</div>;
    }

    return (
        <Grid
            templateColumns="repeat(4, 2fr)"
            gap={6}
            display="grid"
            width="100%"
            p="auto"
            m="auto"
            >
            <GridItem colSpan={1} gap={0}>
                <h3>Subscription Information</h3>
                <DataList.Root size="lg" orientation="horizontal" divideY="1px" maxW="md">
                    <DataList.Item>
                        <DataList.ItemLabel>Subscription #</DataList.ItemLabel>
                        <DataList.ItemValue>{subscription?.subscription_number}</DataList.ItemValue>
                    </DataList.Item>
                </DataList.Root>
                <DataList.Root size="lg" orientation="horizontal" divideY="1px" maxW="md">
                    <DataList.Item>
                        <DataList.ItemLabel>Terms</DataList.ItemLabel>
                        <DataList.ItemValue>
                            {subscription?.cycle_days} Days
                        </DataList.ItemValue>
                    </DataList.Item>
                </DataList.Root>
                <DataList.Root size="lg" orientation="horizontal" divideY="1px" maxW="md">
                    <DataList.Item>
                        <DataList.ItemLabel>Start Date</DataList.ItemLabel>
                        <DataList.ItemValue>
                            {subscription?.start_date}
                        </DataList.ItemValue>
                    </DataList.Item>
                </DataList.Root>
            </GridItem>

            <GridItem colSpan={1}>
                <h3>&nbsp;</h3>
                <DataList.Root size="lg" orientation="horizontal" divideY="1px" maxW="md">
                    <DataList.Item>
                        <DataList.ItemLabel>&nbsp;</DataList.ItemLabel>
                        <DataList.ItemValue>&nbsp;</DataList.ItemValue>
                    </DataList.Item>
                </DataList.Root>
                <DataList.Root size="lg" orientation="horizontal" divideY="1px" maxW="md">
                    <DataList.Item>
                        <DataList.ItemLabel>&nbsp;</DataList.ItemLabel>
                        <DataList.ItemValue>&nbsp;</DataList.ItemValue>
                    </DataList.Item>
                </DataList.Root>
                <DataList.Root size="lg" orientation="horizontal" divideY="1px" maxW="md">
                    <DataList.Item>
                        <DataList.ItemLabel>Next Date</DataList.ItemLabel>
                        <DataList.ItemValue>{subscription?.next_date}</DataList.ItemValue>
                    </DataList.Item>
                </DataList.Root>
            </GridItem>

            <GridItem colSpan={1}>
                <h3>Order Information</h3>
                <DataList.Root size="lg" orientation="horizontal" divideY="1px" maxW="md">
                    <DataList.Item>
                        <DataList.ItemLabel>Order Number</DataList.ItemLabel>
                        <DataList.ItemValue>{subscription?.order?.order_number}</DataList.ItemValue>
                    </DataList.Item>
                </DataList.Root>
                <DataList.Root size="lg" orientation="horizontal" divideY="1px" maxW="md">
                    <DataList.Item>
                        <DataList.ItemLabel>Order Date</DataList.ItemLabel>
                        <DataList.ItemValue>{subscription?.order?.order_date}</DataList.ItemValue>
                    </DataList.Item>
                </DataList.Root>
            </GridItem>

            <GridItem colSpan={1}>
                <h3>&nbsp;</h3>
                <DataList.Root size="lg" orientation="horizontal" divideY="1px" maxW="md">
                    <DataList.Item>
                        <DataList.ItemLabel>Price</DataList.ItemLabel>
                        <DataList.ItemValue>{CurrencyHelper(subscription?.order?.price)}</DataList.ItemValue>
                    </DataList.Item>
                </DataList.Root>
                <DataList.Root size="lg" orientation="horizontal" divideY="1px" maxW="md">
                    <DataList.Item>
                        <DataList.ItemLabel>Tax</DataList.ItemLabel>
                        <DataList.ItemValue>{CurrencyHelper(subscription?.order?.tax)}</DataList.ItemValue>
                    </DataList.Item>
                </DataList.Root>
            </GridItem>

            <GridItem colSpan={4} >
                <h3>Order Lines</h3>
                <SalesOrdersLinesComponent order_header_id={subscription?.order_header_id} onSelect={() => {}}/>
            </GridItem>
        </Grid>
    );
}

export default ViewSubscriptionsPage;