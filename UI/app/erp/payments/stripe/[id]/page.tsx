"use client";

import '../../../../styles/payments-stripe.css';

import SessionStorage from "@/components/session-storage";
import { CreateStripeNewCardIntentCommand, GetSavedPaymentMethodsCommand, SavedBankDto, SavedCreditCardDto } from "@/models/payment-models";
import { paymentService } from "@/services/payment-service";
import { useParams, useRouter } from "next/navigation";
import { useEffect, useRef, useState } from "react";
import { Box, Heading, VStack, HStack, Card, Button, Text, Spinner, Icon } from "@chakra-ui/react";
import { FaCashRegister, FaCreditCard, FaUniversity } from "react-icons/fa";
import { CardElement, Elements, useElements, useStripe } from "@stripe/react-stripe-js";
import { stripePromise } from '@/lib/stripe';
import { useAuth } from '@/lib/auth/auth-context';


export default function StripeExistingPaymentPage() {
    const auth = useAuth();
    const params = useParams();
    const router = useRouter();
    
    const userId = SessionStorage.getUserId();
    const sessionId = SessionStorage.getSession();

    const [savedCards, setSavedCards] = useState<SavedCreditCardDto[]>([]);
    const [savedBanks, setSavedBanks] = useState<SavedBankDto[]>([]);

    const [loading, setLoading] = useState(true);

    const [selectedId, setSelectedId] = useState<string | null>(null);
    const [arInvoiceGuid, setARInvoiceGuid] = useState<string>("");

    const hasInitialized = useRef(false);


    useEffect(() => {
        const ar_invoice_guid = String(params.id);

        if(ar_invoice_guid == '')
            return;

        if(hasInitialized.current)
            return;
        
        setARInvoiceGuid(ar_invoice_guid);

        hasInitialized.current = true;

        getSavedCards();

    }, []);

    const getSavedCards = () => {

        let command = new GetSavedPaymentMethodsCommand();
        command.ar_invoice_header_guid = arInvoiceGuid;

        paymentService.getSavedPaymentMethods(command, auth.token || "").then((response) => 
        {
            //console.log("Saved Payment Methods Response:", response);

            if(response.success && response.data) {
                setSavedCards(response.data.credit_cards);
                setSavedBanks(response.data.banks);
            }
            setLoading(false);
        });
    }


    const handleSelectPaymentMethod = (id: string) => {
        setSelectedId(id);
        //console.log("Selected payment method ID:", id);

        if(id === "onetime") {
            router.push("/erp/payments/stripe/payment/" + arInvoiceGuid);
        }
        else
        {
            router.push("/erp/payments/stripe/payment/" + arInvoiceGuid + "?payment_method_id=" + id);
        }
    };

    if (loading) {
        return (
            <Box display="flex" justifyContent="center" alignItems="center" minH="100vh">
                <VStack gap={4}>
                    <Spinner size="xl" colorPalette="blue" />
                    <Text>Loading payment methods...</Text>
                </VStack>
            </Box>
        );
    }

    const options = {
        appearance: {
            theme: 'stripe' as const,
        },
    };

    return (
        <Box minH="100vh" bg="gray.50" py={8} px={4}>
            <VStack maxW="4xl" mx="auto" gap={8}>
                <Box textAlign="center">
                    <Heading size="xl">Select Payment Method</Heading>
                    <Text color="gray.500" mt={2}>Choose a saved card or bank account to continue</Text>
                </Box>

                <VStack gap={6} w="full">
                    <VStack w="full" alignItems="flex-start" gap={4}>
                        <HStack wrap="wrap" gap={4} w="full" justifyContent="center">
                            <Card.Root
                                key="onetime"
                                size="md"
                                height="200px"
                                width="250px"
                                borderWidth="2px"
                                borderColor={selectedId === "onetime" ? "gray.600" : "gray.200"}
                                bg={selectedId === "onetime" ? "gray.50" : "white"}
                                cursor="pointer"
                                transition="all 0.2s"
                                _hover={{
                                    borderColor: "gray.400",
                                    boxShadow: "md"
                                }}
                                onClick={() => handleSelectPaymentMethod("onetime")}
                            >
                                <Card.Body>
                                    <VStack alignItems="flex-start" gap={4}>
                                        <HStack>
                                            <Icon asChild color="gray.600" fontSize="24px">
                                                <FaCashRegister />
                                            </Icon>
                                            <Heading size="sm">One Time Payment</Heading>
                                        </HStack>
                                        
                                        <VStack gap={2} w="full" textAlign="center">
                                            <Text fontSize="lg" fontWeight="bold">
                                                Bank or Credit Card
                                            </Text>
                                            <Text fontSize="2xl" fontWeight="bold" letterSpacing="2px">
                                                &nbsp;
                                            </Text>
                                        </VStack>

                                        <Button
                                            w="full"
                                            colorPalette={selectedId === "onetime" ? "gray" : "gray"}
                                            size="sm"
                                            variant={selectedId === "onetime" ? "solid" : "outline"}
                                            onClick={(e) => {
                                                e.stopPropagation();
                                                handleSelectPaymentMethod("onetime");
                                            }}
                                        >
                                            {selectedId === "onetime" ? "Selected" : "Select"}
                                        </Button>
                                    </VStack>
                                </Card.Body>
                            </Card.Root>

                            {savedCards && savedCards.map((card) => (
                                <Card.Root
                                    key={card.id}
                                    size="md"
                                    height="200px"
                                    width="250px"
                                    borderWidth="2px"
                                    borderColor={selectedId === card.id ? "blue.600" : "gray.200"}
                                    bg={selectedId === card.id ? "blue.50" : "white"}
                                    cursor="pointer"
                                    transition="all 0.2s"
                                    _hover={{
                                        borderColor: "blue.400",
                                        boxShadow: "md"
                                    }}
                                    onClick={() => handleSelectPaymentMethod(card.id)}
                                >
                                    <Card.Body>
                                        <VStack alignItems="flex-start" gap={4}>
                                            <HStack>
                                                <Icon asChild color="blue.600" fontSize="24px">
                                                    <FaCreditCard />
                                                </Icon>
                                                <Heading size="sm">{card.brand?.toUpperCase()}</Heading>
                                            </HStack>
                                            
                                            <VStack textAlign="center" gap={2} w="full">
                                                <Text fontSize="2xl" fontWeight="bold" letterSpacing="2px">
                                                    •••• {card.last4}
                                                </Text>
                                                <Text fontSize="sm" color="gray.500">
                                                    Expires {card.exp_month}/{card.exp_year}
                                                </Text>
                                            </VStack>

                                            <Button
                                                w="full"
                                                colorPalette={selectedId === card.id ? "blue" : "gray"}
                                                size="sm"
                                                variant={selectedId === card.id ? "solid" : "outline"}
                                                onClick={(e) => {
                                                    e.stopPropagation();
                                                    handleSelectPaymentMethod(card.id);
                                                }}
                                            >
                                                {selectedId === card.id ? "Selected" : "Select"}
                                            </Button>
                                        </VStack>
                                    </Card.Body>
                                </Card.Root>
                            ))}

                            {savedBanks && savedBanks.map((bank) => (
                                <Card.Root
                                    key={bank.id}
                                    size="md"
                                    height="200px"
                                    width="250px"
                                    borderWidth="2px"
                                    borderColor={selectedId === bank.id ? "green.600" : "gray.200"}
                                    bg={selectedId === bank.id ? "green.50" : "white"}
                                    cursor="pointer"
                                    transition="all 0.2s"
                                    _hover={{
                                        borderColor: "green.400",
                                        boxShadow: "md"
                                    }}
                                    onClick={() => handleSelectPaymentMethod(bank.id)}
                                >
                                    <Card.Body>
                                        <VStack alignItems="flex-start" gap={4}>
                                            <HStack>
                                                <Icon asChild color="green.600" fontSize="24px">
                                                    <FaUniversity />
                                                </Icon>
                                                <Heading size="sm">Bank Account</Heading>
                                            </HStack>
                                            
                                            <VStack textAlign="center" gap={2} w="full">
                                                <Text fontSize="lg" fontWeight="bold">
                                                    {bank.bank}
                                                </Text>
                                                <Text fontSize="sm" color="gray.500">
                                                    Account ending in {bank.last4}
                                                </Text>
                                            </VStack>

                                            <Button
                                                w="full"
                                                colorPalette={selectedId === bank.id ? "green" : "gray"}
                                                size="sm"
                                                variant={selectedId === bank.id ? "solid" : "outline"}
                                                onClick={(e) => {
                                                    e.stopPropagation();
                                                    handleSelectPaymentMethod(bank.id);
                                                }}
                                            >
                                                {selectedId === bank.id ? "Selected" : "Select"}
                                            </Button>
                                        </VStack>
                                    </Card.Body>
                                </Card.Root>
                            ))}
                        </HStack>
                    </VStack>
                </VStack>

                <Box textAlign="center">
                    <Heading size="xl">Save New Card Payment</Heading>
                </Box>

                <Card.Root
                    key="onetime"
                    size="lg"
                    w="400px"
                    h="225px">
                    <Card.Body>
                        <Elements stripe={stripePromise} options={options}>
                            <NewCardForm ar_header_guid={arInvoiceGuid} onCreated={() => { getSavedCards(); }} />
                        </Elements>
                    </Card.Body>
                </Card.Root>

                

                {selectedId && (
                    <Box w="full" textAlign="center">
                        <Text fontSize="sm" color="gray.500" mb={4}>
                            Selected: {selectedId}
                        </Text>
                        <Button colorPalette="blue" size="lg">
                            Continue to Payment
                        </Button>
                    </Box>
                )}
            </VStack>
        </Box>
    );
}

function NewCardForm({ ar_header_guid, onCreated }: { ar_header_guid: string, onCreated: () => void })
{
    const auth = useAuth();
    const stripe = useStripe();
    const elements = useElements();

    const [status, setStatus] = useState("");

    async function handleSubmit(e: any) {
        e.preventDefault();

        const card = elements?.getElement(CardElement);
        if (!stripe || !card) {
            return;
        }

        let command = new CreateStripeNewCardIntentCommand();
        command.ar_header_guid = ar_header_guid;

        paymentService.createStripeNewCardIntent(command, auth.token || "").then(async (response) => 
        {
            //console.log("Create New Card Intent Response:", response);

            if(response.success && response.data) {

                const result = await stripe?.confirmCardSetup(response.data.clientSecret, {
                    payment_method: {
                        card: card
                    }
                });

                if (result.error) {
                    setStatus(result.error.message || "An error occurred");
                } else {
                    setStatus("Card saved!");
                    onCreated();
                }
            }
        });
        
    }

    const options = {
        classes: {
            base: "CardElement-base",
            invalid: "CardElement-invalid"
        },
        style: {
            base: {
                color: "#2d3748"
            }
        }
    };

    return (
        <form onSubmit={handleSubmit}>
            <Text color="gray.500" mt={2}>Card Details</Text>
            <CardElement options={options}/>
            <div style={{marginTop: 30, width: "100%", textAlign: "center"}}>
                <Button type="submit" colorPalette="blue" size="lg">Save card</Button>
                <p>{status}</p>
            </div>
            
        </form>
    );

}