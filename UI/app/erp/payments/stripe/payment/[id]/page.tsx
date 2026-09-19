'use client';

import '../../../../../styles/payments-stripe.css';

import { useEffect, useState, useRef } from 'react';
import { Elements, PaymentElement, useStripe, useElements } from '@stripe/react-stripe-js';
import { paymentService } from '@/services/payment-service';
import { Box, Button, Heading, Text, VStack, HStack, Spinner, } from '@chakra-ui/react';
import { stripePromise } from '@/lib/stripe';
import { CreateStripePaymentIntent, PaymentProviderCreateDto, StripePaymentIntentStatusCommand } from '@/models/payment-models';
import SessionStorage from '@/components/session-storage';
import { useParams, useSearchParams } from 'next/navigation';
import { useAuth } from '@/lib/auth/auth-context';


export default function StripePaymentPage() {
    const auth = useAuth();
    const params = useParams();
    const searchParams = useSearchParams();

    const [stripePaymentResponse, setStripePaymentResponse] = useState<PaymentProviderCreateDto | null>(null);
    
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    const hasInitialized = useRef(false);

    const [arInvoiceGuid, setARInvoiceGuid] = useState<string>("");
    const [paymentMethodId, setPaymentMethodId] = useState<string>("");
    const [paymentStatus, setPaymentStatus] = useState<string | null>(null);

    const fetchStripeSession = async () => {
        const ar_invoice_guid = String(params.id);
        const payment_method_id = searchParams.get('payment_method_id') || "";

        if(ar_invoice_guid == '' || ar_invoice_guid == 'undefined')
            return;


        setARInvoiceGuid(ar_invoice_guid);
        setPaymentMethodId(payment_method_id);

        try {
            setLoading(true);
            
            let command = new CreateStripePaymentIntent();
            command.ar_header_guid = ar_invoice_guid;

            if(payment_method_id != null && payment_method_id != "")
            {
                command.payment_method_id = payment_method_id;
            }

            let response = await paymentService.getStripeSessionFromARInvoce(command, auth.token || "");

            if (response && response.data) {
                setStripePaymentResponse(response.data);
                setError(null);
            } else {
                setError('Failed to retrieve payment session');
            }
        } catch (err: any) {
            //console.error('Error fetching Stripe session:', err);

            if(err.response != null && err.response.data && err.response.data.exception)
            {
                setError(err.response.data.exception);
            }
            else{
                setError('Unable to load payment session. Please try again.');
            }
            
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        
        if(auth.authenticated == false) return;
        

        if(hasInitialized.current)
            return;
        
        hasInitialized.current = true;
        
        

        fetchStripeSession();
    }, [params, searchParams, auth.authenticated]);


    const processIndividualPayment = async () => {
        let command = new StripePaymentIntentStatusCommand();
        command.payment_intent_id = stripePaymentResponse?.id || "";

        await paymentService.payStripePaymentIntent(command, auth.token || "").then((response) => {
            if (response && response.data) {
                setPaymentStatus(response.data.status || null);

                setError(null);
            } else {
                setError('Failed to process payment');
            }
        });
    }


    if (loading) {
        return (
            <Box display="flex" justifyContent="center" alignItems="center" minH="100vh">
                <VStack>
                    <Spinner size="xl" color="blue.500" />
                    <Text>Loading payment form...</Text>
                </VStack>
            </Box>
        );
    }

    if (error) {
        return (
            <Box display="flex" justifyContent="center" alignItems="center" minH="100vh" p={4}>
                <VStack maxW="md">
                    <Heading size="lg" color="red.500">Payment Error</Heading>
                    <Text textAlign="center">{error}</Text>
                    <Button colorScheme="blue" onClick={() => window.location.reload()}>
                        Try Again
                    </Button>
                </VStack>
            </Box>
        );
    }

    if (!stripePaymentResponse?.clientSecret) {
        return (
            <Box display="flex" justifyContent="center" alignItems="center" minH="100vh" p={4}>
                <VStack maxW="md">
                    <Heading size="lg">Payment Unavailable</Heading>
                    <Text textAlign="center">Unable to initialize payment session. Please contact support.</Text>
                </VStack>
            </Box>
        );
    }

    const options = {
        clientSecret: stripePaymentResponse?.clientSecret,
        appearance: {
            theme: 'stripe' as const,
        },
    };

    if(paymentStatus === 'succeeded')
    {
        return (
            <Box minH="100vh" bg="gray.50" py={8} px={4}>
                <VStack maxW="2xl" mx="auto">
                    <Box textAlign="center">
                        <Heading size="xl">Complete Payment</Heading>
                        <Text color="gray.600" mt={2}>Invoice: {stripePaymentResponse.ar_invoice_number}</Text>
                        <Text color="gray.600" mt={2}>Amount: ${stripePaymentResponse.amount}</Text>
                    </Box>
                    <Box
                        w="full"
                        p={3}
                        borderRadius="md"
                        bg={paymentStatus === 'succeeded' ? 'green.50' : 'blue.50'}
                        borderColor={paymentStatus === 'succeeded' ? 'green.200' : 'blue.200'}
                        borderWidth="1px"
                    >
                        <Text
                            color={paymentStatus === 'succeeded' ? 'green.700' : 'blue.700'}
                            fontSize="sm"
                        >
                            Payment successful!
                        </Text>
                    </Box>
                </VStack>
            </Box>
        )
    }

    if (stripePaymentResponse && (paymentMethodId == "" || paymentMethodId == null)) {
        return (
            <Box minH="100vh" bg="gray.50" py={8} px={4}>
                <VStack maxW="2xl" mx="auto">
                    <Box textAlign="center">
                        <Heading size="xl">Complete Payment</Heading>
                        <Text color="gray.600" mt={2}>Invoice: {stripePaymentResponse.ar_invoice_number}</Text>
                        <Text color="gray.600" mt={2}>Amount: ${stripePaymentResponse.amount}</Text>
                    </Box>

                    <Box w="full" bg="white" p={8} borderRadius="lg" boxShadow="md">
                        <Elements stripe={stripePromise} options={options}>
                            <StripePaymentForm payment_intent_id={stripePaymentResponse.id} />
                        </Elements>
                    </Box>
                </VStack>
            </Box>
        );
    }

    if (stripePaymentResponse && paymentMethodId != "" && paymentMethodId != null) {
        return (
            <Box minH="100vh" bg="gray.50" py={8} px={4}>
                <VStack maxW="2xl" mx="auto">
                    <Box textAlign="center">
                        <Heading size="xl">Complete Payment</Heading>
                        <Text color="gray.600" mt={2}>Invoice: {stripePaymentResponse.ar_invoice_number}</Text>
                        <Text color="gray.600" mt={2}>Amount: ${stripePaymentResponse.amount}</Text>
                    </Box>

                    <Box w="full" bg="white" p={8} borderRadius="lg" boxShadow="md" textAlign="center">
                        <Button colorPalette="black" size="lg" onClick={() => processIndividualPayment()}>
                            Pay ${stripePaymentResponse.amount} Payment
                        </Button>
                    </Box>
                </VStack>
            </Box>
        );
    }
    
}



function StripePaymentForm({ payment_intent_id }: { payment_intent_id: string }) {
    const auth = useAuth();

    const stripe = useStripe();
    const elements = useElements();
    const [isProcessing, setIsProcessing] = useState(false);
    const [statusMessage, setStatusMessage] = useState<string | null>(null);
    const [paymentStatus, setPaymentStatus] = useState<string | null>(null);

    const checkPaymentStatus = async () => {
       
        try {
            let command = new StripePaymentIntentStatusCommand();
            command.payment_intent_id = payment_intent_id;

            const status_response = await paymentService.getStripeSessionStatus(command, auth.token || "");
            //console.log('Payment status:', status_response);

            if (status_response && status_response.data) {

                setPaymentStatus(status_response.data.status || null);

                if (status_response.data.status === 'succeeded') {
                    setStatusMessage('Payment successful!');
                    
                    setIsProcessing(false);
                }
            }
        } catch (err) {
            console.error('Error checking payment status:', err);
        }
    };

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();

        if (!stripe || !elements) {
            return;
        }

        setIsProcessing(true);
        setStatusMessage(null);

        try {
            const result = await stripe.confirmPayment({
                elements,
                redirect: 'if_required',
            });

            //console.log('Stripe Result:', result);

            if (result.error) {
                setStatusMessage(`Payment failed: ${result.error.message}`);
                setIsProcessing(false);
            } else {
                setStatusMessage('Payment processing...');
                
                // Poll for payment status with exponential backoff
                let attempts = 0;
                const maxAttempts = 5;
                let delay = 3000;
                
                const pollPaymentStatus = async () => {
                    while (attempts < maxAttempts) {
                        attempts++;
                        
                        try {
                            let command = new StripePaymentIntentStatusCommand();
                            command.payment_intent_id = payment_intent_id;

                            const status_response = await paymentService.getStripeSessionStatus(command, auth.token || "");
                            
                            if (status_response && status_response.data) {
                                const status = status_response.data.status;
                                setPaymentStatus(status || null);
                                
                                if (status === 'succeeded') {
                                    setStatusMessage('Payment successful!');
                                    setIsProcessing(false);
                                    setPaymentStatus('succeeded');
                                    return true;
                                }
                            }
                        } catch (err) {
                            console.error('Error checking payment status:', err);
                        }
                        
                        // Wait before next attempt (max 5 seconds between attempts)
                        await new Promise(resolve => setTimeout(resolve, Math.min(delay, 5000)));
                        delay *= 1.5; // Exponential backoff
                    }
                    
                    // After max attempts, check one final time
                    setStatusMessage('Payment is still processing. Please check your account.');
                    setIsProcessing(false);
                    return false;
                };
                
                pollPaymentStatus();
            }
        } catch (err) {
            console.error('Payment error:', err);
            setStatusMessage('An unexpected error occurred. Please try again.');
            setIsProcessing(false);
        }
    };

    return (
        <form onSubmit={handleSubmit}>
            <VStack>
                <div className={paymentStatus == 'succeeded'? 'hideSection' : "processing"}>
                    <PaymentElement/>
                </div>
                {statusMessage && (
                    <Box
                        w="full"
                        p={3}
                        borderRadius="md"
                        bg={paymentStatus === 'succeeded' ? 'green.50' : 'blue.50'}
                        borderColor={paymentStatus === 'succeeded' ? 'green.200' : 'blue.200'}
                        borderWidth="1px"
                    >
                        <Text
                            color={paymentStatus === 'succeeded' ? 'green.700' : 'blue.700'}
                            fontSize="sm"
                        >
                            Payment successful!
                        </Text>
                    </Box>
                )}

                <Button
                    type="submit"
                    colorScheme="blue"
                    size="lg"
                    width="full"
                    disabled={!stripe || !elements || isProcessing}
                    className={paymentStatus == 'succeeded'? 'hideSection' : "processing"}
                >
                    {isProcessing ? 'Processing...' : 'Pay Now'}
                </Button>
            </VStack>
        </form>
    );
} 